# Docker.md
# Plataforma Inteligente de Intercambio de Objetos

> **Documento:** Contenedorización (Docker)
> **Fase SDLC:** 2 (Diseño) — documento de proceso / DevOps
> **Versión:** 1.2.0
> **Estado:** `PENDIENTE DE APROBACIÓN`
> **Fecha:** 2026-07-09
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
| Base de datos gestionada   | PostgreSQL corre en Supabase (servicio externo, fuera de Docker Compose). |
| Imágenes ligeras           | Multi-stage builds; imágenes runtime mínimas.               |
| Reproducibilidad           | Misma imagen de backend en Dev local, Testing (CI) y Render (Staging/Prod). |
| Configuración externa      | Variables de entorno por ambiente (sin secretos en imagen). |
| Persistencia               | Volumen para logs solo en desarrollo local. En Render, los logs van a stdout (Deployment.md §7). |
| Cloud-ready                | El Dockerfile del backend es portable a Azure/AWS/GCP/cualquier VPS o PaaS con soporte Docker sin cambios (ADR-009, ADR-011). |

---

## 2. Arquitectura de Contenedores

### 2.1 Desarrollo Local (Docker Compose)

```
┌────────────────────────────────────────────────────────────────┐
│                Docker Compose (máquina local del dev)          │
│                                                                │
│  ┌──────────────┐              ┌──────────────────┐             │
│  │   frontend   │              │     backend      │             │
│  │ (Nginx+SPA)  │              │  (ASP.NET Core)  │             │
│  │   :3000      │              │     :5000        │             │
│  └──────────────┘              └────────┬─────────┘             │
│                                          │                       │
│  Red: exchange-network     Volumen: logs-data                   │
└──────────────────────────────────────────┼───────────────────────┘
                                            │ TLS :5432 (pooler)
                                            ▼
                              ┌───────────────────────────────┐
                              │   Supabase (servicio externo) │
                              │   PostgreSQL 15+ gestionado   │
                              │   Connection Pooler (PgBouncer)│
                              └───────────────────────────────┘
```

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

  backend:
    build:
      context: ./backend
      dockerfile: Dockerfile
    container_name: exchange-backend
    environment:
      ASPNETCORE_ENVIRONMENT: ${ASPNETCORE_ENVIRONMENT}
      CONNECTION_STRING: ${CONNECTION_STRING}   # apunta al pooler de Supabase
      JWT_SECRET: ${JWT_SECRET}
    ports:
      - "5000:5000"
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
```

> **Sin servicio `db`:** la base de datos vive en Supabase, fuera de este `docker-compose.yml`. El backend se conecta directamente por `CONNECTION_STRING` a la URL del pooler de Supabase. Ya no hay `depends_on: db` ni healthcheck local de base de datos — la disponibilidad de la BD se verifica a través del propio healthcheck del backend (sección 8).

### 5.1 Overrides Disponibles

| Archivo                          | Uso                                                                |
|----------------------------------|---------------------------------------------------------------------|
| docker-compose.yml               | Base común (desarrollo local).                                      |
| docker-compose.override.yml      | Development (hot reload, Swagger, debug) — se aplica automáticamente. |

> Los antiguos `docker-compose.staging.yml` y `docker-compose.prod.yml` se retiran: Staging y Producción ya no se orquestan con Docker Compose. La configuración de esos ambientes vive en los dashboards de Vercel/Render y en GitHub Secrets (Deployment.md §2, §8.4 de Arquitectura.md).

Ejecución (desarrollo local):

```
docker compose up -d
```

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
