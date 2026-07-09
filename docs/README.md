# Plataforma Inteligente de Intercambio de Objetos

> Plataforma web para la gestión **segura, trazable y geolocalizada** de intercambios de objetos entre ciudadanos. Fomenta la economía circular, la reutilización y la confianza comunitaria.
>
> **Ubicación inicial:** Ayacucho, Perú · **Escalabilidad:** nacional, sin rediseños estructurales.

![estado](https://img.shields.io/badge/estado-en%20desarrollo-blue) ![versión](https://img.shields.io/badge/versión-1.0.0-green) ![licencia](https://img.shields.io/badge/licencia-MIT-lightgrey)

---

## Tabla de Contenidos

1. Descripción
2. Problemática y Justificación
3. Características Principales
4. Stack Tecnológico
5. Arquitectura
6. Requisitos Previos
7. Instalación y Ejecución
8. Estructura del Repositorio
9. Documentación (SDD)
10. Convenciones
11. Roadmap
12. Equipo
13. Licencia

---

## 1. Descripción

La Plataforma Inteligente de Intercambio de Objetos permite a los ciudadanos publicar objetos en desuso e intercambiarlos de forma segura, con identidad verificada, geolocalización y un sistema de reputación. Su objetivo es formalizar y dar confianza a una práctica que hoy ocurre de manera informal e insegura.

El desarrollo sigue estrictamente la metodología **SDD (Spec Driven Development)**: la documentación es la fuente de verdad y precede a todo el código.

## 2. Problemática y Justificación

Miles de objetos en desuso tienen valor para otras personas, pero los canales actuales (redes sociales, mensajería) carecen de organización, trazabilidad, geolocalización, reputación y control. La plataforma resuelve esto y aporta valor **ambiental** (economía circular, menos residuos), **social** (confianza, comunidad) y **económico** (acceso a bienes sin costo monetario).

## 3. Características Principales

- Registro con identidad y ubicación verificables (no anónimo).
- Publicación de objetos con imágenes y geolocalización.
- Búsqueda avanzada con filtros geográficos y por categoría.
- Intercambio de trueque (objeto por objeto) con doble confirmación.
- Sistema de reputación y calificaciones.
- Reportes, favoritos, mensajería y notificaciones.
- Panel administrativo con dashboard y auditoría.
- Seguridad JWT, roles/permisos y mitigaciones OWASP.

## 4. Stack Tecnológico

**Backend**
- .NET 10 · ASP.NET Core Web API
- Entity Framework Core · PostgreSQL 15+ (Supabase, gestionado)
- MediatR (CQRS) · AutoMapper · FluentValidation
- Serilog · Swagger · JWT

**Frontend**
- React · TypeScript · Vite
- TailwindCSS · Axios · React Router DOM
- React Hook Form · Zod · Zustand

**DevOps**
- Docker · Docker Compose (desarrollo local)
- Git · GitHub · GitHub Actions (CI/CD)
- **Despliegue:** Vercel (frontend) · Render (backend) · Supabase (BD) — plan gratuito

## 5. Arquitectura

- **Backend:** Clean Architecture en capas (Domain, Application, Infrastructure, API) + CQRS + Repository + Unit Of Work.
- **Frontend:** Feature Based Architecture.
- **Flujo:** Frontend → REST API (/api/v1) → Backend → PostgreSQL (Supabase).

```
Frontend (React) ──REST/JWT──► API (.NET 10) ──EF Core (Npgsql)──► PostgreSQL (Supabase)
                                  │
                    Domain ◄─ Application ◄─ Infrastructure
```

Detalle completo en [`docs/Arquitectura.md`](docs/Arquitectura.md).

## 6. Requisitos Previos

- [Docker](https://www.docker.com/) y Docker Compose (solo para desarrollo local — ver sección 7)
- [.NET 10 SDK](https://dotnet.microsoft.com/) (para desarrollo backend)
- [Node.js LTS](https://nodejs.org/) y npm (para desarrollo frontend)
- Git
- Cuentas de [Vercel](https://vercel.com/) y [Render](https://render.com/) (solo para desplegar — no se requieren para desarrollo local)

## 7. Instalación y Ejecución

> Las instrucciones a continuación son para **desarrollo local**. El despliegue público (Staging/Producción) se hace en Vercel + Render — ver [`docs/Deployment.md`](docs/Deployment.md) y [ADR-011 en `docs/Arquitectura.md`](docs/Arquitectura.md).

```bash
# 1. Clonar el repositorio
git clone https://github.com/<organizacion>/exchange-platform.git
cd exchange-platform

# 2. Configurar variables de entorno
cp .env.example .env
# Editar .env con los valores del ambiente (ver Docker.md)

# 3. Levantar todo con Docker Compose (local)
docker compose up -d --build

# 4. Aplicar migraciones de base de datos
docker compose exec backend dotnet ef database update
```

Servicios disponibles (local):

| Servicio | URL                               |
|----------|-----------------------------------|
| Frontend | http://localhost:3000             |
| Backend  | http://localhost:5000             |
| Swagger  | http://localhost:5000/api/swagger |

Para desarrollo sin Docker, consultar [`docs/Backend.md`](docs/Backend.md) y [`docs/Frontend.md`](docs/Frontend.md) (Fase 3).

### 7.1 Despliegue Público

| Servicio | Plataforma | Detalle |
|----------|-----------|---------|
| Frontend | [Vercel](https://vercel.com/) | Build automático (`npm run build`) en cada push a `main`/`develop`. |
| Backend  | [Render](https://render.com/) | Deploy automático desde el Dockerfile de `backend/` en cada push. |
| Base de datos | [Supabase](https://supabase.com/) | PostgreSQL gestionado (sin cambios — ADR-010). |

Procedimiento completo, variables de entorno por ambiente y rollback en [`docs/Deployment.md`](docs/Deployment.md).

## 8. Estructura del Repositorio

```
exchange-platform/
├── docs/              ← Documentación SDD (fuente de verdad)
├── backend/           ← API .NET 10 (Clean Architecture)
│   ├── src/
│   │   ├── ExchangePlatform.Domain/
│   │   ├── ExchangePlatform.Application/
│   │   ├── ExchangePlatform.Infrastructure/
│   │   └── ExchangePlatform.API/
│   └── tests/
├── frontend/          ← SPA React + TypeScript (Feature Based)
│   └── src/
├── database/          ← Scripts y seed (UBIGEO, datos maestros)
├── scripts/           ← Utilidades (cobertura, smoke tests)
├── deployment/        ← Configuración de despliegue
├── docker-compose.yml
└── README.md
```

## 9. Documentación (SDD)

Toda la documentación vive en `/docs` y es la fuente oficial de verdad. Orden de la metodología SDD y documentos complementarios:

**Fase 1 — Análisis**
- [VisionProyecto.md](docs/VisionProyecto.md) · [Requisitos.md](docs/Requisitos.md) · [ReglasNegocio.md](docs/ReglasNegocio.md)
- [CasosDeUso.md](docs/CasosDeUso.md) · [HistoriasUsuario.md](docs/HistoriasUsuario.md) · [MatrizTrazabilidad.md](docs/MatrizTrazabilidad.md)
- [Riesgos.md](docs/Riesgos.md) · [Glosario.md](docs/Glosario.md)

**Fase 2 — Diseño**
- [UML.md](docs/UML.md) · [Arquitectura.md](docs/Arquitectura.md) · [BD.md](docs/BD.md) · [API.md](docs/API.md)
- [Seguridad.md](docs/Seguridad.md) · [Testing.md](docs/Testing.md) · [UX.md](docs/UX.md) · [UI.md](docs/UI.md)
- [Convenciones.md](docs/Convenciones.md) · [DatosGeograficos.md](docs/DatosGeograficos.md)

**Proceso / DevOps**
- [GitFlow.md](docs/GitFlow.md) · [Docker.md](docs/Docker.md) · [CICD.md](docs/CICD.md) · [Deployment.md](docs/Deployment.md)

**Gestión y cierre**
- [Roadmap.md](docs/Roadmap.md) · [ChecklistCalidad.md](docs/ChecklistCalidad.md)

> Pendientes de Fase 3: `Backend.md`, `Frontend.md`, `Prototipo.md` (se generan al implementar).

## 10. Convenciones

- **Documentación:** español · **Código/entidades:** español (Usuario, Objeto, Intercambio) · **Rutas REST:** inglés.
- **Commits:** Conventional Commits (`feat:`, `fix:`, `docs:`...).
- **Ramas:** GitFlow (main, develop, feature/*, release/*, hotfix/*).

Detalle en [`docs/Convenciones.md`](docs/Convenciones.md) y [`docs/GitFlow.md`](docs/GitFlow.md).

## 11. Roadmap

| Versión | Tema         | Enfoque                                   |
|---------|--------------|-------------------------------------------|
| V1      | MVP          | Intercambio seguro y geolocalizado.       |
| V2      | Comunidad    | Chat, favoritos, reputación, mapas.       |
| V3      | Analítica    | Dashboard avanzado, KPIs, observabilidad. |
| V4      | Inteligencia | Recomendaciones y detección de fraude.    |
| V5      | Movilidad    | App móvil Android/iOS.                    |

Detalle en [`docs/Roadmap.md`](docs/Roadmap.md).

## 12. Equipo

Proyecto desarrollado por el **Equipo Enterprise Senior** bajo metodología SDD: Arquitectura de Software, Empresarial, Soluciones y Datos; Backend (.NET); Frontend (React); DBA; DevOps; QA; Seguridad; UX/UI.

## 13. Licencia

Distribuido bajo licencia **MIT**. Ver el archivo `LICENSE` para más información.

---

*Plataforma Inteligente de Intercambio de Objetos — Ayacucho, Perú. Documentación y desarrollo guiados por la metodología SDD (Spec Driven Development).*
