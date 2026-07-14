# Docker.md
# Plataforma Inteligente de Intercambio de Objetos

> **Documento:** Contenedorización (Docker)
> **Fase SDLC:** 2 (Diseño) — documento de proceso / DevOps
> **Versión:** 1.3.0
> **Estado:** `PENDIENTE DE APROBACIÓN`
> **Fecha:** 2026-07-13
> **Autor:** Equipo Enterprise Senior (Especialista DevOps / Arquitecto)
> **Documentos padre:** Arquitectura.md | BD.md | Seguridad.md
> **Convenciones:** Documentación en español. Diagramas en ASCII.

---

## Control de Versiones

| Versión | Fecha      | Autor                    | Cambios          |
|---------|------------|--------------------------|------------------|
| 1.0.0   | 2026-06-03 | Equipo Enterprise Senior | Versión inicial. |
| 1.1.0   | 2026-07-08 | Equipo Enterprise Senior | Se elimina el contenedor `db` (SQL Server). La base de datos pasa a ser Supabase (PostgreSQL gestionado externo). Ver ADR-010 en `Arquitectura.md`. |
| 1.2.0   | 2026-07-09 | Equipo Enterprise Senior | Docker Compose deja de usarse para desplegar en Staging/Producción (ahora Vercel + Render, ver ADR-011 en `Arquitectura.md`) y se conserva **solo para desarrollo local**. Se elimina el contenedor `proxy` (Nginx). El Dockerfile del backend se mantiene sin cambios — es el que consume Render para construir el servicio. |
| 1.3.0   | 2026-07-13 | Equipo Enterprise Senior | **Se revierte parcialmente el principio "BD fuera de Docker"** (ver ADR-012 en `Arquitectura.md`). Se documentan los **dos contenedores PostgreSQL** que ya operan: `exchange-dev-db` (`:5432`, desarrollo local) y `exchange-db-test` (`:5433`, suite E2E, definido en `docker-compose.test.yml`). Supabase queda restringido al ambiente de **Producción**. La BD de test pasa a ser **efímera (`tmpfs`)** — corrige el defecto DEF-01 / PR-097 de `Testing.md` (acumulación de datos entre ejecuciones). |

---

## Tabla de Contenidos

1. Estrategia de Contenedorización
2. Arquitectura de Contenedores
3. Dockerfile — Backend
4. Dockerfile — Frontend
5. Docker Compose
6. Redes y Volúmenes
7. Variables de Entorno
8. Healthchecks
9. Comandos Operativos
10. Aprobación

---

## 1. Estrategia de Contenedorización

| Principio                  | Aplicación                                                  |
|----------------------------|-------------------------------------------------------------|
| Alcance de Docker           | **Solo desarrollo local** (Docker Compose). En Staging/Producción, el frontend se despliega en Vercel (sin Docker) y el backend en Render (usando este mismo Dockerfile) — ver ADR-011. |
| Un servicio por contenedor | Frontend y Backend en contenedores separados (desarrollo local). |
| Base de datos por ambiente | **Aislamiento en tres niveles (ADR-012):** Producción → Supabase (externo, fuera de Docker) · Desarrollo → contenedor `exchange-dev-db` (`localhost:5432`) · Test → contenedor `exchange-db-test` (`localhost:5433`, efímero). Ningún proceso local escribe en Supabase. |
| Imágenes ligeras           | Multi-stage builds; imágenes runtime mínimas.               |
| Reproducibilidad           | Misma imagen de backend en Dev local, Testing (CI) y Render (Staging/Prod). |
| Configuración externa      | Variables de entorno por ambiente (sin secretos en imagen). |
| Persistencia               | Volumen para logs solo en desarrollo local. En Render, los logs van a stdout (Deployment.md §7). |
| Cloud-ready                | El Dockerfile del backend es portable a Azure/AWS/GCP/cualquier VPS o PaaS con soporte Docker sin cambios (ADR-009, ADR-011). |

---

## 2. Arquitectura de Contenedores

### 2.1 Desarrollo Local (Docker Compose)

```
┌──────────────────────────────────────────────────────────────────────┐
│                   Docker (máquina local del dev)                     │
│                                                                      │
│  ┌──────────────┐        ┌──────────────────┐                        │
│  │   frontend   │        │     backend      │                        │
│  │ (Nginx+SPA)  │───────►│  (ASP.NET Core)  │                        │
│  │   :3000      │        │     :5000        │                        │
│  └──────────────┘        └────────┬─────────┘                        │
│                                   │                                  │
│                                   │ ConnectionStrings__              │
│                                   │ DefaultConnection                │
│                                   ▼                                  │
│                          ┌─────────────────────┐                     │
│                          │  exchange-dev-db    │  ◄── N2 DESARROLLO  │
│                          │  postgres:15-alpine │                     │
│                          │  :5432 (persistente)│                     │
│                          │  DB: exchange_platform                    │
│                          └─────────────────────┘                     │
│                                                                      │
│  Red: exchange-network          Volumen: logs-data, pgdata-dev       │
└──────────────────────────────────────────────────────────────────────┘

        ┌──────────────────────────────────────────────────┐
        │              docker-compose.test.yml             │
        │        (se levanta SOLO durante el E2E)          │
        │                                                  │
        │            ┌─────────────────────┐               │
        │            │  exchange-db-test   │ ◄── N3 TEST   │
        │            │  postgres:15-alpine │               │
        │            │  :5433  (tmpfs/RAM) │               │
        │            │  DB: exchange_test  │               │
        │            └─────────────────────┘               │
        │            Se destruye al terminar la suite.     │
        └──────────────────────────────────────────────────┘

                            ✗  NUNCA
                            ✗
        ┌───────────────────────────────┐
        │   Supabase (servicio externo) │  ◄── N1 PRODUCCIÓN
        │   PostgreSQL 15+ gestionado   │      ⛔ Ningún proceso local
        │   Pooler :6543 (PgBouncer)    │         se conecta aquí.
        └───────────────────────────────┘
```

> **Cambio respecto a v1.2.0 (ADR-012):** en desarrollo local el backend **ya no se conecta a
> Supabase**, sino al contenedor `exchange-dev-db`. Supabase queda reservado exclusivamente
> para el ambiente de Producción (Vercel + Render, §2.2).

### 2.2 Staging / Producción (Vercel + Render — sin Docker Compose)

```
┌───────────────────┐        ┌──────────────────────┐
│   Vercel           │        │   Render              │
│   (build Vite,     │─HTTPS─►│   (imagen del         │
│    sin Docker)     │        │    Dockerfile §3)      │
│   Frontend :443     │        │   Backend :443         │
└───────────────────┘        └──────────┬─────────────┘
                                          │ TLS :5432 (pooler)
                                          ▼
                              ┌───────────────────────────────┐
                              │   Supabase (servicio externo) │
                              │   PostgreSQL 15+ gestionado   │
                              │   Connection Pooler (PgBouncer)│
                              └───────────────────────────────┘
```

> No hay contenedor `proxy` (Nginx) en producción — Vercel y Render terminan TLS cada uno de forma independiente (ADR-011). Render construye el servicio backend **a partir del mismo Dockerfile de la sección 3**, por lo que la imagen que corre en Render es equivalente a la que corre en desarrollo local. Vercel no usa el Dockerfile de frontend (sección 4): construye directo con `npm run build` sobre su propia infraestructura — ese Dockerfile queda vigente solo para levantar el frontend en Docker Compose local.
>
> La BD **no vive dentro de Docker Compose ni de Render/Vercel**. El backend se conecta a Supabase por internet (TLS) usando el *connection pooler*, tanto en desarrollo local como en Render.

---

## 3. Dockerfile — Backend

Multi-stage build (.NET 10): compilación en una etapa, runtime mínimo en otra. **Este es el mismo Dockerfile que usa Render** para construir el servicio backend en Staging/Producción (ADR-011) — no requiere cambios entre desarrollo local y despliegue.

```dockerfile
# /backend/Dockerfile

# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar (capa cacheable)
COPY ["src/ExchangePlatform.API/ExchangePlatform.API.csproj", "ExchangePlatform.API/"]
COPY ["src/ExchangePlatform.Application/ExchangePlatform.Application.csproj", "ExchangePlatform.Application/"]
COPY ["src/ExchangePlatform.Domain/ExchangePlatform.Domain.csproj", "ExchangePlatform.Domain/"]
COPY ["src/ExchangePlatform.Infrastructure/ExchangePlatform.Infrastructure.csproj", "ExchangePlatform.Infrastructure/"]
RUN dotnet restore "ExchangePlatform.API/ExchangePlatform.API.csproj"

# Copiar el resto y publicar
COPY src/ .
RUN dotnet publish "ExchangePlatform.API/ExchangePlatform.API.csproj" \
    -c Release -o /app/publish --no-restore

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Usuario no root (seguridad)
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "ExchangePlatform.API.dll"]
```

---

## 4. Dockerfile — Frontend

Build de Vite servido por Nginx. **Uso: solo desarrollo local vía Docker Compose.** En Staging/Producción, Vercel construye el frontend directamente con `npm run build` sobre su propia infraestructura (sin este Dockerfile ni Nginx) — ver ADR-011.

```dockerfile
# /frontend/Dockerfile

# Etapa 1: Build
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

# Etapa 2: Servir con Nginx
FROM nginx:alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 3000
CMD ["nginx", "-g", "daemon off;"]
```

`nginx.conf` (resumen): sirve la SPA y reenvía rutas desconocidas a `index.html` (SPA routing).

```
server {
    listen 3000;
    root /usr/share/nginx/html;
    location / {
        try_files $uri $uri/ /index.html;
    }
}
```

---

## 5. Docker Compose

> **Alcance:** este `docker-compose.yml` y sus overrides se usan **solo para desarrollo local**. Staging y Producción se despliegan en Vercel (frontend) + Render (backend), sin Docker Compose (ADR-011, Deployment.md).

```yaml
# docker-compose.yml (base — desarrollo local)
services:

  db:
    image: postgres:15-alpine
    container_name: exchange-dev-db
    environment:
      POSTGRES_USER: ${DEV_DB_USER:-postgres}
      POSTGRES_PASSWORD: ${DEV_DB_PASSWORD:-postgres}
      POSTGRES_DB: ${DEV_DB_NAME:-exchange_platform}
    ports:
      - "5432:5432"
    volumes:
      - pgdata-dev:/var/lib/postgresql/data    # persistente: los datos de dev sobreviven
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres -d exchange_platform"]
      interval: 5s
      timeout: 3s
      retries: 10
    networks:
      - exchange-network

  backend:
    build:
      context: ./backend
      dockerfile: Dockerfile
    container_name: exchange-backend
    environment:
      ASPNETCORE_ENVIRONMENT: ${ASPNETCORE_ENVIRONMENT}
      # ADR-012: en desarrollo apunta al contenedor `db`, NO a Supabase.
      ConnectionStrings__DefaultConnection: "Host=db;Port=5432;Database=exchange_platform;Username=postgres;Password=${DEV_DB_PASSWORD:-postgres}"
      Jwt__Secret: ${JWT_SECRET}
    ports:
      - "5000:5000"
    depends_on:
      db:
        condition: service_healthy
    networks:
      - exchange-network
    volumes:
      - logs-data:/app/logs

  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile
    container_name: exchange-frontend
    ports:
      - "3000:3000"
    depends_on:
      - backend
    networks:
      - exchange-network

networks:
  exchange-network:
    driver: bridge

volumes:
  logs-data:
  pgdata-dev:
```

> **Con servicio `db` (cambio v1.3.0 — ADR-012):** a diferencia de la versión anterior de este
> documento, el compose de desarrollo **sí incluye** un contenedor PostgreSQL (`exchange-dev-db`).
> El backend local se conecta a él, **no a Supabase**. El motivo: el desarrollo diario escribía
> usuarios, objetos e intercambios de prueba directamente en la base de datos de producción,
> contaminando los listados y los indicadores del panel de administración (RF-111) que se
> exhibirán en la sustentación. Supabase queda reservado exclusivamente para Producción.

---

## 5.2 `docker-compose.test.yml` — Base de datos efímera para E2E

> **Nivel 3 de ADR-012.** Contenedor exclusivo de la suite E2E de Playwright (`Testing.md` §13).
> Se levanta al inicio de la ejecución y se destruye al terminar.

```yaml
# docker-compose.test.yml
# Base de datos EFÍMERA para la suite E2E (Playwright).
# Nivel 3 de ADR-012 — Arquitectura.md
#
# ⚠️ SIN VOLUMEN PERSISTENTE, POR DISEÑO.
#    Cada ejecución arranca con una BD limpia. Esto garantiza pruebas
#    deterministas y elimina la acumulación de datos entre corridas
#    (defecto DEF-01 / PR-097 en Testing.md).

services:

  db-test:
    image: postgres:15-alpine
    container_name: exchange-db-test
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: ${TEST_DB_PASSWORD:-TestE2E2026}
      POSTGRES_DB: exchange_test
    ports:
      - "5433:5432"          # host 5433 → contenedor 5432 (evita choque con dev)
    tmpfs:
      - /var/lib/postgresql/data   # datos en RAM: se evaporan al detener el contenedor
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres -d exchange_test"]
      interval: 3s
      timeout: 3s
      retries: 10
    networks:
      - exchange-test-network

networks:
  exchange-test-network:
    driver: bridge

# SIN sección `volumes:` — deliberadamente.
```

### 5.2.1 Por qué `tmpfs` y no un volumen

| Estrategia                   | Datos entre ejecuciones | Velocidad | DEF-01     |
|------------------------------|-------------------------|-----------|------------|
| Volumen nombrado (v1.2.0)    | ❌ Persisten            | Normal    | **Falla**  |
| Sin volumen                  | ✅ Se borran al `down`  | Normal    | Resuelto   |
| **`tmpfs`** *(adoptado)*     | ✅ Imposible persistir  | **Mayor** | Resuelto   |

`tmpfs` monta el directorio de datos de PostgreSQL en **memoria RAM**. Los datos no pueden
sobrevivir a la detención del contenedor — no es una convención, es una imposibilidad física.
Beneficio secundario: se elimina la E/S a disco, reduciendo el tiempo de la suite E2E.

> **Origen del defecto DEF-01:** el volumen `exchange-platform_pgdata-test` conservaba los
> objetos creados en cada corrida. Tras 20 ejecuciones, la aserción `toHaveCount(1)` de
> `search.spec.ts` recibía `20`. El síntoma (`Expected: 1, Received: 20`) era literalmente el
> contador de ejecuciones acumuladas.

### 5.2.2 Migración desde la configuración anterior

El volumen persistente ya existe en disco y **debe destruirse una sola vez**:

```powershell
# Destruye contenedor Y volumen (la bandera -v es indispensable)
docker compose -f docker-compose.test.yml down -v
```

> `docker compose down` sin `-v` detiene el contenedor pero **conserva** el volumen. Es esa
> bandera la que separa el defecto de su solución.

### 5.2.3 Tabla comparativa de los dos contenedores

| Parámetro           | `exchange-dev-db` (N2)   | `exchange-db-test` (N3) |
|---------------------|--------------------------|--------------------------|
| Archivo compose     | `docker-compose.yml`     | `docker-compose.test.yml`|
| Imagen              | `postgres:15-alpine`     | `postgres:15-alpine`     |
| Puerto host         | `5432`                   | `5433`                   |
| Base de datos       | `exchange_platform`      | `exchange_test`          |
| Usuario             | `postgres`               | `postgres`               |
| Almacenamiento      | Volumen `pgdata-dev`     | **`tmpfs` (RAM)**        |
| Persistencia        | Sí                       | **No** (por diseño)      |
| Ciclo de vida       | Permanente               | Por ejecución de la suite|
| Consumidor          | `dotnet run` local       | Playwright (`start-e2e.ps1`) |

> **Doble capa de defensa:** además del puerto distinto, el **nombre de la base de datos difiere**
> (`exchange_platform` vs `exchange_test`). Si por error se apunta al puerto equivocado, la base
> de datos de destino no coincide y el fallo es inmediato y visible, en lugar de silencioso.

### 5.3 Archivos Compose Disponibles

| Archivo                     | Ambiente        | Contenedores                              | Se aplica            |
|-----------------------------|-----------------|-------------------------------------------|----------------------|
| `docker-compose.yml`        | Desarrollo (N2) | `frontend`, `backend`, `db` (`:5432`)     | `docker compose up`  |
| `docker-compose.override.yml` | Desarrollo    | Hot reload, Swagger, debug                 | Automático           |
| `docker-compose.test.yml`   | **Test (N3)**   | `db-test` (`:5433`, efímero)              | `start-e2e.ps1` / `-f` |

> Los antiguos `docker-compose.staging.yml` y `docker-compose.prod.yml` se retiran: Staging y Producción ya no se orquestan con Docker Compose. La configuración de esos ambientes vive en los dashboards de Vercel/Render y en GitHub Secrets (Deployment.md §2, §8.4 de Arquitectura.md).

Ejecución:

```powershell
# Desarrollo local (N2) — frontend + backend + BD de desarrollo
docker compose up -d

# Suite E2E (N3) — solo la BD de test, efímera
docker compose -f docker-compose.test.yml up -d

# Destruir la BD de test al terminar (-v elimina cualquier volumen residual)
docker compose -f docker-compose.test.yml down -v
```

> En la práctica, el ciclo de vida del contenedor de test lo gestiona el script
> `start-e2e.ps1` (ver `Testing.md` §13). No es necesario invocarlo manualmente.

---

## 6. Redes y Volúmenes

### 6.1 Redes

| Red               | Tipo   | Propósito                                       |
|-------------------|--------|-------------------------------------------------|
| exchange-network  | bridge | Comunicación interna entre frontend y backend.  |

El backend habla con Supabase por internet vía TLS (fuera de `exchange-network`), usando el *connection pooler*. Ya no hay base de datos local que exponer o proteger dentro de la red Docker.

### 6.2 Volúmenes

| Volumen         | Contenedor | Propósito                           |
|-----------------|------------|-------------------------------------|
| logs-data       | backend    | Persistencia de logs (Serilog).     |

> No existe volumen de datos de BD en este host — la persistencia de datos la gestiona Supabase.

---

## 7. Variables de Entorno

Definidas en archivos `.env` por ambiente (nunca en el repositorio — Seguridad.md).

```
# .env (ejemplo, valores reales fuera del repo)
ASPNETCORE_ENVIRONMENT=Development
CONNECTION_STRING=Host=aws-0-[region].pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.[ref];Password=<...>;SSL Mode=Require;Trust Server Certificate=true
JWT_SECRET=<secreto-generado>
JWT_ISSUER=exchange-platform
JWT_AUDIENCE=exchange-platform-users
ACCESS_TOKEN_EXPIRY_MINUTES=15
REFRESH_TOKEN_EXPIRY_DAYS=7
MAX_FILE_SIZE_MB=5
ALLOWED_ORIGINS=http://localhost:3000
```

> `CONNECTION_STRING` usa el formato de cadena de Npgsql y apunta al **connection pooler** de Supabase (puerto 5432 en modo *session* o 6543 en modo *transaction*, según el caso de uso), no a una conexión directa a Postgres. Los valores `[region]` y `[ref]` (referencia del proyecto) se obtienen del dashboard de Supabase → Project Settings → Database.

`.env` está en `.gitignore`. Se versiona un `.env.example` sin valores reales.

---

## 8. Healthchecks

| Servicio | Healthcheck                                       | Propósito                                                    |
|----------|---------------------------------------------------|----------------------------------------------------------------|
| backend  | Endpoint GET /health (200 OK)                     | Verifica que la API responde y que la conexión a Supabase funciona (el endpoint hace un ping simple a la BD). Usado por Render (health check nativo del servicio) y por `keep-alive-supabase.yml` (CICD.md). |
| frontend | Nginx responde en :3000 (solo desarrollo local)   | Verifica que la SPA se sirve localmente. En Vercel, la disponibilidad la gestiona la propia plataforma (CDN). |

Al no correr la BD en este host, ya no hay `depends_on: db (service_healthy)`. El endpoint `/health` del backend debe incluir una verificación activa de conectividad a Supabase (ej. `SELECT 1` vía EF Core) para que el healthcheck sea representativo, tanto en desarrollo local como en Render.

---

## 9. Comandos Operativos

> Los siguientes comandos aplican a **desarrollo local**. Para operaciones en Staging/Producción (Render/Vercel), ver Deployment.md.

```
# Construir e iniciar todo (local)
docker compose up -d --build

# Ver estado de contenedores
docker compose ps

# Ver logs (seguir)
docker compose logs -f backend

# Ejecutar migraciones EF Core dentro del contenedor backend (local)
docker compose exec backend dotnet ef database update

# Detener y eliminar contenedores (conserva volúmenes)
docker compose down

# Detener y eliminar TODO incluyendo volúmenes (cuidado: borra datos)
docker compose down -v

# Reconstruir solo un servicio
docker compose up -d --build backend

# Backup de la base de datos (ya no corre en este host — ver BD.md §12.1)
pg_dump "postgresql://postgres.[ref]:[password]@aws-0-[region].pooler.supabase.com:5432/postgres" \
  --format=custom --file="backup_$(date +%Y%m%d).dump"
```

---

## 10. Aprobación

| Rol                    | Nombre            | Aprobación  | Fecha |
|------------------------|-------------------|-------------|-------|
| Especialista DevOps    | Equipo Enterprise | ☐ PENDIENTE | —     |
| Arquitecto de Software | Equipo Enterprise | ☐ PENDIENTE | —     |
| DBA Senior             | Equipo Enterprise | ☐ PENDIENTE | —     |

---

> **GATE DE CALIDAD:**
> Los contenedores deben construir y arrancar correctamente en CI antes de desplegar.
> El pipeline que construye estas imágenes se define en `CICD.md`; el despliegue en `Deployment.md`.

---

*Documento generado bajo la metodología SDD — Plataforma Inteligente de Intercambio de Objetos — Ayacucho, Perú.*
