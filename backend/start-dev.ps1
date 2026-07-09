<#
    start-dev.ps1
    Levanta el entorno de desarrollo completo en el orden correcto:
      1. Base de datos PostgreSQL (contenedor Docker)
      2. API .NET  (https://localhost:7149)
      3. Frontend Vite (http://localhost:5173)

    Este script vive dentro del repo Backend y asume que el repo 'frontend'
    esta como carpeta hermana (mismo directorio padre):
        source\repos\
          |- Backend\   <- aqui esta este script
          |- frontend\

    Uso:   .\start-dev.ps1
    Parar: cierra las ventanas de API y Frontend (o Ctrl+C en cada una),
           y si quieres detener la BD:  docker stop exchange-dev-db
#>

$ErrorActionPreference = 'Stop'

$Root        = $PSScriptRoot                                   # ...\Backend
$ApiDir      = Join-Path $Root 'src\ExchangePlatform.API'
$FrontDir    = Join-Path (Split-Path $Root -Parent) 'frontend' # ...\frontend (hermano)
$DbContainer = 'exchange-dev-db'
$HealthUrl   = 'https://localhost:7149/health'

function Write-Step($msg) { Write-Host "`n==> $msg" -ForegroundColor Cyan }

if (-not (Test-Path $FrontDir)) {
    Write-Host "AVISO: no encontre el frontend en '$FrontDir'." -ForegroundColor Yellow
    Write-Host "       Ajusta \$FrontDir en este script si tu repo esta en otra ruta." -ForegroundColor Yellow
}

# --- 1. Base de datos -------------------------------------------------------
Write-Step "1/3  Base de datos ($DbContainer)"

try { docker info *> $null } catch {
    Write-Host "ERROR: Docker Desktop no responde. Abrelo y vuelve a ejecutar." -ForegroundColor Red
    exit 1
}

$running = (docker ps      --filter "name=$DbContainer" --format '{{.Names}}')
$exists  = (docker ps -a   --filter "name=$DbContainer" --format '{{.Names}}')

if ($running -eq $DbContainer) {
    Write-Host "    Ya esta corriendo." -ForegroundColor Green
} elseif ($exists -eq $DbContainer) {
    docker start $DbContainer | Out-Null
    Write-Host "    Contenedor iniciado." -ForegroundColor Green
} else {
    Write-Host "    No existe el contenedor. Creandolo (postgres:15-alpine, volumen persistente)..." -ForegroundColor Yellow
    docker run -d --name $DbContainer `
        -e POSTGRES_PASSWORD=postgres `
        -e POSTGRES_DB=exchange_platform `
        -p 5432:5432 `
        -v exchange_dev_pgdata:/var/lib/postgresql/data `
        postgres:15-alpine | Out-Null
    Write-Host "    Contenedor creado." -ForegroundColor Green
}

# Esperar a que Postgres acepte conexiones
Write-Host "    Esperando a que Postgres este listo..." -NoNewline
for ($i = 0; $i -lt 30; $i++) {
    $ok = $false
    try { docker exec $DbContainer pg_isready -U postgres *> $null; if ($LASTEXITCODE -eq 0) { $ok = $true } } catch {}
    if ($ok) { break }
    Start-Sleep -Seconds 1
    Write-Host "." -NoNewline
}
Write-Host " listo." -ForegroundColor Green

# --- 2. API .NET ------------------------------------------------------------
Write-Step "2/3  API .NET (aplica migraciones al arrancar)"
Start-Process powershell -ArgumentList @(
    '-NoExit', '-Command',
    "Set-Location '$ApiDir'; `$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run --launch-profile https"
)
Write-Host "    API arrancando en ventana nueva -> $HealthUrl" -ForegroundColor Green

# Esperar a que la API responda Healthy antes de abrir el frontend
Write-Host "    Esperando health check" -NoNewline
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }
for ($i = 0; $i -lt 60; $i++) {
    try {
        $r = Invoke-WebRequest -Uri $HealthUrl -UseBasicParsing -TimeoutSec 3
        if ($r.StatusCode -eq 200) { break }
    } catch {}
    Start-Sleep -Seconds 2
    Write-Host "." -NoNewline
}
Write-Host " OK." -ForegroundColor Green

# --- 3. Frontend Vite -------------------------------------------------------
Write-Step "3/3  Frontend Vite"
if (Test-Path $FrontDir) {
    Start-Process powershell -ArgumentList @(
        '-NoExit', '-Command',
        "Set-Location '$FrontDir'; npm run dev"
    )
    Write-Host "    Frontend arrancando en ventana nueva -> http://localhost:5173" -ForegroundColor Green
} else {
    Write-Host "    Omitido (no se encontro el directorio del frontend)." -ForegroundColor Yellow
}

Write-Step "Entorno levantado."
Write-Host @"
    BD       : contenedor Docker '$DbContainer' (puerto 5432)
    API      : https://localhost:7149   (ventana propia)
    Frontend : http://localhost:5173    (ventana propia)

    Abre http://localhost:5173 en tu navegador.
    Para detener: cierra las dos ventanas nuevas. La BD sigue viva
    (docker stop $DbContainer para apagarla).
"@ -ForegroundColor Gray
