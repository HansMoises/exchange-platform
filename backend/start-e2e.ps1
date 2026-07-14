<#
    start-e2e.ps1
    Levanta el entorno AISLADO para pruebas End-To-End con Playwright.

    Diferencias con start-dev.ps1:
      - BD: contenedor 'exchange-db-test' (puerto 5433), NO el de desarrollo.
      - La connection string se inyecta por variable de entorno, sobrescribiendo
        cualquier appsettings.json.
      - NUNCA toca Supabase (produccion) ni exchange-dev-db (desarrollo).

    Uso:   .\backend\start-e2e.ps1
#>

$ErrorActionPreference = 'Stop'

$Root        = Split-Path $PSScriptRoot -Parent          # ...\exchange-platform
$ApiDir      = Join-Path $Root 'backend\src\ExchangePlatform.API'
$FrontDir    = Join-Path $Root 'frontend'
$DbContainer = 'exchange-db-test'
$HealthUrl   = 'https://localhost:7149/health'

$TestConn = 'Host=localhost;Port=5433;Database=exchange_test;Username=postgres;Password=TestE2E2026;Timeout=30;Command Timeout=30;Pooling=true'

function Write-Step($msg) { Write-Host "`n==> $msg" -ForegroundColor Cyan }

# --- 1. Verificar BD de pruebas --------------------------------------------
Write-Step "1/3  Base de datos de pruebas ($DbContainer, puerto 5433)"

try { docker info *> $null } catch {
    Write-Host "ERROR: Docker Desktop no responde. Abrelo y reintenta." -ForegroundColor Red
    exit 1
}

$running = (docker ps --filter "name=$DbContainer" --format '{{.Names}}')
if ($running -ne $DbContainer) {
    Write-Host "    No esta corriendo. Levantando con docker compose..." -ForegroundColor Yellow
    Push-Location $Root
    docker compose -f docker-compose.test.yml up -d | Out-Null
    Pop-Location
    Start-Sleep -Seconds 5
}
Write-Host "    BD de pruebas lista." -ForegroundColor Green

# --- 2. API .NET apuntando al 5433 -----------------------------------------
Write-Step "2/3  API .NET (BD: exchange_test @ 5433)"
Start-Process powershell -ArgumentList @(
    '-NoExit', '-Command',
    "Set-Location '$ApiDir'; " +
    "`$env:ConnectionStrings__DefaultConnection='$TestConn'; " +
    "`$env:ASPNETCORE_ENVIRONMENT='Development'; " +
    "Write-Host 'BD -> puerto 5433 (exchange_test)' -ForegroundColor Magenta; " +
    "dotnet run --launch-profile https"
)
Write-Host "    API arrancando -> $HealthUrl" -ForegroundColor Green

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
Start-Process powershell -ArgumentList @(
    '-NoExit', '-Command',
    "Set-Location '$FrontDir'; npm run dev"
)
Write-Host "    Frontend arrancando -> http://localhost:5173" -ForegroundColor Green

Write-Step "Entorno E2E levantado."
Write-Host @"
    BD       : exchange-db-test  (puerto 5433)  <- AISLADA
    API      : https://localhost:7149
    Frontend : http://localhost:5173

    Produccion (Supabase) y desarrollo (5432) NO se tocan.
    Para detener: cierra las dos ventanas nuevas.
"@ -ForegroundColor Gray