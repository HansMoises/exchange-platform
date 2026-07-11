# Deployment.md
# Plataforma Inteligente de Intercambio de Objetos

> **Documento:** Estrategia de Despliegue
> **Fase SDLC:** 2 (Diseño) — documento de proceso / DevOps; base para Fase 5
> **Versión:** 1.4.0
> **Estado:** `PENDIENTE DE APROBACIÓN`
> **Fecha:** 2026-07-10
> **Autor:** Equipo Enterprise Senior (Especialista DevOps / Arquitecto)
> **Documentos padre:** Docker.md | CICD.md | BD.md | Arquitectura.md
> **Convenciones:** Documentación en español. Diagramas en ASCII.

---

## Control de Versiones

| Versión | Fecha      | Autor                    | Cambios          |
|---------|------------|--------------------------|------------------|
| 1.0.0   | 2026-06-03 | Equipo Enterprise Senior | Versión inicial. |
| 1.1.0   | 2026-07-08 | Equipo Enterprise Senior | Adaptado a Supabase (PostgreSQL gestionado) en lugar de SQL Server en contenedor propio. Ver ADR-010 en `Arquitectura.md`. |
| 1.2.0   | 2026-07-09 | Equipo Enterprise Senior | Despliegue movido de VPS Hostinger (Docker Compose) a **Vercel (frontend) + Render (backend)**. Ver ADR-011 en `Arquitectura.md`. Procedimiento de despliegue, migraciones y rollback actualizados a los mecanismos nativos de cada plataforma. |
| 1.3.0   | 2026-07-10 | Equipo Enterprise Senior | Migraciones ahora se aplican **automáticamente al arrancar el backend** (`Program.cs`, flag `RunMigrationsAtStartup`), eliminando el paso manual como prerrequisito. Sección 4 actualizada. |
| 1.4.0   | 2026-07-10 | Equipo Enterprise Senior | Conexión a Supabase: se recomienda el **Session pooler (5432)** para el backend persistente y se documenta el **endurecimiento del pool Npgsql** (KeepAlive, ConnectionIdleLifetime, reintentos) que corrige 500 intermitentes por conexiones muertas reutilizadas. Prerrequisitos y sección 4 actualizados. |

---

## Tabla de Contenidos

1. Ambientes
2. Prerrequisitos
3. Procedimiento de Despliegue
4. Migraciones de Base de Datos
5. Smoke Tests (Verificación Post-Despliegue)
6. Estrategia de Rollback
7. Monitoreo y Observabilidad
8. Backup y Recuperación
9. Checklist "Antes de Desplegar"
10. Aprobación

---

## 1. Ambientes

| Ambiente    | Propósito                                     | Frontend / Backend           | Proyecto Supabase (BD)     |
|-------------|-----------------------------------------------|-------------------------------|------------------------------|
| Development | Desarrollo local. Debug, Swagger, hot reload. | Local (Vite dev server / dotnet run) | exchange-platform-dev        |
| Testing     | Pruebas automatizadas en CI.                  | Pipeline (GitHub Actions)    | exchange-platform-test (o BD efímera en el runner de CI) |
| Staging     | Validación pre-producción. Espejo de prod.    | Vercel (preview) + Render (staging service) | exchange-platform-staging    |
| Production  | Sistema en uso real. Sin debug ni Swagger.    | Vercel (producción) + Render (producción) | exchange-platform-prod       |

Cada ambiente tiene su propia configuración, variables de entorno y su **propio proyecto Supabase** (aislamiento total — Supabase aísla por proyecto, no por base de datos dentro de una misma instancia).

> **Nota sobre Testing:** para pruebas de integración en CI se recomienda usar Testcontainers con PostgreSQL en Docker (no Supabase real), evitando consumir cuota del proyecto Supabase y garantizando pruebas reproducibles y aisladas por ejecución.

---

## 2. Prerrequisitos

Antes de cualquier despliegue:

```
□ Pipeline CI/CD en verde (build, tests, cobertura >= 90%, seguridad).
□ Versión etiquetada (SemVer) en caso de Producción.
□ Variables de entorno del ambiente configuradas en Vercel y Render (dashboards de cada plataforma / GitHub Secrets para CI), incluido el
  CONNECTION_STRING de Supabase correspondiente al ambiente. Para el backend persistente usar el **Session pooler (puerto 5432)**, NO el
  Transaction pooler (6543): este último rota conexiones entre clientes y provoca 500 intermitentes con un servicio ASP.NET Core que
  mantiene su propio pool Npgsql.
□ Proyecto Supabase del ambiente activo (no pausado por inactividad).
□ Servicio de Render del ambiente activo (no dormido — ver §7 y `keep-alive-supabase.yml` en CICD.md).
□ Backup reciente de la base de datos (en Staging/Producción).
□ Checklist "Antes de Desplegar" completo (sección 9).
□ Aprobación manual para Producción (revisión de PR a `main` — ver GitFlow.md).
```

Infraestructura mínima: cuentas de **Vercel** (frontend) y **Render** (backend) conectadas al repositorio GitHub (ADR-011, Arquitectura.md). No se requiere Docker Compose ni gestión de host en Staging/Producción — Docker Compose se conserva solo para desarrollo local (Docker.md). La base de datos no requiere infraestructura propia — es el proyecto Supabase del ambiente correspondiente.

---

## 3. Procedimiento de Despliegue

```
┌────────────────────────────────────────────────────────┐
│  1. Obtener artefacto                                  │
│     - Merge/tag del commit aprobado en la rama de      │
│       despliegue (develop → Staging, main → Prod).     │
│                                                        │
│  2. CI valida (GitHub Actions)                          │
│     - build + test + cobertura + seguridad (CICD.md).  │
│                                                        │
│  3. Deploy automático (Vercel + Render)                │
│     - Ambas plataformas detectan el push y despliegan  │
│       por su cuenta — sin comando manual (ADR-011).     │
│                                                        │
│  4. Migraciones de BD (automáticas al arrancar)         │
│     - Program.cs aplica las migraciones pendientes en   │
│       el arranque del backend (sección 4), antes de     │
│       atender tráfico.                                  │
│                                                        │
│  5. Verificar (smoke tests)                            │
│     - sección 5. Si fallan → rollback (sección 6).     │
│                                                        │
│  6. Confirmar despliegue                               │
│     - Notificar versión desplegada. Activar monitoreo. │
└────────────────────────────────────────────────────────┘
```

### 3.1 Disparadores por Ambiente

```
# Staging
push/merge a `develop` → Vercel despliega preview/staging del frontend
                        → Render despliega el servicio de staging del backend

# Production
push de tag vX.Y.Z / merge a `main` → Vercel despliega producción del frontend
                                     → Render despliega producción del backend
```

> Vercel y Render están conectados directamente al repositorio (GitHub App / Deploy Hooks); no se ejecuta `docker compose up` en ningún host. El pipeline CD (`CICD.md`) sigue siendo el gate de calidad (build, test, cobertura, seguridad) antes de que el código llegue a esas ramas; el propio proveedor hace el build de la imagen (Render, con el Dockerfile de `backend/`) o del bundle estático (Vercel, con `npm run build`).

---

## 4. Migraciones de Base de Datos

| Aspecto              | Decisión                                                                 |
|----------------------|--------------------------------------------------------------------------|
| Herramienta          | Entity Framework Core Migrations (Code First) + proveedor Npgsql.        |
| Cuándo               | Automáticamente al arrancar cada nueva versión del backend (antes de atender tráfico). |
| Mecanismo            | `Program.cs` llama a `db.Database.Migrate()` en el arranque, aplicando toda migración pendiente contra el Supabase del ambiente (cadena en `ConnectionStrings__DefaultConnection`). |
| Interruptor          | Config `RunMigrationsAtStartup` (default `true`). Las pruebas de integración lo ponen en `false` porque migran su propio contenedor Testcontainers. |
| Reversibilidad       | Migraciones con Down() para poder revertir (ver Opción manual abajo).    |
| Seed inicial         | Datos maestros UBIGEO + roles + categorías (DatosGeograficos.md, BD.md). |
| Regla                | Nunca editar una migración ya aplicada en Producción; crear una nueva.   |
| Conexión             | Se aplican vía conexión **directa** (no el pooler en modo *transaction*), ya que el DDL requiere sesión persistente. |

```
# Opción A (por defecto): automática al arrancar el backend.
# No requiere acción manual — Render arranca el contenedor y Program.cs aplica
# las migraciones pendientes usando ConnectionStrings__DefaultConnection.
# Para desactivarla en un ambiente concreto: variable RunMigrationsAtStartup=false.

# Opción B: manual, apuntando directo al proyecto Supabase del ambiente
# (util para revertir, o para aplicar migraciones sin redesplegar).
# Usar la conexión directa, no el pooler en modo transaction.
dotnet ef database update --connection "Host=db.[ref].supabase.co;Port=5432;Database=postgres;Username=postgres;Password=<...>;SSL Mode=Require"

# Revertir a una migración previa (si es necesario)
dotnet ef database update NombreMigracionAnterior --connection "<misma cadena>"
```

> El auto-migrate corre dentro del mismo contenedor del backend, con las mismas variables de entorno ya configuradas en Render — no expone credenciales adicionales. Como `Migrate()` es idempotente, arrancar varias veces no reaplica migraciones ya presentes.

### 4.1 Resiliencia de la conexión (pool Npgsql)

Supabase (y su pooler) cierran conexiones inactivas sin avisar al cliente. Un backend persistente que mantiene su propio pool Npgsql puede reutilizar una conexión ya muerta: la lectura se cuelga hasta el *Command Timeout* (~30 s) y devuelve **500 de forma intermitente** (síntoma observado: la misma consulta alternaba `200 500 200 500`). `AddInfrastructure` (Infrastructure) endurece el pool para evitarlo:

| Ajuste                     | Valor | Propósito                                                          |
|----------------------------|-------|--------------------------------------------------------------------|
| `KeepAlive` / `TcpKeepAlive` | 30 s / on | Sondea la conexión y detecta/descarta las muertas.             |
| `ConnectionIdleLifetime`   | 60 s  | Cierra conexiones ociosas del pool antes de que el servidor las mate. |
| `ConnectionPruningInterval`| 10 s  | Frecuencia de limpieza de conexiones ociosas.                      |
| `Timeout` / `CommandTimeout` | 15 s / 30 s | Acotan la espera para fallar rápido en vez de colgarse.      |
| `EnableRetryOnFailure`     | 3 reintentos | Reintenta errores transitorios en una conexión nueva.        |

Esto complementa —no reemplaza— el uso del **Session pooler (5432)** de la sección 2. Ambos juntos eliminan los 500 intermitentes.

---

## 5. Smoke Tests (Verificación Post-Despliegue)

Pruebas mínimas que confirman que el sistema está operativo tras desplegar.

| # | Verificación                                  | Resultado esperado        |
|---|-----------------------------------------------|---------------------------|
| 1 | GET /health                                   | 200 OK                    |
| 2 | GET /api/v1/geo/departamentos                 | 200 + datos UBIGEO        |
| 3 | POST /api/v1/auth/login (usuario de prueba)   | 200 + tokens (en Staging) |
| 4 | Frontend carga (GET / )                       | 200 + SPA visible         |
| 5 | Conexión backend ↔ BD                         | Sin errores en logs       |

Si algún smoke test crítico falla → se activa el **rollback** (sección 6).

---

## 6. Estrategia de Rollback

```
Disparador: smoke test crítico falla o error grave detectado tras desplegar.

Pasos:
  1. Volver a la versión anterior estable:
        - Frontend (Vercel): "Rollback" al deployment anterior desde el
          dashboard de Vercel (o el CLI `vercel rollback`) — instantáneo,
          no requiere rebuild.
        - Backend (Render): "Rollback" al deploy anterior desde el dashboard
          de Render (mantiene historial de deploys), o revertir el commit/tag
          en `main` y dejar que Render redespliegue automáticamente.
  2. Si hubo migración con cambios incompatibles:
        - Revertir la migración (dotnet ef database update <previa>), o
        - Restaurar el backup previo al despliegue (BD.md §12).
  3. Verificar con smoke tests que la versión anterior opera.
  4. Notificar y registrar el incidente.
  5. Analizar la causa antes de reintentar (post-mortem).

Objetivo (RTO): restaurar servicio en < 2 horas (BD.md §12.2).
```

> El historial de deployments de Vercel y Render hace el rollback más simple que con Docker Compose: ambas plataformas conservan versiones anteriores listas para reactivar con un clic, sin necesidad de reconstruir imágenes.

---

## 7. Monitoreo y Observabilidad

| Aspecto    | Implementación MVP                  | Futuro (V2+)                       |
|------------|-------------------------------------|------------------------------------|
| Logs       | Serilog estructurado → stdout, capturado por el log stream de Render (Docker Compose + volumen logs solo en desarrollo local) | Elastic Stack / Seq                |
| Métricas   | Logs de tiempos de respuesta        | Prometheus + Grafana               |
| Errores    | Nivel Error/Critical en Serilog     | Application Insights / Sentry      |
| Salud      | Endpoint /health                    | Health dashboard + alertas         |
| Auditoría  | AuditLog en BD (Seguridad.md)       | Reportes de auditoría              |

Niveles de log: Information, Warning, Error, Critical. Sin datos sensibles en logs (Seguridad.md).

> **Nota — plan gratuito de Render:** el servicio backend se suspende tras ~15 min sin tráfico; la primera petición tras inactividad puede tardar 30–60s (cold start) mientras el contenedor arranca. `keep-alive-supabase.yml` (CICD.md) hace ping periódico para reducir la frecuencia de este escenario, aunque no lo elimina por completo dentro del plan Free.

---

## 8. Backup y Recuperación

Coherente con `BD.md` §12. La base de datos es un servicio gestionado (Supabase); los backups automáticos dependen del plan contratado, complementados con `pg_dump` manual.

| Acción                        | Cuándo                                                      |
|--------------------------------|--------------------------------------------------------------|
| Backup pre-despliegue          | `pg_dump` manual antes de cada despliegue a Producción.      |
| Backup automático (Supabase)   | Según plan (diario en Free/Pro).                              |
| Backup manual complementario   | Semanal, almacenado fuera de Supabase.                        |
| Prueba de restauración         | Periódica (validar que el backup/`pg_dump` sirve para restaurar). |

DRP: ante pérdida de datos, restaurar desde el backup automático de Supabase o desde el `pg_dump` más reciente. RTO objetivo < 2 horas (< 30 min si el plan de Supabase incluye Point-in-Time Recovery).

---

## 9. Checklist "Antes de Desplegar"

```
□ Build exitoso en CI.
□ Todas las pruebas pasan.
□ Cobertura mínima alcanzada (>= 90%).
□ Vulnerabilidades revisadas (sin críticas).
□ Variables de entorno del ambiente configuradas.
□ Backups configurados y backup pre-despliegue tomado.
□ Migraciones de BD revisadas y reversibles.
□ Logs y monitoreo configurados.
□ Documentación actualizada.
□ Aprobación manual (Producción).
```

> Este checklist es el gate de salida de la Fase 5 (Despliegue). Coincide con el de `ChecklistCalidad.md`.

---

## 10. Aprobación

| Rol                    | Nombre            | Aprobación  | Fecha |
|------------------------|-------------------|-------------|-------|
| Especialista DevOps    | Equipo Enterprise | ☐ PENDIENTE | —     |
| Arquitecto de Software | Equipo Enterprise | ☐ PENDIENTE | —     |
| QA Senior              | Equipo Enterprise | ☐ PENDIENTE | —     |
| DBA Senior             | Equipo Enterprise | ☐ PENDIENTE | —     |

---

> **GATE DE CALIDAD — FASE 5 (Despliegue):**
> Ningún despliegue a Producción procede sin el checklist completo, backup previo
> y aprobación manual. El rollback debe estar siempre disponible.

---

*Documento generado bajo la metodología SDD — Plataforma Inteligente de Intercambio de Objetos — Ayacucho, Perú.*
