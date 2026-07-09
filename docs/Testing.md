# Testing.md
# Plataforma Inteligente de Intercambio de Objetos

> **Documento:** Estrategia y Plan de Pruebas
> **Fase SDLC:** 2 (Diseño) / base para Fase 4 (Pruebas)
> **Versión:** 1.1.0
> **Estado:** `PENDIENTE DE APROBACIÓN`
> **Fecha:** 2026-07-08
> **Autor:** Equipo Enterprise Senior (QA Senior / Arquitecto)
> **Documentos padre:** Requisitos.md | ReglasNegocio.md | CasosDeUso.md | HistoriasUsuario.md | MatrizTrazabilidad.md | API.md | Seguridad.md
> **Convenciones:** Documentación en español. Diagramas en ASCII.

---

## Control de Versiones

| Versión | Fecha      | Autor                    | Cambios          |
|---------|------------|--------------------------|------------------|
| 1.0.0   | 2026-06-03 | Equipo Enterprise Senior | Versión inicial. |
| 1.1.0   | 2026-07-08 | Equipo Enterprise Senior | Testcontainers migrado de SQL Server a PostgreSQL, consistente con la migración de motor de BD a Supabase (ver ADR-010 en Arquitectura.md). |

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
12. Trazabilidad y Aprobación

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

---

## 2. Pirámide de Pruebas

```
                  ╱ ╲
                 ╱   ╲        E2E (pocas)
                ╱ E2E ╲       Flujos completos críticos
               ╱────── ╲      ~10%
              ╱         ╲
             ╱Integración╲    API + BD, componentes React
            ╱──────────── ╲   ~30%
           ╱               ╲
          ╱   Unitarias     ╲ Lógica de dominio, handlers,
         ╱                   ╲ validators, utils
        ╱──────────────────── ╲ ~60%
```

Mayor cantidad de pruebas unitarias (rápidas, baratas), menos de integración, mínimas E2E (lentas, costosas pero realistas).

---

## 3. Herramientas

### 3.1 Backend (.NET 10)

| Herramienta                    | Uso                                                |
|--------------------------------|----------------------------------------------------|
| xUnit                          | Framework de pruebas unitarias e integración.      |
| Moq                            | Mocking de dependencias (repositorios, servicios). |
| FluentAssertions               | Aserciones legibles y expresivas.                  |
| WebApplicationFactory          | Pruebas de integración de la API.                  |
| Testcontainers (PostgreSQL)     | BD efímera en Docker para pruebas de integración (imagen `postgres:15-alpine`). No usa el proyecto Supabase real. |
| Coverlet                       | Medición de cobertura de código.                   |

### 3.2 Frontend (React + TS)

| Herramienta               | Uso                                   |
|---------------------------|---------------------------------------|
| Vitest                    | Runner de pruebas unitarias.          |
| React Testing Library     | Pruebas de componentes.               |
| MSW (Mock Service Worker) | Mock de llamadas a la API.            |
| Playwright (futuro)       | Pruebas End To End.                   |

---

## 4. Tipos de Pruebas

| Tipo          | Qué verifica                                               | Nivel         |
|---------------|------------------------------------------------------------|---------------|
| Unitarias     | Lógica de dominio, handlers, validators, funciones puras.  | Backend/Front |
| Integración   | Interacción API ↔ Application ↔ Infrastructure ↔ BD.       | Backend       |
| Componentes   | Renderizado y comportamiento de componentes React.         | Frontend      |
| Funcionales   | Que un caso de uso completo funcione según especificación. | E2E           |
| End To End    | Flujos críticos de extremo a extremo.                      | E2E           |
| Regresión     | Que cambios nuevos no rompan lo existente.                 | Todos         |
| Aceptación    | Criterios de aceptación de las historias (HU-XXX).         | E2E           |
| Seguridad     | Mitigaciones OWASP, auth, autorización, rate limiting.     | Integración   |
| Rendimiento   | Tiempo de respuesta y consultas críticas.                  | No funcional  |

---

## 5. Estructura de Proyectos de Test

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

/frontend/src
  └── (cada feature incluye __tests__/ junto a sus componentes)
        features/auth/__tests__/LoginForm.test.tsx
        features/objects/__tests__/ObjectForm.test.tsx
```

---

## 6. Catálogo de Casos de Prueba (PR-XXX)

Desarrolla los identificadores declarados en `MatrizTrazabilidad.md`.

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
| PR-080 | Cobertura de pruebas ≥ 90%                              | RNF-053              | No funcional    |
| PR-081 | Mitigaciones OWASP Top 10                               | RNF-003, CE-004      | Seguridad       |
| PR-082 | Rendimiento de consultas críticas                       | RNF-011              | No funcional    |
| PR-083 | Validación de arquitectura (Clean/CQRS/Feature)         | RNF-050, RNF-051     | Revisión        |

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
□ Cobertura cumple el umbral del ámbito.
□ Criterios de aceptación de la historia verificados.
□ Sin defectos críticos ni altos abiertos.
□ Documentación actualizada (Backend.md / Frontend.md / API.md / Swagger).
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
                                  [Bloquea]  [Quality Gate OK]
                                                 │
                                                 ▼
                                            [Deploy]
```

Las pruebas se ejecutan automáticamente en cada push/PR. Un fallo de prueba o cobertura insuficiente detiene el pipeline (detalle en `CICD.md`).

---

## 12. Trazabilidad y Aprobación

### 12.1 Trazabilidad

Cada caso de prueba (PR-XXX) está vinculado a un requisito o regla en `MatrizTrazabilidad.md`. La cadena completa:

`Objetivo → Requisito → Regla → Caso de Uso → Historia → Prueba`

queda cerrada con este documento: ningún requisito o regla queda sin verificación.

### 12.2 Aprobación

| Rol                      | Nombre            | Aprobación  | Fecha |
|--------------------------|-------------------|-------------|-------|
| QA Senior (A/R)          | Equipo Enterprise | ☐ PENDIENTE | —     |
| Arquitecto de Software   | Equipo Enterprise | ☐ PENDIENTE | —     |
| Especialista en Seguridad| Equipo Enterprise | ☐ PENDIENTE | —     |

---

> **GATE DE CALIDAD:**
> La cobertura mínima del 90% y la ausencia de defectos críticos son criterios de salida
> obligatorios de la Fase 4 (Pruebas) antes de desplegar.

---

*Documento generado bajo la metodología SDD — Plataforma Inteligente de Intercambio de Objetos — Ayacucho, Perú.*
