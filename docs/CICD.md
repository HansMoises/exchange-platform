# CICD.md
# Plataforma Inteligente de Intercambio de Objetos

> **Documento:** Integración y Entrega Continua (CI/CD)
> **Fase SDLC:** 2 (Diseño) — documento de proceso / DevOps
> **Versión:** 1.2.0
> **Estado:** `PENDIENTE DE APROBACIÓN`
> **Fecha:** 2026-07-09
> **Autor:** Equipo Enterprise Senior (Especialista DevOps / QA)
> **Documentos padre:** GitFlow.md | Docker.md | Testing.md | Seguridad.md
> **Convenciones:** Documentación en español. Diagramas en ASCII. Herramienta: GitHub Actions.

---

## Control de Versiones

| Versión | Fecha      | Autor                    | Cambios          |
|---------|------------|--------------------------|------------------|
| 1.0.0   | 2026-06-03 | Equipo Enterprise Senior | Versión inicial. |
| 1.1.0   | 2026-07-08 | Equipo Enterprise Senior | Gestión de secretos y despliegue adaptados a Supabase (PostgreSQL gestionado) en lugar de SQL Server propio. Se agrega workflow `keep-alive-supabase.yml` para evitar la pausa del proyecto Supabase (plan Free) por inactividad. Ver ADR-010 en `Arquitectura.md`. |
| 1.2.0   | 2026-07-09 | Equipo Enterprise Senior | Etapa de Deploy adaptada a Vercel + Render (integración nativa con GitHub, sin `docker compose up` por SSH). Ver ADR-011 en `Arquitectura.md`. Workflow `cd-prod.yml` simplificado (ya no construye/despliega imágenes manualmente); gestión de secretos actualizada. |

---

## Tabla de Contenidos

1. Objetivos
2. Herramienta y Estructura
3. Etapas del Pipeline
4. Disparadores por Rama
5. Quality Gates
6. Workflow de Ejemplo (CI)
7. Workflow de Despliegue (CD)
8. Gestión de Secretos
9. Notificaciones y Monitoreo del Pipeline
10. Aprobación

---

## 1. Objetivos

| Objetivo                | Cómo se logra                                            |
|-------------------------|----------------------------------------------------------|
| Detección temprana      | Build + pruebas en cada push/PR.                         |
| Calidad garantizada     | Quality gate: cobertura ≥ 90% (Testing.md).              |
| Seguridad continua      | Escaneo de dependencias y análisis (Seguridad.md).       |
| Despliegue reproducible | Backend: misma imagen Docker en todos los ambientes (Render la construye desde el mismo Dockerfile). Frontend: mismo build de Vite en Vercel. |
| Trazabilidad            | Cada despliegue ligado a un commit/tag.                  |
| Velocidad con control   | Automatización con gates que protegen producción.        |

---

## 2. Herramienta y Estructura

Herramienta: **GitHub Actions**. Los workflows viven en `.github/workflows/`.

```
.github/workflows/
  ├── ci.yml                    ← Integración: build + test + quality (PR y push)
  ├── cd-staging.yml            ← Despliegue a Staging (merge a develop / release)
  ├── cd-prod.yml                ← Despliegue a Producción (merge/tag en main)
  └── keep-alive-supabase.yml   ← Ping periódico a /health para evitar pausa del proyecto Supabase (plan Free)
```

---

## 3. Etapas del Pipeline

```
   ┌─────────┐   ┌─────────┐   ┌──────────┐   ┌──────────┐   ┌──────────┐   ┌─────────┐
   │ 1.BUILD │──►│ 2.TEST  │──►│3.QUALITY │──►│4.SECURITY│──►│5.DOCKER  │──►│6.DEPLOY │
   └─────────┘   └─────────┘   └──────────┘   └──────────┘   └──────────┘   └─────────┘
   compila      unitarias +   cobertura      escaneo de    construye      despliega
   back+front   integración   >= 90%         dependencias  imágenes       al ambiente
```

| Etapa       | Acción                                                        | Falla si...                       |
|-------------|---------------------------------------------------------------|-----------------------------------|
| 1. Build    | dotnet build + npm run build                                  | No compila.                       |
| 2. Test     | dotnet test (xUnit) + vitest                                  | Alguna prueba falla.              |
| 3. Quality  | Cobertura (Coverlet/Vitest)                                   | Cobertura < 90%.                  |
| 4. Security | Escaneo de dependencias (Dependabot / audit)                  | Vulnerabilidad crítica.           |
| 5. Docker   | docker build backend (imagen validada; Render construye la suya propia al detectar el push) | La imagen no construye.           |
| 6. Deploy   | Vercel y Render despliegan automáticamente al detectar el push (sin paso manual)              | El despliegue o smoke test falla. |

---

## 4. Disparadores por Rama

Coherente con `GitFlow.md`.

| Evento                       | Workflow      | Etapas ejecutadas                 | Despliegue   |
|------------------------------|---------------|-----------------------------------|--------------|
| Push a feature/*             | ci.yml        | Build + Test                      | —            |
| Pull Request → develop       | ci.yml        | Build + Test + Quality + Security | —            |
| Merge a develop              | cd-staging.yml| Todas                             | Staging      |
| Push de tag vX.Y.Z en main   | cd-prod.yml   | Todas                             | Production   |

---

## 5. Quality Gates

El pipeline **se detiene** si no se cumple un gate:

| Gate                 | Criterio                         | Documento           |
|----------------------|----------------------------------|---------------------|
| Build                | Compilación exitosa back + front | —                   |
| Pruebas              | 100% de pruebas pasan            | Testing.md          |
| Cobertura            | ≥ 90% global                     | Testing.md PR-080   |
| Seguridad            | Sin vulnerabilidades críticas    | Seguridad.md PR-081 |
| Revisión de código   | PR aprobado (mín. 1 revisor)     | GitFlow.md          |

Un PR no se puede mergear a `develop`/`main` si algún gate falla (Branch Protection + checks requeridos).

---

## 6. Workflow de Ejemplo (CI)

```yaml
# .github/workflows/ci.yml
name: CI

on:
  push:
    branches: [ "feature/**", "bugfix/**" ]
  pull_request:
    branches: [ "develop", "main" ]

jobs:
  backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - name: Restore
        run: dotnet restore ./backend
      - name: Build
        run: dotnet build ./backend --no-restore -c Release
      - name: Test + Coverage
        run: dotnet test ./backend --no-build -c Release --collect:"XPlat Code Coverage"
      - name: Verify Coverage >= 90%
        run: ./scripts/check-coverage.sh 90

  frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup Node
        uses: actions/setup-node@v4
        with:
          node-version: '20'
      - name: Install
        run: npm ci --prefix ./frontend
      - name: Build
        run: npm run build --prefix ./frontend
      - name: Test
        run: npm run test --prefix ./frontend

  security:
    runs-on: ubuntu-latest
    needs: [ backend, frontend ]
    steps:
      - uses: actions/checkout@v4
      - name: Audit dependencias (npm)
        run: npm audit --audit-level=critical --prefix ./frontend
      # Dependabot cubre el escaneo continuo de dependencias.
```

---

## 7. Workflow de Despliegue (CD)

Con Vercel y Render conectados directamente al repositorio (ADR-011), el despliegue en sí lo dispara cada plataforma al detectar el push — GitHub Actions ya no construye ni levanta contenedores en producción. El workflow `cd-prod.yml` conserva el rol de **gate de calidad** y de **verificación post-despliegue**:

```yaml
# .github/workflows/cd-prod.yml
name: CD Production

on:
  push:
    tags: [ "v*.*.*" ]   # se dispara al etiquetar una release en main

jobs:
  quality-gate:
    runs-on: ubuntu-latest
    environment: production    # requiere aprobación manual (GitHub Environments)
    steps:
      - uses: actions/checkout@v4
      - name: Verificar build backend
        run: dotnet build ./backend -c Release
      - name: Verificar build frontend
        run: |
          npm ci --prefix ./frontend
          npm run build --prefix ./frontend
      # Vercel y Render ya iniciaron su propio deploy al detectar el push del tag/main.
      # Este job actúa como gate de aprobación manual antes de confirmar el release.

  wait-and-verify:
    runs-on: ubuntu-latest
    needs: quality-gate
    steps:
      - name: Esperar a que Render/Vercel completen el deploy
        run: sleep 60   # margen simple; alternativa: consultar la API de Render/Vercel por estado "live"
      - name: Smoke tests
        run: ./scripts/smoke-tests.sh
```

> El ambiente `production` usa **GitHub Environments** con aprobación manual obligatoria antes de que el job corra (control humano sobre producción). Si `smoke-tests.sh` falla, se activa el rollback nativo de Vercel/Render (Deployment.md §6) en vez de un `docker compose down`.

---

## 8. Gestión de Secretos

| Secreto              | Dónde se guarda                        | Uso                                                    |
|----------------------|------------------------------------------|---------------------------------------------------------|
| CONNECTION_STRING    | GitHub Secrets + variables de entorno en Render (por ambiente) | Cadena de conexión Npgsql al pooler de Supabase.        |
| SUPABASE_DB_PASSWORD | GitHub Secrets + Render (por ambiente)  | Contraseña de la BD del proyecto Supabase.              |
| JWT_SECRET           | GitHub Secrets + Render                 | Firma de tokens.                                        |
| VITE_API_URL         | Variables de entorno en Vercel (por ambiente) | URL pública del backend en Render, consumida por el frontend. |
| ALLOWED_ORIGINS      | Render (por ambiente)                   | Dominio de Vercel permitido para CORS.                  |
| Credenciales deploy  | Integración nativa GitHub ↔ Vercel/Render (App/Deploy Hooks) | Autoriza a cada plataforma a leer el repo y desplegar. |
| BACKEND_HEALTH_URL   | GitHub Secrets                          | URL `/health` del backend en Render, usada por `keep-alive-supabase.yml`. |

Reglas: nunca en el repositorio ni en logs; inyectados como variables en tiempo de ejecución (en GitHub Secrets para CI, y en los dashboards de Vercel/Render para cada servicio); rotación periódica (Seguridad.md).

---

## 8bis. Keep-Alive del Proyecto Supabase y del Servicio Render

El plan Free de Supabase pausa el proyecto tras ~1 semana de inactividad, y el plan Free de Render suspende el servicio backend tras ~15 min sin tráfico (ADR-011). El mismo workflow programado (`keep-alive-supabase.yml`) hace `ping` al endpoint `GET /health` del backend en Render cada 3 días, lo que de paso mantiene activa la conexión a Supabase (evitando su pausa por inactividad) entre sesiones de desarrollo o antes de una demo/sustentación.

> Este ping cada 3 días **no** evita el sleep de Render por inactividad de 15 minutos (para eso se necesitaría un ping cada pocos minutos, lo cual excede el uso razonable del plan gratuito). Su propósito principal es evitar que el proyecto Supabase se pause; como beneficio adicional, si se ejecuta poco antes de una demo, también "despierta" el backend en Render con antelación.

```yaml
# .github/workflows/keep-alive-supabase.yml
name: Keep-Alive Supabase

on:
  schedule:
    - cron: "0 8 */3 * *"   # cada 3 días, 08:00 UTC
  workflow_dispatch: {}

jobs:
  ping:
    runs-on: ubuntu-latest
    steps:
      - name: Ping backend /health (Render)
        run: |
          response=$(curl -s -o /dev/null -w "%{http_code}" "${{ secrets.BACKEND_HEALTH_URL }}")
          echo "Código de respuesta: $response"
          if [ "$response" -ne 200 ]; then
            echo "::warning::El endpoint /health no respondió 200 (respondió $response)."
          fi
```

> Requiere el secreto `BACKEND_HEALTH_URL` (ej. `https://tu-servicio.onrender.com/health`) configurado en GitHub Secrets, apuntando al backend desplegado en Render. Si aún no hay backend desplegado públicamente, se puede usar como alternativa temporal un ping directo al REST de Supabase con `SUPABASE_URL` y `SUPABASE_ANON_KEY`.
>
> **Antes de una sustentación o demo:** ejecutar manualmente el workflow (`workflow_dispatch`) unos minutos antes, para asegurar que tanto Render como Supabase respondan sin el retraso del cold start.

---

## 9. Notificaciones y Monitoreo del Pipeline

| Evento                   | Notificación                              |
|--------------------------|-------------------------------------------|
| Pipeline falla           | Aviso al autor del PR / canal del equipo. |
| Cobertura insuficiente   | Comentario automático en el PR.           |
| Despliegue exitoso       | Notificación con versión desplegada.      |
| Vulnerabilidad detectada | Alerta de Dependabot + bloqueo del merge. |

El estado del pipeline (badge) se muestra en el `README.md`.

---

## 10. Aprobación

| Rol                    | Nombre            | Aprobación  | Fecha |
|------------------------|-------------------|-------------|-------|
| Especialista DevOps    | Equipo Enterprise | ☐ PENDIENTE | —     |
| QA Senior              | Equipo Enterprise | ☐ PENDIENTE | —     |
| Arquitecto de Software | Equipo Enterprise | ☐ PENDIENTE | —     |

---

> **GATE DE CALIDAD:**
> Ningún código llega a producción sin pasar todas las etapas del pipeline.
> El despliegue a producción requiere aprobación manual. Detalle de ambientes en `Deployment.md`.

---

*Documento generado bajo la metodología SDD — Plataforma Inteligente de Intercambio de Objetos — Ayacucho, Perú.*
