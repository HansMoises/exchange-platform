# Arquitectura.md
# Plataforma Inteligente de Intercambio de Objetos

> **Documento:** Arquitectura del Sistema
> **Paso SDD:** 6 de 8 (Arquitectura) — **Fase SDLC:** 2 (Diseño)
> **Versión:** 1.5.0
> **Estado:** `PENDIENTE DE APROBACIÓN`
> **Fecha:** 2026-07-14
> **Autor:** Equipo Enterprise Senior (Arquitecto de Software / Soluciones / Empresarial / Datos / Seguridad / DevOps)
> **Documentos padre:** VisionProyecto.md | Requisitos.md | ReglasNegocio.md | CasosDeUso.md | HistoriasUsuario.md | MatrizTrazabilidad.md | UML.md
> **Convenciones:** Documentación en español; código/entidades en español (Usuario, Objeto, Intercambio). Diagramas en ASCII. Decisiones registradas como ADR.

---

## Control de Versiones

| Versión | Fecha      | Autor                    | Cambios                                                                 |
|---------|------------|--------------------------|-------------------------------------------------------------------------|
| 1.0.0   | 2026-06-03 | Equipo Enterprise Senior | Versión inicial.                                                        |
| 1.1.0   | 2026-06-03 | Equipo Enterprise Senior | Estilo Clean Architecture en capas; mapeo atributos→RNF; ADR ampliados. |
| 1.2.0   | 2026-07-08 | Equipo Enterprise Senior | Migración de motor de BD: SQL Server 2022 (contenedor propio) → PostgreSQL 15+ en Supabase (gestionado externo). Ver ADR-010. Diagramas C4, flujo CQRS, despliegue y escalabilidad actualizados. ADR-002 marcado como Reemplazado. |
| 1.3.0   | 2026-07-09 | Equipo Enterprise Senior | Despliegue de frontend y backend movido de VPS Hostinger (Docker Compose + Nginx proxy) a **Vercel (frontend) + Render (backend)**, plan gratuito. Ver ADR-011. Diagrama de flujo general, sección 8 (Arquitectura de Despliegue), escalabilidad y trazabilidad actualizados. ADR-009 marcado como Vigente (parcial) — Docker Compose se conserva solo para desarrollo local. |
| 1.4.0   | 2026-07-13 | Equipo Enterprise Senior | Se añade **ADR-012 — Aislamiento de bases de datos en tres niveles** (Producción/Supabase · Desarrollo/Docker `:5432` · Test/Docker `:5433`), motivado por la implementación de la suite E2E (Playwright, ver `Testing.md` v1.2.0). Modifica parcialmente **ADR-009** y **ADR-010**: el principio *"la BD corre en Supabase, fuera de Docker"* deja de ser cierto para los ambientes de Desarrollo y Test. ADR-010 marcado como *Modificado parcialmente*. Obliga a cascada sobre `Docker.md`, `Deployment.md`, `CICD.md`, `Testing.md`, `Riesgos.md` y `README.md`. |
| 1.5.0   | 2026-07-14 | Equipo Enterprise Senior | Se añade **ADR-013 — Almacenamiento de imágenes en Supabase Storage**, en reemplazo del almacenamiento en disco local, que se perdía en el filesystem efímero de Render (ADR-011). Nueva implementación `AlmacenamientoSupabaseService` tras `IAlmacenamientoService`; el disco local queda solo para Desarrollo/Test. Corrige la descripción «genérico/local» del almacenamiento y la nomenclatura `FileStorageService` → `IAlmacenamientoService` (diagrama de componentes, árbol de carpetas, escalabilidad y trazabilidad). Obliga a cascada sobre `Backend.md`, `Deployment.md`, `BD.md` y `README.md`. |

---

## Tabla de Contenidos

1. Visión General de la Arquitectura
2. Modelo C4
3. Arquitectura Backend — Clean Architecture
4. Arquitectura Frontend — Feature Based
5. Arquitectura de Datos
6. Arquitectura de Seguridad
7. Arquitectura de Integración
8. Arquitectura de Despliegue
9. Patrones Aplicados
10. Antipatrones Evitados
11. Architecture Decision Records (ADR)
12. Principios de Diseño Aplicados
13. Atributos de Calidad (mapeo a RNF)
14. Estrategia de Escalabilidad
15. Trazabilidad y Aprobación

---

## 1. Visión General de la Arquitectura

### 1.1 Estilo Arquitectónico

La plataforma adopta una **arquitectura en capas basada en Clean Architecture** sobre un único despliegue de backend (.NET 10), con frontend SPA desacoplado (React + TypeScript, Feature Based). El diseño aísla el dominio del negocio de los detalles de infraestructura, lo que permite mantenibilidad, testabilidad y evolución tecnológica. La separación por capas y los bounded contexts internos dejan abierta una futura extracción de módulos si el crecimiento lo exigiera, pero **el estilo declarado y soportado es Clean Architecture en capas** (ver ADR-001).

### 1.2 Flujo General del Sistema

```
┌────────────────────────────────────────────────────────────────┐
│                        CLIENTE (Browser)                       │
│              React + TypeScript + Vite + TailwindCSS           │
└────────────────────────────┬───────────────────────────────────┘
                             │ HTTPS
              ┌──────────────┴──────────────┐
              ▼                             │
┌───────────────────────────┐               │ HTTPS — REST API (JSON)
│   Vercel (Frontend)       │               │
│   CDN + SSL automático    │               │
│   React App (estático)    │               │
└────────────────────────────┘               ▼
                              ┌───────────────────────────┐
                              │   Render (Backend)        │
                              │   SSL automático           │
                              │  ASP.NET Core Web API     │
                              │       .NET 10             │
                              │                           │
                              │  ┌─────────────────────┐  │
                              │  │   Capa API          │  │
                              │  │   Controllers       │  │
                              │  │   Middlewares       │  │
                              │  └──────────┬──────────┘  │
                              │             │             │
                              │  ┌──────────▼──────────┐  │
                              │  │ Capa Application    │  │
                              │  │ Commands / Queries  │  │
                              │  │ Handlers / MediatR  │  │
                              │  └──────────┬──────────┘  │
                              │             │             │
                              │  ┌──────────▼──────────┐  │
                              │  │   Capa Domain       │  │
                              │  │   Entidades / Reglas│  │
                              │  │   Value Objects     │  │
                              │  └──────────┬──────────┘  │
                              │             │             │
                              │  ┌──────────▼──────────┐  │
                              │  │ Capa Infrastructure │  │
                              │  │ EF Core / Repos     │  │
                              │  │ JWT / Serilog       │  │
                              │  └──────────┬──────────┘  │
                              └─────────────┼─────────────┘
                                            │ TCP :5432 (pooler, TLS)
                                            ▼
                              ┌─────────────────────────┐
                              │  PostgreSQL 15+          │
                              │  (Supabase — gestionado, │
                              │   servicio externo)      │
                              └─────────────────────────┘
```

> Vercel y Render terminan TLS cada uno de forma independiente (sin proxy Nginx intermedio — ver ADR-011). El frontend en Vercel consume la API de Render vía HTTPS directo, usando la URL pública del backend configurada en las variables de entorno del frontend.

### 1.3 Principio Rector

> **"La documentación es la fuente de verdad. El código es una consecuencia del diseño aprobado, nunca al revés."**

---

## 2. Modelo C4

### 2.1 Nivel 1 — Contexto del Sistema

Muestra cómo el sistema interactúa con usuarios y sistemas externos.

```
┌─────────────────────────────────────────────────────────────────────┐
│                        CONTEXTO DEL SISTEMA                         │
│                                                                     │
│   [Usuario]          [Moderador]       [Administrador]              │
│       │                   │                  │                      │
│       └───────────────────┴──────────────────┘                      │
│                           │                                         │
│                           ▼                                         │
│         ┌─────────────────────────────────────┐                     │
│         │  Plataforma Inteligente de          │                     │
│         │  Intercambio de Objetos             │                     │
│         │                                     │                     │
│         │  Plataforma web para gestión        │                     │
│         │  segura y trazable de intercambios  │                     │
│         │  de objetos entre ciudadanos.       │                     │
│         └──────────────────┬──────────────────┘                     │
│                            │                                        │
│              ┌─────────────┴──────────────┐                         │
│              │                            │                         │
│              ▼                            ▼                         │
│   [Servicio de Correo]        [Servicio de Mapas]                   │
│   (SMTP / SendGrid)           (Google Maps / Leaflet)               │
│   Futuro — V2                 Futuro — V2                           │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### 2.2 Nivel 2 — Contenedores

```
┌─────────────────────────────────────────────────────────────────────┐
│                          CONTENEDORES                               │
│                                                                     │
│  [Browser del Usuario]                                              │
│       │                                                             │
│       │ HTTPS                                                       │
│       ▼                                                             │
│  ┌─────────────────┐        ┌──────────────────────────────────┐    │
│  │  SPA Frontend   │        │      REST API Backend            │    │
│  │                 │◄──────►│                                  │    │
│  │  React + TS     │  JSON  │  ASP.NET Core Web API (.NET 10)  │    │
│  │  Vite + Tailwind│  HTTP  │  Puerto: 5000                    │    │
│  │  Puerto: 3000   │        │                                  │    │
│  └─────────────────┘        └──────────────┬───────────────────┘    │
│                                            │                        │
│                                            │ EF Core (Npgsql) / TLS :5432 │
│                                            ▼                        │
│                             ┌──────────────────────────┐            │
│                             │     Base de Datos        │            │
│                             │  PostgreSQL 15+ (Supabase)│            │
│                             │  Gestionado — externo    │            │
│                             └──────────────────────────┘            │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### 2.3 Nivel 3 — Componentes del Backend

```
┌─────────────────────────────────────────────────────────────────────┐
│                    COMPONENTES — BACKEND                            │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │                        CAPA API                              │   │
│  │   AuthController  │ UsuariosController │ ObjetosController   │   │
│  │IntercambiosController │ AdminController │ NotificacionesCtrl │   │
│  │  ─────────────────────────────────────────────────────────   │   │
│  │  JwtMiddleware │ ExceptionMiddleware │ RateLimitMiddleware   │   │
│  │  AuditMiddleware │ CorrelationIdMiddleware                   │   │
│  └─────────────────────────────┬────────────────────────────────┘   │
│                                │ MediatR                            │
│  ┌─────────────────────────────▼────────────────────────────────┐   │
│  │                    CAPA APPLICATION                          │   │
│  │  Commands: CrearUsuario │ CrearObjeto │ CrearIntercambio     │   │
│  │            AceptarIntercambio │ ConfirmarIntercambio         │   │
│  │            CrearCalificacion                                 │   │
│  │  Queries:  ObtenerObjetos │ ObtenerIntercambios              │   │
│  │            ObtenerUsuario │ ObtenerDashboard                 │   │
│  │  ─────────────────────────────────────────────────────────   │   │
│  │  FluentValidation Validators (por Command/Query)             │   │
│  │  AutoMapper Profiles                                         │   │
│  │  MediatR Pipeline Behaviors (Validation │ Logging │ Audit)   │   │
│  └─────────────────────────────┬────────────────────────────────┘   │
│                                │ Interfaces (DIP)                   │
│  ┌─────────────────────────────▼────────────────────────────────┐   │
│  │                      CAPA DOMAIN                             │   │
│  │  Entidades: Usuario │ Objeto │ Intercambio │ Calificacion    │   │
│  │             Mensaje │ Notificacion │ Reporte │ Favorito      │   │
│  │  Enums: EstadoObjeto │ EstadoIntercambio │ TipoNotificacion  │   │
│  │  Value Objects: Email │ PasswordHash │ Ubicacion             │   │
│  │  Domain Events: SolicitudCreada │ IntercambioAceptado        │   │
│  │                 IntercambioCompletado │ CalificacionEmitida  │   │
│  │  Interfaces: IUsuarioRepository │ IObjetoRepository          │   │
│  │              IIntercambioRepository │ IUnitOfWork            │   │
│  └─────────────────────────────┬────────────────────────────────┘   │
│                                │ Implementaciones                   │
│  ┌─────────────────────────────▼────────────────────────────────┐   │
│  │                  CAPA INFRASTRUCTURE                         │   │
│  │  EF Core DbContext (ExchangePlatformDbContext)               │   │
│  │  Repositories: UsuarioRepository │ ObjetoRepository          │   │
│  │               IntercambioRepository │ CalificacionRepository │   │
│  │                NotificacionRepository                        │   │
│  │  UnitOfWork: envuelve DbContext.SaveChangesAsync()           │   │
│  │  Services: JwtService │ PasswordHasher │ AlmacenamientoService│   │
│  │            NotificationService │ AuditService                │   │
│  │  Serilog (logging estructurado)                              │   │
│  │  EF Core Migrations                                          │   │
│  └──────────────────────────────────────────────────────────────┘   │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### 2.4 Nivel 4 — Código

Se materializa en la implementación (Fase 3), gobernada por `Convenciones.md`. Cada Command/Query tiene su carpeta con Command, Handler y Validator; cada entidad su configuración EF Core.

---

## 3. Arquitectura Backend — Clean Architecture

### 3.1 Estructura de Solución

```
/backend
  src/
  │
  ├── ExchangePlatform.Domain/
  │     ├── Entities/
  │     │     ├── BaseEntity.cs
  │     │     ├── Usuario.cs
  │     │     ├── Objeto.cs
  │     │     ├── Intercambio.cs
  │     │     ├── Calificacion.cs
  │     │     ├── Mensaje.cs
  │     │     ├── Notificacion.cs
  │     │     ├── Reporte.cs
  │     │     ├── Favorito.cs
  │     │     ├── RefreshToken.cs
  │     │     ├── AuditLog.cs
  │     │     └── Geo/
  │     │           ├── Departamento.cs
  │     │           ├── Provincia.cs
  │     │           └── Distrito.cs
  │     ├── Enums/
  │     │     ├── EstadoObjeto.cs
  │     │     ├── EstadoIntercambio.cs
  │     │     ├── TipoNotificacion.cs
  │     │     └── EstadoReporte.cs
  │     ├── ValueObjects/
  │     │     ├── Email.cs
  │     │     └── Ubicacion.cs
  │     ├── Events/
  │     │     ├── SolicitudIntercambioCreada.cs
  │     │     ├── IntercambioAceptado.cs
  │     │     ├── IntercambioCompletado.cs
  │     │     └── CalificacionEmitida.cs
  │     └── Interfaces/
  │           ├── Repositories/
  │           │     ├── IUsuarioRepository.cs
  │           │     ├── IObjetoRepository.cs
  │           │     ├── IIntercambioRepository.cs
  │           │     ├── ICalificacionRepository.cs
  │           │     ├── INotificacionRepository.cs
  │           │     └── IGenericRepository.cs
  │           └── IUnitOfWork.cs
  │
  ├── ExchangePlatform.Application/
  │     ├── Commands/
  │     │     ├── Auth/
  │     │     │     ├── RegistrarUsuario/
  │     │     │     │     ├── RegistrarUsuarioCommand.cs
  │     │     │     │     ├── RegistrarUsuarioCommandHandler.cs
  │     │     │     │     └── RegistrarUsuarioCommandValidator.cs
  │     │     │     └── IniciarSesion/
  │     │     │           ├── IniciarSesionCommand.cs
  │     │     │           ├── IniciarSesionCommandHandler.cs
  │     │     │           └── IniciarSesionCommandValidator.cs
  │     │     ├── Objetos/
  │     │     │     ├── CrearObjeto/
  │     │     │     ├── ActualizarObjeto/
  │     │     │     └── EliminarObjeto/
  │     │     └── Intercambios/
  │     │           ├── CrearIntercambio/
  │     │           ├── AceptarIntercambio/
  │     │           ├── RechazarIntercambio/
  │     │           ├── ConfirmarIntercambio/
  │     │           └── CancelarIntercambio/
  │     ├── Queries/
  │     │     ├── Objetos/
  │     │     │     ├── ObtenerObjetos/
  │     │     │     │     ├── ObtenerObjetosQuery.cs
  │     │     │     │     ├── ObtenerObjetosQueryHandler.cs
  │     │     │     │     └── ObtenerObjetosQueryValidator.cs
  │     │     │     └── ObtenerObjetoPorId/
  │     │     ├── Usuarios/
  │     │     ├── Intercambios/
  │     │     └── Admin/
  │     ├── DTOs/
  │     │     ├── Auth/
  │     │     ├── Usuarios/
  │     │     ├── Objetos/
  │     │     ├── Intercambios/
  │     │     └── Common/
  │     │           ├── ApiResponse.cs
  │     │           └── PagedResult.cs
  │     ├── Behaviors/
  │     │     ├── ValidationBehavior.cs
  │     │     ├── LoggingBehavior.cs
  │     │     └── AuditBehavior.cs
  │     └── Mappings/
  │           └── MappingProfiles.cs
  │
  ├── ExchangePlatform.Infrastructure/
  │     ├── Persistence/
  │     │     ├── ExchangePlatformDbContext.cs
  │     │     ├── UnitOfWork.cs
  │     │     ├── Configurations/
  │     │     │     ├── UsuarioConfiguration.cs
  │     │     │     ├── ObjetoConfiguration.cs
  │     │     │     ├── IntercambioConfiguration.cs
  │     │     │     └── ...
  │     │     ├── Repositories/
  │     │     │     ├── GenericRepository.cs
  │     │     │     ├── UsuarioRepository.cs
  │     │     │     ├── ObjetoRepository.cs
  │     │     │     └── IntercambioRepository.cs
  │     │     └── Migrations/
  │     ├── Security/
  │     │     ├── JwtService.cs
  │     │     └── PasswordHasher.cs
  │     ├── Services/
  │     │     ├── AlmacenamientoLocalService.cs      (IAlmacenamientoService — Dev/Test)
  │     │     ├── AlmacenamientoSupabaseService.cs   (IAlmacenamientoService — Staging/Prod, ADR-013)
  │     │     ├── NotificationService.cs
  │     │     └── AuditService.cs
  │     └── Logging/
  │           └── SerilogConfig.cs
  │
  └── ExchangePlatform.API/
        ├── Controllers/
        ├── Middlewares/
        ├── Filters/
        ├── Extensions/
        ├── appsettings.json
        └── Program.cs
  tests/
    ├── ExchangePlatform.UnitTests/
    └── ExchangePlatform.IntegrationTests/
```

### 3.2 Regla de Dependencias

```
┌─────────────────────────────────────────────────┐
│                   API Layer                     │
│  Referencia: Application                        │
└──────────────────────┬──────────────────────────┘
                       │ usa
┌──────────────────────▼──────────────────────────┐
│               Application Layer                 │
│  Referencia: Domain                             │
│  NO referencia: Infrastructure                  │
│  NO referencia: API                             │
└──────────────────────┬──────────────────────────┘
                       │ usa (interfaces)
┌──────────────────────▼──────────────────────────┐
│                 Domain Layer                    │
│  Sin referencias externas                       │
│  Es el núcleo — no depende de nada              │
└─────────────────────────────────────────────────┘
         ▲ implementa interfaces
┌────────┴────────────────────────────────────────┐
│             Infrastructure Layer                │
│  Referencia: Domain (implementa interfaces)     │
│  Referencia: Application (registra servicios)   │
│  NO referencia: API                             │
└─────────────────────────────────────────────────┘
```

**Regla fundamental:** las dependencias siempre apuntan hacia adentro (hacia Domain). Domain no conoce ninguna capa exterior.

### 3.3 Flujo CQRS con MediatR

```
HTTP Request
     │
     ▼
Controller (Capa API)
     │ Crea Command o Query
     ▼
MediatR.Send(command/query)
     │
     ├── ValidationBehavior (FluentValidation) → lanza si inválido
     ├── LoggingBehavior    (Serilog)          → registra entrada/salida
     └── AuditBehavior      (AuditService)     → registra en AuditLog
          │
          ▼
     Handler (Capa Application)
          │ Usa IRepository / IUnitOfWork
          │ Aplica reglas de dominio
          ▼
     Repository (Capa Infrastructure)
          │ EF Core DbContext (Npgsql)
          ▼
     PostgreSQL (Supabase)
```

### 3.4 Contrato de Respuesta Estándar de la API

Toda respuesta de la API sigue este contrato sin excepción:

```json
{
  "success"   : true,
  "message"   : "Operación exitosa.",
  "data"      : { },
  "errors"    : null,
  "timestamp" : "2026-06-03T00:00:00Z"
}
```

En caso de error:

```json
{
  "success"   : false,
  "message"   : "Error de validación.",
  "data"      : null,
  "errors"    : ["El título es requerido.", "La categoría no es válida."],
  "timestamp" : "2026-06-03T00:00:00Z"
}
```

### 3.5 Contrato de Respuesta Paginada

Para todos los endpoints de lista:

```json
{
  "success"     : true,
  "message"     : "OK",
  "data"        : {
    "items"       : [ ],
    "pageNumber"  : 1,
    "pageSize"    : 20,
    "totalRecords": 150,
    "totalPages"  : 8,
    "hasPrevious" : false,
    "hasNext"     : true
  },
  "errors"      : null,
  "timestamp"   : "2026-06-03T00:00:00Z"
}
```

---

## 4. Arquitectura Frontend — Feature Based

### 4.1 Estructura de Carpetas

```
/frontend
  src/
  │
  ├── assets/
  │     ├── images/
  │     └── icons/
  │
  ├── components/                    ← Componentes reutilizables globales
  │     ├── ui/
  │     │     ├── Button.tsx
  │     │     ├── Input.tsx
  │     │     ├── Select.tsx
  │     │     ├── Modal.tsx
  │     │     ├── Card.tsx
  │     │     ├── Table.tsx
  │     │     ├── Pagination.tsx
  │     │     ├── SearchBox.tsx
  │     │     ├── Loading.tsx
  │     │     ├── EmptyState.tsx
  │     │     ├── Toast.tsx
  │     │     ├── Avatar.tsx
  │     │     ├── Badge.tsx
  │     │     └── Gallery.tsx
  │     └── layout/
  │           ├── Navbar.tsx
  │           ├── Sidebar.tsx
  │           └── Footer.tsx
  │
  ├── features/                      ← Módulos por dominio (Feature Based)
  │     ├── auth/
  │     │     ├── components/
  │     │     │     ├── LoginForm.tsx
  │     │     │     ├── RegisterForm.tsx
  │     │     │     └── RecoverPasswordForm.tsx
  │     │     ├── hooks/
  │     │     │     └── useAuth.ts
  │     │     ├── services/
  │     │     │     └── authService.ts
  │     │     ├── types/
  │     │     │     └── auth.types.ts
  │     │     └── schemas/
  │     │           └── auth.schemas.ts     ← Zod schemas
  │     │
  │     ├── users/
  │     │     ├── components/
  │     │     ├── hooks/
  │     │     ├── services/
  │     │     ├── types/
  │     │     └── schemas/
  │     │
  │     ├── objects/
  │     │     ├── components/
  │     │     │     ├── ObjectCard.tsx
  │     │     │     ├── ObjectDetail.tsx
  │     │     │     ├── ObjectForm.tsx
  │     │     │     ├── ObjectList.tsx
  │     │     │     └── ImageUploader.tsx
  │     │     ├── hooks/
  │     │     ├── services/
  │     │     ├── types/
  │     │     └── schemas/
  │     │
  │     ├── exchanges/
  │     │     ├── components/
  │     │     │     ├── ExchangeRequest.tsx
  │     │     │     ├── ExchangeDetail.tsx
  │     │     │     ├── ExchangeList.tsx
  │     │     │     └── ExchangeConfirm.tsx
  │     │     ├── hooks/
  │     │     ├── services/
  │     │     ├── types/
  │     │     └── schemas/
  │     │
  │     ├── notifications/
  │     ├── messages/
  │     ├── favorites/
  │     ├── reports/
  │     └── admin/
  │           ├── components/
  │           │     ├── DashboardKPIs.tsx
  │           │     ├── UserManagement.tsx
  │           │     ├── ObjectManagement.tsx
  │           │     ├── ReportManagement.tsx
  │           │     └── AuditLog.tsx
  │           ├── hooks/
  │           └── services/
  │
  ├── layouts/
  │     ├── PublicLayout.tsx
  │     ├── AuthLayout.tsx
  │     ├── DashboardLayout.tsx
  │     └── AdminLayout.tsx
  │
  ├── pages/
  │     ├── Landing.tsx
  │     ├── Login.tsx
  │     ├── Register.tsx
  │     ├── RecoverPassword.tsx
  │     ├── Profile.tsx
  │     ├── EditProfile.tsx
  │     ├── PublishObject.tsx
  │     ├── ObjectDetail.tsx
  │     ├── Search.tsx
  │     ├── Exchanges.tsx
  │     ├── Messages.tsx
  │     ├── Favorites.tsx
  │     ├── Notifications.tsx
  │     ├── Dashboard.tsx
  │     └── admin/
  │           ├── AdminDashboard.tsx
  │           ├── AdminUsers.tsx
  │           ├── AdminObjects.tsx
  │           ├── AdminReports.tsx
  │           └── AdminAudit.tsx
  │
  ├── routes/
  │     ├── AppRouter.tsx
  │     ├── PublicRoute.tsx
  │     ├── ProtectedRoute.tsx
  │     └── RoleBasedRoute.tsx
  │
  ├── services/
  │     └── apiClient.ts             ← Axios instance + interceptors
  │
  ├── stores/                        ← Zustand stores
  │     ├── authStore.ts
  │     ├── userStore.ts
  │     ├── objectStore.ts
  │     ├── exchangeStore.ts
  │     ├── notificationStore.ts
  │     └── adminStore.ts
  │
  ├── hooks/                         ← Hooks globales reutilizables
  │     ├── useDebounce.ts
  │     ├── usePagination.ts
  │     └── useGeolocation.ts
  │
  ├── types/                         ← Tipos globales TypeScript
  │     ├── api.types.ts
  │     ├── auth.types.ts
  │     └── common.types.ts
  │
  ├── utils/
  │     ├── formatters.ts
  │     ├── validators.ts
  │     └── constants.ts
  │
  └── styles/
        └── globals.css
```

### 4.2 Gestión de Estado (Zustand)

Cada store es independiente y sigue el mismo patrón:

```typescript
// Patrón estándar de un Zustand Store
interface AuthStore {
  // Estado
  user        : UserDto | null;
  accessToken : string | null;
  isAuthenticated: boolean;

  // Acciones
  login       : (dto: LoginResponseDto) => void;
  logout      : () => void;
  setUser     : (user: UserDto) => void;
}
```

### 4.3 Interceptores de Axios

El `apiClient.ts` implementa:

```
Request Interceptor:
  → Añade Authorization: Bearer {accessToken}
  → Añade X-Correlation-Id header

Response Interceptor:
  → Si HTTP 401 → intenta refresh token automático
  → Si refresh falla → logout y redirige a /login
  → Si error de red → muestra toast de error
```

### 4.4 Protección de Rutas

```
/login         → PublicRoute    (redirige a /dashboard si ya autenticado)
/register      → PublicRoute
/              → PublicRoute    (Landing pública)
/search        → PublicRoute
/objects/:id   → PublicRoute

/dashboard     → ProtectedRoute (cualquier usuario autenticado)
/profile       → ProtectedRoute
/publish       → ProtectedRoute
/exchanges     → ProtectedRoute
/messages      → ProtectedRoute
/favorites     → ProtectedRoute

/admin/*       → RoleBasedRoute (roles: Administrador, Moderador)
/admin/audit   → RoleBasedRoute (rol: solo Administrador)
```

---

## 5. Arquitectura de Datos

### 5.1 Estrategia de Acceso a Datos

```
Application Layer
      │ usa interfaces (IRepository)
      ▼
Infrastructure Layer
      │
      ├── GenericRepository<T>   ← operaciones CRUD base
      │     ├── GetByIdAsync()
      │     ├── GetAllAsync()
      │     ├── AddAsync()
      │     ├── UpdateAsync()
      │     └── DeleteAsync()    ← Soft Delete
      │
      ├── UsuarioRepository      ← extiende GenericRepository
      │     └── GetByEmailAsync()
      │
      ├── ObjetoRepository
      │     ├── GetDisponiblesAsync(filtros, paginacion)
      │     └── GetByUsuarioIdAsync(usuarioId)
      │
      └── IntercambioRepository
            ├── GetActivosPorUsuarioAsync(usuarioId)
            └── TieneSolicitudActivaAsync(usuarioId, objetoId)
```

### 5.2 Unit Of Work

Garantiza la atomicidad de operaciones que afectan múltiples entidades:

```
IUnitOfWork
  ├── IUsuarioRepository       UsuarioRepository
  ├── IObjetoRepository        ObjetoRepository
  ├── IIntercambioRepository   IntercambioRepository
  ├── ...otros repositorios
  └── SaveChangesAsync()       → DbContext.SaveChangesAsync()
```

Uso en handlers críticos (ej. AceptarIntercambioHandler):

```
// Todas estas operaciones se persisten en un único commit
await _uow.IntercambioRepository.UpdateAsync(intercambio);
await _uow.ObjetoRepository.UpdateAsync(objetoSolicitado);
await _uow.IntercambioRepository.RechazarOtrasAsync(ids);
await _uow.SaveChangesAsync();   // ← único punto de commit
```

### 5.3 Filtro Global de Soft Delete

El `DbContext` aplica un query filter global que excluye automáticamente registros eliminados:

```csharp
// En ExchangePlatformDbContext.OnModelCreating()
// Se aplica a todas las entidades que heredan BaseEntity
modelBuilder.Entity<Usuario>().HasQueryFilter(u => !u.IsDeleted);
modelBuilder.Entity<Objeto>().HasQueryFilter(o => !o.IsDeleted);
// ... etc para todas las entidades
```

### 5.4 Auditoría Automática

El `AuditBehavior` (MediatR) y la sobrescritura de `SaveChangesAsync()` completan automáticamente los campos de auditoría (CreatedAt/By, UpdatedAt/By) y registran los eventos sensibles en `AuditLog` (Quién, Qué, Cuándo, Dónde, Resultado, IP, Dispositivo).

### 5.5 Modelo de Datos

El diseño conceptual, lógico y físico completo (tablas, tipos, claves, índices, constraints y normalización 1FN/2FN/3FN) se detalla en `BD.md` (Paso 7). La geografía jerárquica Departamento→Provincia→Distrito (UBIGEO Perú) se carga como datos maestros desde V1, habilitando el escalamiento nacional sin cambios de esquema (RNF-021, RN-052).

---

## 6. Arquitectura de Seguridad

### 6.1 Autenticación y Autorización

```
┌──────────────────────────────────────────────────────────┐
│  AUTENTICACIÓN (quién eres)                              │
│ JWT Access Token (15 min) + Refresh Token rotation (7 d) │
├──────────────────────────────────────────────────────────┤
│  AUTORIZACIÓN (qué puedes hacer)                         │
│  Roles: Administrador │ Moderador │ Usuario              │
│  Permisos + Políticas + Claims por endpoint              │
└──────────────────────────────────────────────────────────┘
```

### 6.2 Mitigación OWASP Top 10

| #   | Riesgo OWASP                      | Mitigación en el proyecto                                          |
|-----|-----------------------------------|--------------------------------------------------------------------|
| 1   | Broken Access Control             | Roles/permisos por endpoint. RoleBasedRoute. Validación servidor.  |
| 2   | Cryptographic Failures            | Hash de contraseñas. HTTPS. Secretos en variables de entorno.      |
| 3   | Injection                         | EF Core parametrizado. Validación con FluentValidation/Zod.        |
| 4   | Insecure Design                   | SDD + Clean Architecture + revisiones formales por fase.           |
| 5   | Security Misconfiguration         | Variables de entorno. Configuración por ambiente.                  |
| 6   | Vulnerable Components             | Dependencias actualizadas. GitHub Dependabot activado.             |
| 7   | Auth Failures                     | Bloqueo tras 5 intentos. Rate Limiting. Refresh Token rotation.    |
| 8   | Software Integrity Failures       | CI/CD con build checks. Code reviews obligatorios.                 |
| 9   | Logging Failures                  | Serilog estructurado. AuditLog inmutable.                          |
| 10  | SSRF                              | Sin llamadas a URLs arbitrarias. Dominios externos whitelisted.    |

> El detalle de la política de seguridad (rangos, expiración, hashing, headers) se desarrolla en `Seguridad.md`.

---

## 7. Arquitectura de Integración

### 7.1 API REST — Convenciones

| Aspecto       | Convención                                                                                                      |
|---------------|-----------------------------------------------------------------------------------------------------------------|
| Versionado    | /api/v1/... (preparado para /api/v2 sin romper compatibilidad)                                                  |
| Verbos HTTP   | GET (leer) / POST (crear) / PUT (reemplazar) / PATCH (parcial) / DELETE (eliminar)                              |
| Códigos HTTP  | 200, 201, 400, 401, 403, 404, 409, 422, 429, 500                                                                |
| Formato       | JSON con contrato estándar ApiResponse<T>                                                                       |
| Autenticación | Bearer Token en header Authorization                                                                            |
| Paginación    | Query params: ?pageNumber=1&pageSize=20                                                                         |
| Filtros       | Query params: ?categoriaId=1&departamentoId=5&search=bicicleta                                                  |
| Ordenamiento  | Query params: ?sortBy=createdAt&sortOrder=desc                                                                  |
| Documentación | Swagger / OpenAPI en /api/swagger (ambientes dev y staging)                                                     |

> El catálogo completo de endpoints se define en `API.md` (Paso 8).

### 7.2 Integración Interna

MediatR actúa como bus de mensajes (Commands, Queries, Domain Events), desacoplando las capas y permitiendo cross-cutting concerns vía Pipeline Behaviors.

### 7.3 Preparación para Integraciones Futuras

| Integración              | Estado en V1                     | Planificado   |
|--------------------------|----------------------------------|---------------|
| Servicio de correo       | Interfaz IEmailService lista     | V2            |
| Google Maps / Leaflet    | Modelo de datos preparado        | V2            |
| Notificaciones push      | Estructura de notif. lista       | V3            |
| IA / Recomendaciones     | Sin acoplamiento a implementar   | V4            |

Todas las integraciones externas se diseñan tras interfaces, con degradación elegante y reintentos (mitiga RGO-011).

---

## 8. Arquitectura de Despliegue

### 8.1 Ambientes

| Ambiente    | Propósito                                        | Frontend | Backend | BD (proyecto Supabase)     |
|-------------|---------------------------------------------------|----------|---------|-------------------------------|
| Development | Desarrollo local. Debug activo. Swagger visible. | Vite dev server (local) | Local (dotnet run / Docker) | exchange-platform-dev         |
| Testing     | Ejecución de pruebas automatizadas CI/CD.        | — (no aplica) | — (no aplica) | PostgreSQL vía Testcontainers (no consume el proyecto Supabase) |
| Staging     | Validación pre-producción. Igual a producción.   | Vercel (preview/staging) | Render (staging service) | exchange-platform-staging     |
| Production  | Sistema en producción. Sin debug. Sin Swagger.   | Vercel (producción) | Render (producción) | exchange-platform-prod        |

> Cada ambiente productivo (Dev/Staging/Prod) es un **proyecto Supabase independiente**, con URL, credenciales y `connection string` propios — el aislamiento entre ambientes se da a nivel de proyecto, no de base de datos dentro de una instancia compartida. Staging y Production son además **servicios independientes en Vercel y en Render** (mismo repositorio, distinta rama/ambiente).

### 8.2 Servicios de Despliegue (Vercel + Render)

```
Frontend → Vercel
  - Build: npm run build (Vite)
  - Salida: /frontend/dist servido por CDN de Vercel
  - SSL: automático (certificado gestionado por Vercel)
  - Dominio: *.vercel.app (o dominio custom)
  - Variables de entorno: VITE_API_URL apuntando al backend en Render

Backend → Render (Web Service, Docker)
  - Build: Dockerfile de /backend (Docker.md, sin cambios)
  - SSL: automático (certificado gestionado por Render)
  - Dominio: *.onrender.com (o dominio custom)
  - Variables de entorno: CONNECTION_STRING, JWT_SECRET, ALLOWED_ORIGINS, etc.
  - Healthcheck: GET /health (usado también por keep-alive-supabase.yml)

Base de datos → Supabase (sin cambios, ADR-010)
```

> No hay contenedor `proxy` (Nginx) ni Docker Compose en producción. Vercel y Render terminan TLS y enrutan cada uno su propio servicio de forma independiente (ver ADR-011). Docker Compose se conserva **solo para desarrollo local** (Docker.md).

### 8.3 Pipeline CI/CD (GitHub Actions + Deploys Vercel/Render)

```
Trigger: push a develop o pull_request a main
         │
         ▼
┌─────────────────┐
│   1. BUILD      │  dotnet build + npm run build
└────────┬────────┘
         ▼
┌─────────────────┐
│   2. TEST       │  dotnet test (xUnit + Moq, Testcontainers PostgreSQL)
└────────┬────────┘
         ▼
┌─────────────────┐
│  3. QUALITY     │  Cobertura mínima 90%
└────────┬────────┘
         ▼
┌─────────────────┐
│   4. DOCKER     │  docker build backend (imagen validada antes de que Render la construya)
└────────┬────────┘
         ▼
┌─────────────────┐  (solo en merge a develop/main)
│   5. DEPLOY     │  Vercel y Render despliegan automáticamente al detectar el push
│                 │  (integración nativa GitHub → no requiere SSH ni docker compose up)
└─────────────────┘
```

> El paso 5 ya no lo ejecuta GitHub Actions por SSH: Vercel y Render están conectados directamente al repositorio y disparan su propio build/deploy al detectar el push en la rama configurada. GitHub Actions conserva el control de calidad (pasos 1-4) como gate antes de que el código llegue a esas ramas.

### 8.4 Variables de Entorno por Ambiente

```
# Backend (Render) — variables de entorno del servicio
ASPNETCORE_ENVIRONMENT=Production
CONNECTION_STRING=Host=aws-0-[region].pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.[ref];Password=<...>;SSL Mode=Require
JWT_SECRET=<generado-por-ambiente>
JWT_ISSUER=exchange-platform
JWT_AUDIENCE=exchange-platform-users
ACCESS_TOKEN_EXPIRY_MINUTES=15
REFRESH_TOKEN_EXPIRY_DAYS=7
MAX_FILE_SIZE_MB=5
ALLOWED_ORIGINS=https://<proyecto>.vercel.app

# Frontend (Vercel) — variables de entorno del proyecto
VITE_API_URL=https://<proyecto>.onrender.com/api/v1
```

### 8.5 Cloud Readiness

Frontend y backend son portables: Vercel y Render pueden reemplazarse por otra PaaS o por un VPS con Docker (Azure/AWS/GCP) sin cambios de código, ya que la configuración vive en variables de entorno (RNF-022, ADR-008, ADR-011). La base de datos ya es cloud-native al vivir en Supabase (PostgreSQL gestionado, con backups, pooling y escalado propios, ADR-010). Las imágenes se almacenan en **Supabase Storage** en Staging/Producción (ADR-013); la interfaz `IAlmacenamientoService` mantiene la portabilidad hacia Azure Blob / S3 sin cambios en el resto del código.

---

## 9. Patrones Aplicados

| Patrón                   | Capa           | Justificación                                                  |
|--------------------------|----------------|----------------------------------------------------------------|
| Clean Architecture       | Global         | Separación de responsabilidades. Mantenibilidad. Testabilidad. |
| CQRS                     | Application    | Separación de lectura y escritura. Escalabilidad.              |
| Mediator (MediatR)       | Application    | Desacoplamiento entre controllers y handlers.                  |
| Repository               | Infrastructure | Abstracción del acceso a datos. Testabilidad con mocks.        |
| Unit Of Work             | Infrastructure | Atomicidad en operaciones multi-entidad.                       |
| Domain Events            | Domain         | Desacoplamiento entre agregados. Notificaciones reactivas.     |
| Specification (futuro)   | Application    | Encapsular lógica de consultas complejas.                      |
| Pipeline Behavior        | Application    | Cross-cutting concerns (validación, logging, auditoría).       |
| Feature Based (Frontend) | Frontend       | Modularidad. Cohesión. Escalabilidad por features.             |
| Flux/Store (Zustand)     | Frontend       | Estado predecible y centralizado por módulo.                   |
| Interceptor (Axios)      | Frontend       | Manejo centralizado de auth, errores y refresh token.          |

---

## 10. Antipatrones Evitados

| Antipatrón                  | Cómo se evita                                                        |
|-----------------------------|----------------------------------------------------------------------|
| God Object / God Controller | Un controller por recurso. Sin lógica de negocio en controllers.     |
| Anemic Domain Model         | Las entidades tienen métodos de dominio: Aceptar(), Reservar(), etc. |
| Spaghetti Code              | Clean Architecture. Capas con responsabilidades claras.              |
| Magic Numbers / Strings     | Constantes en clase Constants. Enums para estados.                   |
| Hard-coded Config           | Variables de entorno por ambiente. Sin secretos en código.           |
| Fat Repository              | Repositorios solo hacen CRUD. Lógica en Domain/Application.          |
| N+1 Query Problem           | EF Core con Include(). Proyecciones en queries.                      |
| Direct DB Access in API     | Nunca. Siempre vía Application → Infrastructure → DB.                |
| Shared Mutable State        | Zustand stores aislados por módulo. Sin estado global sin control.   |

---

## 11. Architecture Decision Records (ADR)

### ADR-001 — Clean Architecture en capas

| Campo         | Detalle                                                                                                                                                                              |
|---------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Estado        | Aprobado                                                                                                                                                                             |
| Fecha         | 2026-06-03                                                                                                                                                                           |
| Contexto      | Definir el estilo arquitectónico del backend.                                                                                                                                        |
| Problema      | ¿Monolito simple, monolito modular→microservicios o Clean Architecture en capas?                                                                                                     |
| Alternativas  | Microservicios (complejidad operacional alta para MVP); monolito simple (difícil de mantener/escalar); Clean Architecture en capas (separación clara, testable, evolucionable).      |
| Decisión      | Clean Architecture en capas (Domain, Application, Infrastructure, API) sobre un único backend. Bounded contexts internos permiten evolución futura sin comprometer el estilo actual. |
| Consecuencias | Mayor estructura inicial (más proyectos); a cambio, alta mantenibilidad, testabilidad y dependencias controladas hacia el Dominio.                                                   |

### ADR-002 — SQL Server como motor de base de datos (REEMPLAZADO por ADR-010)

| Campo         | Detalle                                                                                                              |
|---------------|----------------------------------------------------------------------------------------------------------------------|
| Estado        | **Reemplazado** (ver ADR-010, 2026-07-08)                                                                            |
| Fecha         | 2026-06-03                                                                                                           |
| Contexto      | Selección del motor relacional.                                                                                      |
| Alternativas  | PostgreSQL, MySQL, SQL Server.                                                                                       |
| Decisión      | SQL Server 2022: integración nativa con .NET/EF Core, soporte transaccional, herramientas maduras, ruta a Azure SQL. |
| Consecuencias | Licenciamiento en producción (Developer Edition gratuita en dev); fuerte compatibilidad con el ecosistema .NET.      |

> **Nota:** decisión conservada por trazabilidad histórica. **Ya no está vigente** — ver `ADR-010` para la decisión actual (PostgreSQL / Supabase).

### ADR-003 — MediatR para implementar CQRS

| Campo         | Detalle                                                                                                             |
|---------------|---------------------------------------------------------------------------------------------------------------------|
| Estado        | Aprobado                                                                                                            |
| Fecha         | 2026-06-03                                                                                                          |
| Contexto      | Implementar CQRS de forma desacoplada.                                                                              |
| Decisión      | MediatR: estándar de facto en .NET; Pipeline Behaviors para validación, logging y auditoría sin modificar handlers. |
| Consecuencias | Desacoplamiento total API↔Application; pipeline reutilizable; más clases por caso de uso.                           |

### ADR-004 — JWT con Refresh Token Rotation

| Campo         | Detalle                                                                                                            |
|---------------|--------------------------------------------------------------------------------------------------------------------|
| Estado        | Aprobado                                                                                                           |
| Fecha         | 2026-06-03                                                                                                         |
| Contexto      | Autenticación stateless para API REST.                                                                             |
| Decisión      | Access Token JWT (15 min) + Refresh Token con rotación (7 días); cada uso genera uno nuevo e invalida el anterior. |
| Consecuencias | Menor riesgo por tokens de corta vida; rotación previene reutilización.                                            |    

### ADR-005 — Zustand para gestión de estado en Frontend

| Campo         | Detalle                                                                                        |
|---------------|------------------------------------------------------------------------------------------------|
| Estado        | Aprobado                                                                                       |
| Fecha         | 2026-06-03                                                                                     |
| Contexto      | Selección de librería de estado para React.                                                    |      
| Alternativas  | Redux (verboso), Context API (rendimiento limitado), Zustand (simple, potente).                |
| Decisión      | Zustand: API minimalista, sin boilerplate, fácil de testear, stores independientes por módulo. |
| Consecuencias | Menor curva de aprendizaje; stores aislados reducen acoplamiento entre features.               |

### ADR-006 — Soft Delete Universal

| Campo         | Detalle                                                                                                  |
|---------------|----------------------------------------------------------------------------------------------------------|
| Estado        | Aprobado                                                                                                 |
| Fecha         | 2026-06-03                                                                                               |
| Contexto      | Estrategia de eliminación de datos.                                                                      |
| Decisión      | Soft Delete en todas las entidades (IsDeleted, DeletedAt, DeletedBy) con query filter global en EF Core. |
| Consecuencias | Trazabilidad completa; recuperación posible; mayor tamaño de BD (manejable con archivado).               |

### ADR-007 — Geografía jerárquica por datos (UBIGEO)

| Campo         | Detalle                                                                                                                                              |
|---------------|------------------------------------------------------------------------------------------------------------------------------------------------------|
| Estado        | Aprobado                                                                                                                                             |
| Fecha         | 2026-06-03                                                                                                                                           |
| Contexto      | Requisito de escalamiento nacional sin rediseños (RNF-020/021, RN-052).                                                                              |
| Decisión      | Modelar Departamento→Provincia→Distrito como datos maestros (UBIGEO Perú completo desde V1); nuevas regiones se incorporan por datos, no por código. |
| Consecuencias | Carga y validación inicial de datos maestros; escalamiento nacional sin cambios de esquema.                                                          |

### ADR-008 — Repository + Unit Of Work sobre EF Core

| Campo         | Detalle                                                                                                     |
|---------------|-------------------------------------------------------------------------------------------------------------|
| Estado        | Aprobado                                                                                                    |
| Fecha         | 2026-06-03                                                                                                  |
| Contexto      | Acceso a datos testeable y transaccional.                                                                   |
| Decisión      | Patrón Repository (abstracción) + Unit Of Work (un único commit por operación multi-entidad) sobre EF Core. |
| Consecuencias | Capa extra sobre EF Core; a cambio, testabilidad con mocks y atomicidad garantizada.                        |

### ADR-009 — Docker + Docker Compose (VIGENTE — solo desarrollo local, ver ADR-011)

| Campo         | Detalle                                                                                          |
|---------------|------------------------------------------------------------------------------------------------------|
| Estado        | Vigente (parcial)                                                                                  |
| Fecha         | 2026-06-03                                                                                        |
| Contexto      | Reproducibilidad de ambientes y portabilidad a nube.                                              |
| Decisión      | Empaquetar frontend y backend en contenedores orquestados con Docker Compose (BD ver ADR-010).    |
| Consecuencias | Curva de adopción DevOps; entornos reproducibles y cloud-ready. **Actualización 2026-07-09:** el despliegue en la nube (Staging/Producción) pasa a Vercel + Render (ADR-011); Docker Compose se conserva para desarrollo local y como opción de portabilidad futura (§8.5). |

### ADR-010 — Migración de motor de base de datos a PostgreSQL/Supabase (MODIFICADO PARCIALMENTE por ADR-012 — Supabase queda restringido al ambiente de Producción)

| Campo         | Detalle                                                                                                                                                                                                                                                                                             |
|---------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Estado        | Aprobado                                                                                                                                                                                                                                                                                            |
| Fecha         | 2026-07-08                                                                                                                                                                                                                                                                                          |
| Contexto      | ADR-002 aprobó SQL Server 2022 en contenedor propio. Se reevaluó la estrategia de despliegue del backend, frontend y BD para simplificar operación e infraestructura del MVP.                                                                                                                    |
| Problema      | Correr SQL Server en el mismo host que backend/frontend exige un VPS de mayor capacidad, gestión manual de backups/DRP, y no resuelve la licencia de producción (Developer Edition no está autorizada para producción real).                                                                    |
| Alternativas  | (1) Mantener SQL Server en contenedor propio; (2) SQL Server Express (gratis, apto para producción, con límites de 10GB/1 core); (3) PostgreSQL gestionado en Supabase.                                                                                                                          |
| Decisión      | Migrar el motor de base de datos de SQL Server 2022 a **PostgreSQL 15+ sobre Supabase** (servicio gestionado). El equipo ya no gestiona el motor de BD directamente: Supabase administra backups automáticos, connection pooling y disponibilidad. El acceso desde .NET se realiza vía EF Core con el proveedor **Npgsql**. |
| Consecuencias | (+) Menor infraestructura a mantener (el VPS solo aloja backend + frontend); (+) backups y disponibilidad delegados a Supabase; (+) sin problema de licenciamiento. (-) Dependencia de un proveedor externo y de conectividad a internet para cada consulta a BD; (-) el plan gratuito de Supabase tiene límites de pausa por inactividad, egress y almacenamiento a evaluar antes de producción real; (-) reemplaza a ADR-002, obligando a actualizar `BD.md`, `Docker.md`, `Deployment.md` y `CICD.md` en cascada (cumplido en esta versión). |

---

### ADR-011 — Despliegue en Vercel (frontend) + Render (backend), en reemplazo de VPS Hostinger

| Campo         | Detalle                                                                                                                                                                                                                                                                                             |
|---------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Estado        | Aprobado                                                                                                                                                                                                                                                                                            |
| Fecha         | 2026-07-09                                                                                                                                                                                                                                                                                          |
| Contexto      | Deployment.md, Docker.md y CICD.md definían el despliegue de frontend y backend como contenedores Docker Compose sobre un VPS de pago (Hostinger), con un contenedor `proxy` (Nginx) para terminación SSL y enrutamiento. Para la fase de tesis se requiere un despliegue público, verificable y con costo $0, sin gestión manual de servidor. |
| Problema      | Un VPS de pago no es indispensable para el alcance actual (Fase 1 — MVP académico, Ayacucho). Implica costo recurrente y carga operativa (sistema operativo, parches, Nginx, TLS, backups del host) que no aporta valor a la sustentación de tesis.                                              |
| Alternativas  | (1) Mantener VPS Hostinger + Docker Compose (ADR-009 sin cambios); (2) Vercel (frontend) + Render (backend), ambos con plan gratuito, + Supabase (BD, sin cambios); (3) Otras PaaS (Railway, Fly.io) — descartadas por no ofrecer mejor relación simplicidad/costo para .NET 10 + React en esta etapa. |
| Decisión      | Desplegar el **frontend en Vercel** (build estático de Vite, CDN, HTTPS automático, integración nativa con GitHub) y el **backend en Render** (Web Service sobre el Dockerfile ya definido en Docker.md, HTTPS automático, integración con GitHub). Supabase se mantiene sin cambios (ADR-010). El contenedor `proxy` (Nginx) de ADR-009 se elimina del despliegue en la nube: cada plataforma termina TLS y enruta directamente. Docker Compose deja de ser el mecanismo de despliegue en Staging/Producción; se conserva únicamente para desarrollo local (ADR-009, actualizado). |
| Consecuencias | (+) Costo $0; (+) HTTPS y CDN gestionados automáticamente por cada plataforma; (+) despliegue automático en cada push/merge vía integración nativa con GitHub, sin SSH ni gestión de servidor; (+) menor superficie de mantenimiento (sin SO, sin Nginx, sin parches de host). (-) El plan gratuito de Render suspende el servicio tras ~15 min de inactividad, generando un cold start de 30–60s en la primera petición (mitigado parcialmente ampliando `keep-alive-supabase.yml` para incluir el endpoint de Render, ver CICD.md); (-) dependencia de dos proveedores externos adicionales (Vercel, Render) sumados a Supabase; (-) menor control operativo que un VPS propio (límites de build time, ancho de banda y cómputo del plan gratuito a vigilar antes de un eventual crecimiento — Fase 3, §14.1); (-) reemplaza parcialmente a ADR-009, obligando a actualizar `Deployment.md`, `Docker.md`, `CICD.md`, `UML.md` y `README.md` en cascada (pendiente, próximas iteraciones). |

---

### ADR-012 — Aislamiento de bases de datos en tres niveles (Producción / Desarrollo / Test)

| Campo         | Detalle                                                                                                                                                                                                                                                                                             |
|---------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Estado        | Aprobado                                                                                                                                                                                                                                                                                            |
| Fecha         | 2026-07-13                                                                                                                                                                                                                                                                                          |
| Contexto      | ADR-010 estableció Supabase (PostgreSQL) como motor de base de datos, y `Docker.md` v1.2.0 declaró explícitamente que la BD queda *"fuera de Docker Compose"*, sin servicio `db` local. Bajo ese diseño, el backend en desarrollo local se conecta al **mismo proyecto Supabase** que la aplicación desplegada. Con la incorporación de la suite E2E (Playwright, `Testing.md` v1.2.0) — que crea usuarios, objetos e intercambios de forma automatizada en cada ejecución — ese diseño resulta inviable. |
| Problema      | (1) **Contaminación de datos de producción:** el desarrollo local y las pruebas E2E escriben registros basura (usuarios, objetos, intercambios) en la BD que se expondrá durante la sustentación de tesis, alterando listados, búsquedas e indicadores del panel de administración (RF-111). (2) **Riesgo de corrupción de esquema:** `dotnet ef database update` ejecutado durante el desarrollo aplicaría migraciones directamente sobre producción, sin rollback. (3) **Consumo de cuota:** el plan gratuito de Supabase se agota con tráfico de desarrollo y pruebas. (4) **Pruebas no deterministas:** una suite E2E que depende de una BD compartida y mutable no es reproducible (ver DEF-01 / PR-097 en `Testing.md`). |
| Alternativas  | (1) Mantener el diseño de ADR-010 + `Docker.md` (BD única en Supabase para todos los ambientes) — **descartada:** viola la paridad de entornos y expone producción. (2) Aislar **solo** la BD de test en Docker, dejando desarrollo local contra Supabase — **descartada:** mitiga la contaminación por E2E pero no la del desarrollo diario, que es la fuente principal de datos basura. (3) Usar esquemas (`schemas`) separados dentro del mismo proyecto Supabase — **descartada:** no aísla frente a errores de migración ni protege ante `DROP`, y sigue consumiendo cuota. (4) **Aislamiento físico en tres niveles** — **adoptada.** |
| Decisión      | Establecer **tres bases de datos físicamente separadas**, seleccionables por variable de entorno: **(N1) Producción** — Supabase (`PLATAFORMAIDIOA`), pooler `aws-1-us-east-2.pooler.supabase.com:6543`; uso exclusivo de la aplicación desplegada (Vercel + Render); *ningún proceso de desarrollo o prueba se conecta a este nivel*. **(N2) Desarrollo** — contenedor Docker `exchange-dev-db` (PostgreSQL 15), `localhost:5432`; uso: desarrollo local diario y exploración manual. **(N3) Test** — contenedor Docker `exchange-db-test` (PostgreSQL 15), `localhost:5433`, definido en `docker-compose.test.yml`; uso **exclusivo** de la suite E2E de Playwright; se levanta y se destruye por ejecución. La selección se resuelve por **precedencia de configuración**: la variable de entorno `ConnectionStrings__DefaultConnection` **sobrescribe** cualquier valor de `appsettings.json`. El script `start-e2e.ps1` la fija explícitamente antes de ejecutar migraciones o pruebas, e imprime el puerto activo como confirmación visual. |
| Consecuencias | (+) La BD de producción permanece limpia y presentable para la sustentación; (+) desarrollo local sin riesgo — se puede romper, migrar o vaciar la BD sin consecuencias; (+) pruebas E2E deterministas y reproducibles sobre una BD efímera; (+) se preserva la cuota del plan gratuito de Supabase; (+) cumple el principio de paridad de entornos (12-Factor App). (-) Requiere Docker Desktop en ejecución para desarrollar; (-) tres cadenas de conexión que mantener sincronizadas; (-) **riesgo residual crítico:** el `DesignTimeDbContextFactory` de EF Core cae **silenciosamente** al puerto 5432 si `ConnectionStrings__DefaultConnection` no está definida, pudiendo aplicar migraciones sobre la BD equivocada sin emitir aviso — mitigado con la confirmación visual de puerto de `start-e2e.ps1` y registrado como **RGO-016** en `Riesgos.md`; (-) **modifica parcialmente a ADR-009 y ADR-010**: el principio *"la BD corre en Supabase, fuera de Docker Compose"* (`Docker.md` §1) deja de ser cierto para N2 y N3 — obliga a actualizar `Docker.md`, `Deployment.md`, `CICD.md`, `Testing.md`, `Riesgos.md` y `README.md` en cascada. |

#### Diagrama de los tres niveles

```
┌──────────────────────────────────────────────────────────────────────┐
│  N1 — PRODUCCIÓN                                          ⛔ INTOCABLE │
│  Supabase · PLATAFORMAIDIOA                                          │
│  aws-1-us-east-2.pooler.supabase.com:6543                            │
│  Consumidores: Vercel (frontend) + Render (backend) desplegados      │
└──────────────────────────────────────────────────────────────────────┘
              ▲
              │  ✗  NINGÚN proceso local se conecta aquí
              │
┌──────────────────────────────────────────────────────────────────────┐
│  N2 — DESARROLLO                                                     │
│  Docker · exchange-dev-db · PostgreSQL 15                            │
│  localhost:5432                                                      │
│  Consumidores: backend en `dotnet run` local                         │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│  N3 — TEST                                                           │
│  Docker · exchange-db-test · PostgreSQL 15                           │
│  localhost:5433  (docker-compose.test.yml)                           │
│  Consumidores: suite E2E Playwright (start-e2e.ps1) — efímera        │
└──────────────────────────────────────────────────────────────────────┘
```

#### Precedencia de resolución de la cadena de conexión

| Prioridad | Origen                                          | Gana |
|-----------|-------------------------------------------------|------|
| 1         | `ConnectionStrings__DefaultConnection` (env var) | ✅    |
| 2         | `appsettings.{Environment}.json`                 |      |
| 3         | `appsettings.json`                               |      |

> ⚠️ **Trampa conocida:** si la variable de entorno **no está definida**, el `DesignTimeDbContextFactory` de EF Core cae al puerto **5432** sin emitir advertencia. Consecuencia: `dotnet ef database update` aplica las migraciones sobre la BD equivocada de forma silenciosa. **Mitigación obligatoria:** fijar la variable de forma explícita antes de cualquier comando de EF Core (ver `Testing.md` §12.1 y `Riesgos.md` RGO-016).

---

### ADR-013 — Almacenamiento de imágenes en Supabase Storage (reemplaza el almacenamiento en disco local)

| Campo         | Detalle                                                                                                                                                                                                                                                                                             |
|---------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Estado        | Aprobado                                                                                                                                                                                                                                                                                            |
| Fecha         | 2026-07-14                                                                                                                                                                                                                                                                                          |
| Contexto      | La arquitectura describía el almacenamiento de imágenes como un servicio genérico tras interfaz (`FileStorageService` en este documento y en `Backend.md`), con «fácil migración a Azure Blob / S3». La única implementación real (`AlmacenamientoLocalService`) escribe los archivos en `wwwroot/uploads` del propio backend y devuelve una URL servida como archivo estático. ADR-011 movió el backend a Render (contenedor Docker en plan gratuito). |
| Problema      | El filesystem del contenedor de Render es **efímero**: se vacía en cada deploy y en cada reinicio, y el plan Free suspende el servicio tras ~15 min de inactividad (ver `Deployment.md` §7). Las imágenes que subían los usuarios desaparecían minutos después — la BD conservaba la URL en `ImagenesObjeto.url`, pero el archivo físico daba **404** (imágenes rotas en producción). Adicionalmente, la URL se componía con `BackendUrl` (fallback `https://localhost:7149`), quedando rota si esa variable no se definía en Render. |
| Alternativas  | (1) Mantener disco local en Render — **descartada:** el filesystem efímero es la causa raíz. (2) Persistent Disk de Render — **descartada:** no está disponible en el plan Free y ataría el almacenamiento a una única instancia. (3) Azure Blob Storage / AWS S3 — viables y ya contempladas por la interfaz, pero **descartadas para el MVP** por introducir un proveedor y credenciales nuevos. (4) **Supabase Storage** — **adoptada:** reutiliza el proyecto Supabase ya existente (ADR-010), ofrece 1 GB en el plan Free, CDN y URL pública estable. |
| Decisión      | Introducir **`AlmacenamientoSupabaseService`** (implementación de `IAlmacenamientoService`) que sube el archivo a un **bucket público de Supabase Storage** vía su API REST y devuelve la URL pública del CDN (`…/storage/v1/object/public/<bucket>/<archivo>`). Se activa en **Staging/Producción** cuando `Supabase__Url` y `Supabase__ServiceKey` están definidas; en su ausencia (**Desarrollo/Test**) se conserva `AlmacenamientoLocalService` (disco). La selección se resuelve en `Program.cs` por presencia de variables de entorno, coherente con el principio de configuración por entorno de ADR-011/ADR-012. |
| Consecuencias | (+) Las imágenes sobreviven a deploys y reinicios; (+) servidas por CDN con URL estable; (+) sin proveedor nuevo — reutiliza Supabase (ADR-010); (+) paridad de entornos: disco local en Dev/Test, almacenamiento gestionado en Prod. (-) Requiere crear el bucket y definir dos variables de entorno en Render antes de que las imágenes funcionen; (-) la `service_role` key es una credencial sensible (vive **solo** en el backend, nunca en el frontend); (-) las imágenes subidas **antes** de este cambio ya se perdieron y no son recuperables; (-) **corrige la descripción «genérico/local» de este documento y de `Backend.md`** y una discrepancia de nomenclatura (ver nota) — obliga a actualizar `Arquitectura.md` (diagrama de componentes, árbol de carpetas, escalabilidad, trazabilidad), `Backend.md`, `Deployment.md`, `BD.md` y `README.md` en cascada (cumplido en esta versión). |

#### Configuración por ambiente

| Variable                | Dev / Test        | Staging / Producción (Render)                     |
|-------------------------|-------------------|---------------------------------------------------|
| `Supabase__Url`         | *(sin definir)*   | `https://<ref>.supabase.co`                       |
| `Supabase__ServiceKey`  | *(sin definir)*   | `service_role` key del proyecto (Settings → API)  |
| `Supabase__Bucket`      | *(sin definir)*   | `uploads` (por defecto si se omite)               |
| Implementación activa   | `AlmacenamientoLocalService` (disco `wwwroot/uploads`) | `AlmacenamientoSupabaseService` (bucket público + CDN) |

> **Nota de nomenclatura.** Los documentos de arquitectura llamaban a este componente `FileStorageService`. El nombre real, conforme a la convención de código en español, es **`IAlmacenamientoService`** con las implementaciones `AlmacenamientoLocalService` y `AlmacenamientoSupabaseService`. La cascada de esta versión corrige la referencia en todos los `.md`.

---

## 12. Principios de Diseño Aplicados

| Principio                  | Aplicación en el Proyecto                                                            |
|----------------------------|--------------------------------------------------------------------------------------|
| S — SRP                    | Cada clase tiene una única responsabilidad. Controllers solo rutean.                 |
| O — OCP                    | Nuevos features se agregan extendiendo, no modificando código existente.             |
| L — LSP                    | Las implementaciones de repositorios son intercambiables por sus interfaces.         |
| I — ISP                    | Interfaces específicas por repositorio. Sin interfaces "dios".                       |
| D — DIP                    | Application depende de interfaces (Domain), no de implementaciones (Infrastructure). |
| DRY                        | Sin duplicación de lógica. Behaviors reutilizables. Componentes React compartidos.   |
| KISS                       | Soluciones simples primero. Complejidad justificada y documentada.                   |
| YAGNI                      | Sin sobre-ingeniería. Solo lo que el MVP necesita.                                   |
| Boy Scout Rule             | Cada commit deja el código mejor de lo que lo encontró.                              |
| Separation of Concerns     | UI, lógica de negocio, acceso a datos y seguridad en capas separadas.                |

---

## 13. Atributos de Calidad (mapeo a RNF)

| Atributo        | Cómo se logra                                                              | RNF          |
|-----------------|----------------------------------------------------------------------------|--------------|
| Seguridad       | JWT, roles/permisos, OWASP, hashing, auditoría                             | RNF-001..005 |
| Rendimiento     | Índices, paginación, consultas eficientes, lazy loading, code splitting    | RNF-010..012 |
| Escalabilidad   | Geografía por datos, capas desacopladas, cloud ready, contenedores         | RNF-020..022 |
| Disponibilidad  | Backups, DRP, logging estructurado                                         | RNF-030..032 |
| Usabilidad      | Responsive Mobile First, WCAG                                              | RNF-040..042 |
| Mantenibilidad  | Clean Architecture, CQRS, Feature Based, SOLID, cobertura >=90%            | RNF-050..053 |

---

## 14. Estrategia de Escalabilidad

### 14.1 Fases de Crecimiento

| Fase | Alcance         | Estrategia                                                                        |
|------|-----------------|-------------------------------------------------------------------------------------|
| 1    | Ayacucho (MVP)  | Clean Architecture en capas. Vercel (frontend) + Render (backend) + Supabase. UBIGEO Perú completo. |
| 2    | Región Ayacucho | Mismo código. Optimización de índices. Caching en queries frecuentes.               |
| 3    | Todo el Perú    | Escalado vertical (RAM/CPU) del host de aplicación. Upgrade de plan Supabase (más cómputo/conexiones). Caching con Redis. CDN para imágenes. |
| 4    | Cloud           | App Service (Azure/AWS/GCP) para backend+frontend. Supabase se mantiene o se evalúa réplica/Azure Database for PostgreSQL según necesidad. Load Balancer. Auto-scaling. |
| 5    | Evolución       | Extracción de bounded contexts críticos si el volumen lo justifica.                 |

### 14.2 Preparación Actual para Escalabilidad

| Aspecto                  | Cómo está preparado                                                   |
|--------------------------|-----------------------------------------------------------------------|
| Base de datos geográfica | UBIGEO completo del Perú desde V1. Sin cambios de esquema al escalar. |
| Paginación               | Obligatoria en todos los endpoints de lista. Sin cargas completas.    |
| Índices                  | Definidos en BD.md para columnas de búsqueda y filtrado frecuentes.   |
| Imágenes                 | `IAlmacenamientoService` con dos implementaciones: disco local (Dev/Test) y **Supabase Storage** con CDN (Staging/Prod, ADR-013). La interfaz permite migrar a Azure Blob / S3 sin tocar el resto del código. |
| Configuración            | Variables de entorno — sin cambios de código al migrar ambientes.     |
| Contenedores             | Backend empaquetado en Docker (Dockerfile) — portable entre Render y cualquier otro host Docker (Azure/AWS/GCP/VPS) sin cambios. Docker Compose se mantiene para desarrollo local. |
| Módulos desacoplados     | Features independientes en Frontend. Bounded contexts en Backend.     |

---

## 15. Trazabilidad y Aprobación

### 15.1 Trazabilidad

| Vista de arquitectura | Responde a               | Detalle en               |
|-----------------------|--------------------------|--------------------------|
| Lógica (capas)        | RNF-050, ADR-001         | Backend.md               |
| Datos                 | RNF-011/021, RN-013/062  | BD.md                    |
| Seguridad             | RNF-001..005, ADR-004    | Seguridad.md             |
| Integración           | API REST, MediatR        | API.md                   |
| Despliegue            | RNF-022, ADR-009, ADR-011 | Docker.md, Deployment.md |
| Almacenamiento de imágenes | ADR-013              | Deployment.md, Backend.md, BD.md |
| C4 / componentes      | Requisitos + UML         | UML.md                   |

### 15.2 Aprobación

| Rol                           | Nombre            | Aprobación  | Fecha |
|-------------------------------|-------------------|-------------|-------|
| Product Owner                 | —                 | ☐ PENDIENTE | —     |
| Arquitecto de Software Senior | Equipo Enterprise | ☐ PENDIENTE | —     |
| Arquitecto Empresarial Senior | Equipo Enterprise | ☐ PENDIENTE | —     |
| Especialista Seguridad        | Equipo Enterprise | ☐ PENDIENTE | —     |
| Especialista DevOps           | Equipo Enterprise | ☐ PENDIENTE | —     |

---

> **GATE DE CALIDAD — FASE 2, PASO 6:**
> Este documento debe ser revisado y **formalmente aprobado** antes de iniciar el Paso 7 (`BD.md`).
> La arquitectura aquí definida es la fuente oficial para toda implementación de código.
> Ninguna desviación arquitectónica es permitida sin un ADR aprobado que la justifique.

---

*Documento generado bajo la metodología SDD — Plataforma Inteligente de Intercambio de Objetos — Ayacucho, Perú.*
