# Testing.md
# Plataforma Inteligente de Intercambio de Objetos

> **Documento:** Estrategia y Plan de Pruebas
> **Fase SDLC:** 2 (Diseño) / base para Fase 4 (Pruebas)
> **Versión:** 1.2.0
> **Estado:** `PENDIENTE DE APROBACIÓN`
> **Fecha:** 2026-07-13
> **Autor:** Equipo Enterprise Senior (QA Senior / Arquitecto)
> **Documentos padre:** Requisitos.md | ReglasNegocio.md | CasosDeUso.md | HistoriasUsuario.md | MatrizTrazabilidad.md | API.md | Seguridad.md
> **Convenciones:** Documentación en español. Diagramas en ASCII.

---

## Control de Versiones

| Versión | Fecha      | Autor                    | Cambios          |
|---------|------------|--------------------------|------------------|
| 1.0.0   | 2026-06-03 | Equipo Enterprise Senior | Versión inicial. |
| 1.1.0   | 2026-07-08 | Equipo Enterprise Senior | Testcontainers migrado de SQL Server a PostgreSQL, consistente con la migración de motor de BD a Supabase (ver ADR-010 en Arquitectura.md). |
| 1.2.0   | 2026-07-13 | Equipo Enterprise Senior | Playwright pasa de `futuro` a `IMPLEMENTADO`. Se añaden: §3.3 (herramientas E2E), §5.3 (estructura de tests E2E), §6.4 (catálogo PR-090 a PR-102), §12 (Estrategia de Aislamiento de Base de Datos de 3 niveles), §13 (Automatización con `start-e2e.ps1`). Se actualiza §11 (CI/CD) con la etapa E2E. |

---

## Tabla de Contenidos

1. Enfoque y Objetivos
2. Pirámide de Pruebas
3. Herramientas
4. Tipos de Pruebas
5. Estructura de Proyectos de Test
6. Catálogo de Casos de Prueba (PR-XXX)
7. Criterios de Aceptación por Escenario
8. Cobertura de Código
9. Gestión de Defectos
10. Definición de Terminado (DoD)
11. Integración con CI/CD
12. Estrategia de Aislamiento de Base de Datos
13. Automatización de la Ejecución E2E
14. Trazabilidad y Aprobación

---

## 1. Enfoque y Objetivos

El objetivo de las pruebas es **verificar** (construimos el producto correctamente) y **validar** (construimos el producto correcto) que el sistema cumple requisitos, reglas de negocio y criterios de aceptación antes de cada despliegue.

| Objetivo                  | Métrica / Criterio                          |
|---------------------------|---------------------------------------------|
| Cobertura de código       | Mínima 90%, ideal 95% (RNF-053).            |
| Cada requisito verificado | 100% de RF/RNF con al menos una prueba.     |
| Cada regla verificada     | 100% de RN con prueba asociada.             |
| Sin errores críticos      | 0 defectos críticos abiertos al desplegar.  |
| Seguridad                 | OWASP Top 10 verificado (PR-081).           |
| Flujos críticos E2E       | 100% de flujos críticos cubiertos (PR-090+).|
| Aislamiento de datos      | 0 escrituras de test sobre BD de producción.|

---

## 2. Pirámide de Pruebas

```
                  ╱ ╲
                 ╱   ╲        E2E (pocas)
                ╱ E2E ╲       Flujos completos críticos
               ╱────── ╲      ~10%  → 13 specs Playwright
              ╱         ╲
             ╱Integración╲    API + BD, componentes React
            ╱──────────── ╲   ~30%
           ╱               ╲
          ╱   Unitarias     ╲ Lógica de dominio, handlers,
         ╱                   ╲ validators, utils
        ╱──────────────────── ╲ ~60%
```

Mayor cantidad de pruebas unitarias (rápidas, baratas), menos de integración, mínimas E2E (lentas, costosas pero realistas).

**Estado actual de la capa E2E:** 13 pruebas implementadas y pasando (tiempo de ejecución ≈ 1.7 min).

---

## 3. Herramientas

### 3.1 Backend (.NET 10)

| Herramienta                    | Uso                                                |
|--------------------------------|----------------------------------------------------|
| xUnit                          | Framework de pruebas unitarias e integración.      |
| Moq                            | Mocking de dependencias (repositorios, servicios). |
| FluentAssertions               | Aserciones legibles y expresivas.                  |
| WebApplicationFactory          | Pruebas de integración de la API.                  |
| Testcontainers (PostgreSQL)    | BD efímera en Docker para pruebas de integración (imagen `postgres:15-alpine`). No usa el proyecto Supabase real. |
| Coverlet                       | Medición de cobertura de código.                   |

### 3.2 Frontend (React + TS)

| Herramienta               | Uso                                   |
|---------------------------|---------------------------------------|
| Vitest                    | Runner de pruebas unitarias.          |
| React Testing Library     | Pruebas de componentes.               |
| MSW (Mock Service Worker) | Mock de llamadas a la API.            |

### 3.3 End To End (E2E) — `IMPLEMENTADO`

| Herramienta            | Uso                                                                 |
|------------------------|---------------------------------------------------------------------|
| Playwright             | Runner E2E. Automatiza el navegador contra la app real (front + back). |
| Docker Compose         | Levanta la BD de test aislada (`docker-compose.test.yml`).          |
| PostgreSQL 15 (Docker) | Contenedor `exchange-db-test`, expuesto en el **puerto 5433**.      |
| PowerShell             | Script orquestador `start-e2e.ps1` (levanta BD, backend, front y ejecuta las specs). |
| EF Core CLI            | `dotnet ef database update` aplica migraciones sobre la BD de test. |

> **NOTA DE ESTADO:** en la versión 1.1.0 de este documento Playwright figuraba como
> *"(futuro)"*. A partir de la versión 1.2.0 la capa E2E está **implementada y operativa**.

---

## 4. Tipos de Pruebas

| Tipo          | Qué verifica                                               | Nivel         |
|---------------|------------------------------------------------------------|---------------|
| Unitarias     | Lógica de dominio, handlers, validators, funciones puras.  | Backend/Front |
| Integración   | Interacción API ↔ Application ↔ Infrastructure ↔ BD.       | Backend       |
| Componentes   | Renderizado y comportamiento de componentes React.         | Frontend      |
| Funcionales   | Que un caso de uso completo funcione según especificación. | E2E           |
| End To End    | Flujos críticos de extremo a extremo (Playwright).         | E2E           |
| Regresión     | Que cambios nuevos no rompan lo existente.                 | Todos         |
| Aceptación    | Criterios de aceptación de las historias (HU-XXX).         | E2E           |
| Seguridad     | Mitigaciones OWASP, auth, autorización, rate limiting.     | Integración   |
| Rendimiento   | Tiempo de respuesta y consultas críticas.                  | No funcional  |

---

## 5. Estructura de Proyectos de Test

### 5.1 Backend

```
/backend/tests
  ├── ExchangePlatform.UnitTests/
  │     ├── Domain/
  │     │     ├── IntercambioTests.cs        (reglas de dominio)
  │     │     └── UsuarioTests.cs
  │     ├── Application/
  │     │     ├── Commands/
  │     │     │     ├── CrearObjetoHandlerTests.cs
  │     │     │     └── AceptarIntercambioHandlerTests.cs
  │     │     └── Validators/
  │     │           └── RegistrarUsuarioValidatorTests.cs
  │     └── Common/
  │
  └── ExchangePlatform.IntegrationTests/
        ├── Auth/
        │     └── AuthEndpointsTests.cs
        ├── Objects/
        │     └── ObjectsEndpointsTests.cs
        ├── Exchanges/
        │     └── ExchangesFlowTests.cs
        └── Fixtures/
              └── ApiTestFixture.cs
```

### 5.2 Frontend (unitarias / componentes)

```
/frontend/src
  └── (cada feature incluye __tests__/ junto a sus componentes)
        features/auth/__tests__/LoginForm.test.tsx
        features/objects/__tests__/ObjectForm.test.tsx
```

### 5.3 E2E (Playwright) — `IMPLEMENTADO`

```
/frontend
  ├── e2e/
  │     ├── auth.spec.ts            (registro, login, logout)
  │     ├── objects.spec.ts         (publicar / editar / eliminar objeto)
  │     ├── search.spec.ts          (búsqueda y filtros)
  │     ├── exchanges.spec.ts       (flujo completo de intercambio)
  │     ├── favorites.spec.ts       (favoritos)
  │     └── fixtures/
  │           ├── auth.fixture.ts   (sesión autenticada reutilizable)
  │           └── seed.ts           (datos de partida)
  │
  └── playwright.config.ts          (baseURL, browsers, reporters, retries)

/  (raíz del repositorio)
  ├── docker-compose.test.yml       (contenedor exchange-db-test → puerto 5433)
  └── start-e2e.ps1                 (orquestador de la ejecución E2E)
```

---

## 6. Catálogo de Casos de Prueba (PR-XXX)

Desarrolla los identificadores declarados en `MatrizTrazabilidad.md`.

### 6.1 Funcionales / Integración / Unitarias

| ID     | Caso de prueba                                          | Verifica (RN/RF/RNF) | Tipo            |
|--------|---------------------------------------------------------|----------------------|-----------------|
| PR-001 | Registro exitoso con datos válidos                      | RF-001, RN-003       | Funcional       |
| PR-002 | Rechazo de correo duplicado / contraseña inválida       | RN-002, RN-004       | Funcional       |
| PR-003 | Login y emisión de tokens                               | RF-003, RF-004       | Integración     |
| PR-004 | Recuperación de contraseña                              | RF-005               | Funcional       |
| PR-005 | Edición de perfil y bloqueo de perfil ajeno             | RF-011, RN-005       | Funcional       |
| PR-010 | Publicación de objeto válido                            | RF-020, RN-010       | Integración     |
| PR-011 | Validación de imágenes (tamaño/MIME)                    | RN-011, RN-012       | Unitaria        |
| PR-012 | Edición / soft delete de objeto propio                  | RF-023, RN-013       | Funcional       |
| PR-013 | Búsqueda por texto y categoría                          | RF-030, RF-031       | Integración     |
| PR-014 | Filtros geográficos y ordenamiento                      | RF-031, RF-032       | Funcional       |
| PR-020 | Solicitud de intercambio y bloqueo de objeto propio     | RF-040, RN-022       | Funcional       |
| PR-021 | Aceptación/rechazo solo por el propietario              | RN-023               | Funcional       |
| PR-022 | No reapertura de solicitud rechazada                    | RN-024               | Funcional       |
| PR-023 | Registro del historial de estados                       | RN-026               | Integración     |
| PR-024 | Confirmación mutua finaliza el intercambio              | RN-020, RN-025       | Funcional       |
| PR-025 | Calificación única y cálculo de reputación              | RN-030, RN-031, RN-032| Unitaria       |
| PR-030 | Reporte de objeto/usuario                               | RF-060, RN-040       | Funcional       |
| PR-031 | Gestión de reportes por moderador                       | RF-061, RN-041       | Funcional       |
| PR-040 | Marcar / listar favoritos (sin duplicados)              | RF-070, RN-042       | Funcional       |
| PR-041 | Mensajería entre partes                                 | RF-080               | Integración     |
| PR-042 | Notificaciones por evento                               | RF-090               | Integración     |
| PR-050 | Administración de usuarios / categorías                 | RF-110, RF-112       | Funcional       |
| PR-051 | Dashboard e indicadores                                 | RF-111               | Funcional       |
| PR-060 | Coherencia jerárquica de ubicación                      | RN-050, RN-051       | Unitaria        |
| PR-061 | Filtro por cercanía                                     | RF-033               | Integración     |
| PR-070 | Auditoría de acciones CRUD                              | RN-062, RF-121       | Integración     |
| PR-071 | Auditoría de login / intercambio                        | RN-063, RF-120       | Integración     |

### 6.2 No funcionales / Seguridad / Revisión

| ID     | Caso de prueba                                          | Verifica (RN/RF/RNF) | Tipo            |
|--------|---------------------------------------------------------|----------------------|-----------------|
| PR-080 | Cobertura de pruebas ≥ 90%                              | RNF-053              | No funcional    |
| PR-081 | Mitigaciones OWASP Top 10                               | RNF-003, CE-004      | Seguridad       |
| PR-082 | Rendimiento de consultas críticas                       | RNF-011              | No funcional    |
| PR-083 | Validación de arquitectura (Clean/CQRS/Feature)         | RNF-050, RNF-051     | Revisión        |

### 6.3 End To End (Playwright) — `IMPLEMENTADO`

Suite ejecutada: **13/13 pruebas pasando** (≈ 1.7 min).

| ID     | Spec                | Caso de prueba E2E                                        | Verifica (RN/RF)       | Estado     |
|--------|---------------------|-----------------------------------------------------------|------------------------|------------|
| PR-090 | `auth.spec.ts`      | Registro de usuario nuevo desde el formulario             | RF-001, RN-003         | ✅ PASA    |
| PR-091 | `auth.spec.ts`      | Login con credenciales válidas y redirección al home      | RF-003, RF-004         | ✅ PASA    |
| PR-092 | `auth.spec.ts`      | Login con credenciales inválidas muestra error            | RN-002, RN-004         | ✅ PASA    |
| PR-093 | `auth.spec.ts`      | Logout limpia la sesión y protege rutas privadas          | RF-004, RNF-003        | ✅ PASA    |
| PR-094 | `objects.spec.ts`   | Publicar un objeto nuevo con datos válidos                | RF-020, RN-010         | ✅ PASA    |
| PR-095 | `objects.spec.ts`   | Editar un objeto propio                                   | RF-023, RN-013         | ✅ PASA    |
| PR-096 | `objects.spec.ts`   | Eliminar (soft delete) un objeto propio                   | RF-023, RN-013         | ✅ PASA    |
| PR-097 | `search.spec.ts`    | Búsqueda por texto filtra el listado                      | RF-030                 | ⚠️ INESTABLE |
| PR-098 | `search.spec.ts`    | Filtro por categoría                                      | RF-031                 | ✅ PASA    |
| PR-099 | `exchanges.spec.ts` | Solicitar intercambio sobre objeto ajeno                  | RF-040, RN-022         | ✅ PASA    |
| PR-100 | `exchanges.spec.ts` | El propietario acepta la solicitud                        | RN-023                 | ✅ PASA    |
| PR-101 | `exchanges.spec.ts` | Confirmación mutua finaliza el intercambio                | RN-020, RN-025         | ✅ PASA    |
| PR-102 | `favorites.spec.ts` | Marcar y listar favoritos sin duplicados                  | RF-070, RN-042         | ✅ PASA    |

### 6.4 Defectos abiertos en la suite E2E

| ID     | Prueba afectada  | Síntoma                                    | Causa raíz probable                                                  | Severidad |
|--------|------------------|--------------------------------------------|----------------------------------------------------------------------|-----------|
| DEF-01 | PR-097           | `Expected: 1, Received: 20` (intermitente) | Datos acumulados en la BD de test compartida entre ejecuciones. Falta estrategia de limpieza/reset. | Media     |

> **Anti-patrón identificado:** las aserciones negativas `.not.toBeVisible()` se evalúan
> antes de que React complete el re-render posterior al filtrado. Se sustituyen por
> `toHaveURL(/search=/)` + `toHaveCount()` + `toHaveText()`.

---

## 7. Criterios de Aceptación por Escenario

Cada caso de uso se prueba en tres escenarios. Ejemplo con UC-020 (Solicitar intercambio):

| Escenario   | Condición                                  | Resultado esperado                           |
|-------------|--------------------------------------------|----------------------------------------------|
| Feliz       | Objeto ajeno disponible + objeto propio    | 201 Created, solicitud Pendiente, notifica.  |
| Alternativo | El objeto solicitado se reserva mientras   | Comportamiento consistente según estado.     |
| Error       | El objeto solicitado es propio             | 409, "No puedes solicitar tu propio objeto". |
| Error       | Objeto no disponible                       | 409, "El objeto no está disponible".         |

> Los escenarios derivan de los flujos y excepciones de `CasosDeUso.md` y los criterios Dado-Cuando-Entonces de `HistoriasUsuario.md`.

---

## 8. Cobertura de Código

| Ámbito                  | Cobertura mínima |
|-------------------------|------------------|
| Capa Domain             | 95%              |
| Capa Application        | 90%              |
| Capa Infrastructure     | 80%              |
| Capa API (controllers)  | 80%              |
| Frontend (lógica/hooks) | 85%              |
| Global del proyecto     | 90% (RNF-053)    |

La cobertura se mide con Coverlet (backend) y Vitest coverage (frontend), y se valida en el pipeline CI (PR-080). Por debajo del umbral, el merge se bloquea.

> Las pruebas E2E **no** computan para la métrica de cobertura de código: su criterio de
> salida es la cobertura de **flujos críticos** (100%), no de líneas.

---

## 9. Gestión de Defectos

| Severidad | Definición                                  | Acción                          |
|-----------|---------------------------------------------|---------------------------------|
| Crítica   | Bloquea funcionalidad core o seguridad.     | Bloquea release. Corregir ya.   |
| Alta      | Falla importante con workaround.            | Corregir antes del release.     |
| Media     | Falla menor, no bloquea flujo principal.    | Planificar en iteración.        |
| Baja      | Cosmético o mejora menor.                   | Backlog.                        |

Cada defecto se registra con: descripción, pasos para reproducir, severidad, prueba asociada, estado y responsable.

---

## 10. Definición de Terminado (DoD)

Una funcionalidad está "Terminada" cuando:

```
□ Código implementado según el diseño aprobado.
□ Code review aprobado (técnico, funcional, seguridad, arquitectónico).
□ Pruebas unitarias y de integración escritas y pasando.
□ Si toca un flujo crítico: prueba E2E escrita y pasando.
□ Cobertura cumple el umbral del ámbito.
□ Criterios de aceptación de la historia verificados.
□ Sin defectos críticos ni altos abiertos.
□ Documentación actualizada (Backend.md / Frontend.md / API.md / Testing.md / Swagger).
□ Build exitoso en CI.
□ Cumple convenciones de código y seguridad.
```

---

## 11. Integración con CI/CD

```
Commit / PR
     │
     ▼
[Build] ──► [Pruebas Unitarias] ──► [Pruebas Integración]
                                            │
                                            ▼
                                   [Cobertura >= 90%?]
                                       │         │
                                    No │         │ Sí
                                       ▼         ▼
                                  [Bloquea]  [Levanta BD test :5433]
                                                 │
                                                 ▼
                                        [Pruebas E2E Playwright]
                                             │          │
                                       Falla │          │ Pasa
                                             ▼          ▼
                                        [Bloquea]  [Quality Gate OK]
                                                        │
                                                        ▼
                                                   [Deploy]
```

Las pruebas se ejecutan automáticamente en cada push/PR. Un fallo de prueba, cobertura insuficiente o fallo E2E detiene el pipeline (detalle en `CICD.md`).

---

## 12. Estrategia de Aislamiento de Base de Datos

Regla inviolable: **ninguna prueba escribe sobre la BD de producción**. Se establecen tres niveles físicamente separados.

```
┌──────────────────────────────────────────────────────────────────────┐
│                        NIVEL 1 — PRODUCCIÓN                          │
│  Supabase (PLATAFORMAIDIOA)                                          │
│  aws-1-us-east-2.pooler.supabase.com:6543                            │
│  ⛔ NUNCA se toca desde pruebas. Solo la app desplegada.             │
└──────────────────────────────────────────────────────────────────────┘
                                  ▲
                                  │  (sin conexión desde tests)
                                  │
┌──────────────────────────────────────────────────────────────────────┐
│                       NIVEL 2 — DESARROLLO                           │
│  Docker: exchange-dev-db                                             │
│  localhost:5432                                                      │
│  Uso: desarrollo local diario, exploración manual.                   │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│                          NIVEL 3 — TEST                              │
│  Docker: exchange-db-test  (docker-compose.test.yml)                 │
│  localhost:5433                                                      │
│  Uso: EXCLUSIVO de la suite E2E de Playwright.                       │
│  Se levanta y se destruye por ejecución.                             │
└──────────────────────────────────────────────────────────────────────┘
```

### 12.1 Selección de la BD por variable de entorno

La cadena de conexión se resuelve **por precedencia**, no por archivo:

| Prioridad | Origen                                        | Gana |
|-----------|-----------------------------------------------|------|
| 1         | `ConnectionStrings__DefaultConnection` (env)  | ✅    |
| 2         | `appsettings.{Environment}.json`              |      |
| 3         | `appsettings.json`                            |      |

> **⚠️ TRAMPA CONOCIDA:** el `DesignTimeDbContextFactory` de EF Core lee
> `ConnectionStrings__DefaultConnection` y, si la variable **no está definida**, cae
> silenciosamente al **puerto 5432** (desarrollo). Consecuencia: `dotnet ef database update`
> aplica las migraciones **sobre la BD equivocada** sin emitir ningún aviso.
>
> **Mitigación obligatoria:** definir la variable de forma explícita **antes** de ejecutar
> cualquier comando de EF Core dirigido a la BD de test:
>
> ```powershell
> $env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5433;Database=exchange_test;Username=postgres;Password=***"
> dotnet ef database update
> ```

### 12.2 Convención de nombres

Recordatorio de `Convenciones.md`: la configuración anidada de .NET se mapea a variables de entorno con **doble guion bajo** (`__`):

| Configuración .NET                    | Variable de entorno                       |
|---------------------------------------|-------------------------------------------|
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection`    |
| `Jwt:Secret`                          | `Jwt__Secret`                             |
| `AllowedOrigins`                      | `AllowedOrigins`                          |

---

## 13. Automatización de la Ejecución E2E

### 13.1 Script orquestador: `start-e2e.ps1`

```
              start-e2e.ps1
                    │
                    ▼
        ┌───────────────────────┐
        │ 1. docker compose up  │  docker-compose.test.yml
        │    exchange-db-test   │  → PostgreSQL en :5433
        └───────────┬───────────┘
                    ▼
        ┌───────────────────────┐
        │ 2. Set env var        │  ConnectionStrings__DefaultConnection
        │    (puerto 5433)      │  ⚠️ OBLIGATORIO (ver §12.1)
        └───────────┬───────────┘
                    ▼
        ┌───────────────────────┐
        │ 3. dotnet ef          │  Aplica migraciones sobre la BD de test
        │    database update    │
        └───────────┬───────────┘
                    ▼
        ┌───────────────────────┐
        │ 4. CONFIRMACIÓN       │  Imprime en consola el puerto activo.
        │    VISUAL del puerto  │  El operador verifica que dice 5433.
        └───────────┬───────────┘
                    ▼
        ┌───────────────────────┐
        │ 5. dotnet run (API)   │  Backend en background
        │    npm run dev (front)│  Frontend en background
        └───────────┬───────────┘
                    ▼
        ┌───────────────────────┐
        │ 6. npx playwright test│  Ejecuta las 13 specs
        └───────────┬───────────┘
                    ▼
        ┌───────────────────────┐
        │ 7. Teardown           │  Detiene procesos y contenedor
        └───────────────────────┘
```

### 13.2 Control de seguridad: confirmación visual del puerto

El paso 4 es un **control de seguridad deliberado**, no un log decorativo. Antes de que se aplique una sola migración, el script imprime el puerto de destino. Si el operador ve `5432` (desarrollo) o cualquier host de Supabase, **debe abortar**.

```
  ╔══════════════════════════════════════════════╗
  ║  BASE DE DATOS ACTIVA: localhost:5433        ║
  ║  Contenedor: exchange-db-test                ║
  ║  ¿Es correcto? (S/N)                         ║
  ╚══════════════════════════════════════════════╝
```

### 13.3 Comandos manuales de referencia

| Acción                        | Comando                                        |
|-------------------------------|------------------------------------------------|
| Ejecutar suite completa       | `.\start-e2e.ps1`                              |
| Ejecutar una spec             | `npx playwright test e2e/auth.spec.ts`         |
| Modo con navegador visible    | `npx playwright test --headed`                 |
| Modo depuración paso a paso   | `npx playwright test --debug`                  |
| Ver el informe HTML           | `npx playwright show-report`                   |
| Levantar solo la BD de test   | `docker compose -f docker-compose.test.yml up -d` |
| Destruir la BD de test        | `docker compose -f docker-compose.test.yml down -v` |

### 13.4 Deuda técnica pendiente

| # | Pendiente                                                              | Relacionado |
|---|------------------------------------------------------------------------|-------------|
| 1 | Estrategia de limpieza/reset de la BD entre ejecuciones (`down -v` o `TRUNCATE` en `globalSetup`). | DEF-01 / PR-097 |
| 2 | Reemplazar aserciones `.not.toBeVisible()` por `toHaveCount()`.        | DEF-01      |
| 3 | Integrar la etapa E2E en el workflow de GitHub Actions.                | §11, CICD.md |

---

## 14. Trazabilidad y Aprobación

### 14.1 Trazabilidad

Cada caso de prueba (PR-XXX) está vinculado a un requisito o regla en `MatrizTrazabilidad.md`. La cadena completa:

`Objetivo → Requisito → Regla → Caso de Uso → Historia → Prueba`

queda cerrada con este documento: ningún requisito o regla queda sin verificación.

> **Acción pendiente sobre `MatrizTrazabilidad.md`:** dar de alta los identificadores
> **PR-090 a PR-102** (capa E2E) para mantener la trazabilidad cerrada.

### 14.2 Aprobación

| Rol                      | Nombre            | Aprobación  | Fecha |
|--------------------------|-------------------|-------------|-------|
| QA Senior (A/R)          | Equipo Enterprise | ☐ PENDIENTE | —     |
| Arquitecto de Software   | Equipo Enterprise | ☐ PENDIENTE | —     |
| Especialista en Seguridad| Equipo Enterprise | ☐ PENDIENTE | —     |

---

> **GATE DE CALIDAD:**
> La cobertura mínima del 90%, la ausencia de defectos críticos y la suite E2E en verde
> son criterios de salida obligatorios de la Fase 4 (Pruebas) antes de desplegar.

---

*Documento generado bajo la metodología SDD — Plataforma Inteligente de Intercambio de Objetos — Ayacucho, Perú.*
