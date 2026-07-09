# MatrizTrazabilidad.md

> **Documento:** Matriz de Trazabilidad
> **Fase SDLC:** 1 (Análisis) — consolidación y cierre
> **Estado:** `BORRADOR — PENDIENTE DE APROBACIÓN`
> **Versión:** 0.1.0
> **Fecha:** 2026-06-03
> **Autor:** Equipo Enterprise (Arquitecto Empresarial / QA Senior)
> **Depende de:** `VisionProyecto.md`, `Requisitos.md`, `ReglasNegocio.md`, `CasosDeUso.md`, `HistoriasUsuario.md`
> **Principio rector:** *Nada existe sin trazabilidad.* Todo objetivo se realiza en requisitos, reglas, casos, historias, un módulo y al menos una prueba.

---

## 1. Introducción

Esta matriz consolida y verifica la coherencia de toda la especificación de la Fase 1. Permite trazado **hacia adelante** (Objetivo → … → Prueba) y **hacia atrás** (Prueba → … → Objetivo), y es la herramienta base para el **análisis de impacto** ante cualquier cambio futuro. Los identificadores de prueba (PR-XXX) se desarrollarán en `Testing.md` durante la Fase de Pruebas.

Cadena de trazabilidad:

`Objetivo (OE) → Requisito (RF/RNF) → Regla (RN) → Caso de Uso (UC) → Historia (HU) → Módulo → Prueba (PR)`

## 2. Matriz Maestra (trazado hacia adelante)

| OE     | Requisitos                                           | Reglas                         | Casos de Uso                   | Historias              | Módulo                 | Pruebas                |
|--------|------------------------------------------------------|--------------------------------|--------------------------------|------------------------|------------------------|------------------------|
| OE-001 | RF-001, RF-002                                       | RN-002, RN-003, RN-004         | UC-001                         | HU-001                 | auth                   | PR-001, PR-002         |
| OE-002 | RF-003..RF-007, RF-010, RF-011                       | RN-001, RN-061, RN-063, RN-064 | UC-002, UC-003, UC-004, UC-012 | HU-002, HU-003, HU-010 | auth, users            | PR-003, PR-004, PR-005 |
| OE-003 | RF-020..RF-023, RF-025                               | RN-001, RN-010..RN-013         | UC-010, UC-011                 | HU-020, HU-021         | objects                | PR-010, PR-011, PR-012 |
| OE-004 | RF-024, RF-030..RF-032                               | RN-014                         | UC-030                         | HU-030                 | objects                | PR-013, PR-014         |
| OE-005 | RF-040..RF-043, RF-045                               | RN-020..RN-024                 | UC-020, UC-021                 | HU-040, HU-041         | exchanges              | PR-020, PR-021, PR-022 |
| OE-006 | RF-044                                               | RN-026                         | UC-021, UC-022                 | HU-041, HU-042         | exchanges              | PR-023                 |
| OE-007 | RF-012, RF-050..RF-052                               | RN-025, RN-030..RN-032         | UC-022                         | HU-042                 | exchanges, users       | PR-024, PR-025         |
| OE-008 | RF-060, RF-061                                       | RN-040..RN-042                 | UC-040, UC-041                 | HU-050, HU-051         | admin                  | PR-030, PR-031         |
| OE-009 | RF-070, RF-071, RF-080, RF-081, RF-090, RF-091       | —                              | UC-050, UC-060, UC-070         | HU-060, HU-070, HU-080 | notifications, objects | PR-040, PR-041, PR-042 |
| OE-010 | RF-110..RF-112                                       | RN-060                         | UC-080, UC-081                 | HU-090, HU-091         | admin                  | PR-050, PR-051         |
| OE-011 | RF-033, RF-100, RF-101                               | RN-050..RN-052                 | UC-031                         | HU-030                 | objects, users         | PR-060, PR-061         |
| OE-012 | RF-120, RF-121                                       | RN-062, RN-063                 | UC-090                         | HU-092                 | admin                  | PR-070, PR-071         |
| OE-013 | RNF-010..RNF-012, RNF-020..RNF-022, RNF-050..RNF-053 | —                              | (transversal)                  | (transversal)          | (arquitectura)         | PR-080..PR-083         |

## 3. Verificación por Caso de Uso (trazado hacia atrás)

Cada caso de uso justifica su existencia en un objetivo y queda cubierto por al menos una prueba:

| Caso de Uso | Origen (OE) | Historia | Prueba                 |
|-------------|-------------|----------|------------------------|
| UC-001      | OE-001      | HU-001   | PR-001, PR-002         |
| UC-002      | OE-002      | HU-002   | PR-003                 |
| UC-003      | OE-002      | HU-003   | PR-004                 |
| UC-004      | OE-002      | —        | PR-005                 |
| UC-010      | OE-003      | HU-020   | PR-010, PR-011         |
| UC-011      | OE-003      | HU-021   | PR-012                 |
| UC-012      | OE-002      | HU-010   | PR-005                 |
| UC-020      | OE-005      | HU-040   | PR-020                 |
| UC-021      | OE-005/006  | HU-041   | PR-021, PR-022, PR-023 |
| UC-022      | OE-006/007  | HU-042   | PR-024, PR-025         |
| UC-030      | OE-004      | HU-030   | PR-013, PR-014         |
| UC-031      | OE-011      | HU-030   | PR-060, PR-061         |
| UC-040      | OE-008      | HU-050   | PR-030                 |
| UC-041      | OE-008      | HU-051   | PR-031                 |
| UC-050      | OE-009      | HU-060   | PR-040                 |
| UC-060      | OE-009      | HU-070   | PR-041                 |
| UC-070      | OE-009      | HU-080   | PR-042                 |
| UC-080      | OE-010      | HU-090   | PR-050                 |
| UC-081      | OE-010      | HU-091   | PR-051                 |
| UC-090      | OE-012      | HU-092   | PR-070, PR-071         |

## 4. Catálogo de Pruebas Planificadas

> Estos identificadores se detallarán como casos de prueba completos en `Testing.md` (Fase 4). Aquí se declaran para garantizar que cada elemento de la especificación tiene verificación prevista.

| ID Prueba | Verifica                                            | Tipo               |
|-----------|-----------------------------------------------------|--------------------|
| PR-001    | Registro exitoso con datos válidos                  | Funcional/Unitaria |
| PR-002    | Rechazo de correo duplicado / contraseña inválida   | Funcional          |
| PR-003    | Login y emisión de tokens                           | Integración        |
| PR-004    | Recuperación de contraseña                          | Funcional          |
| PR-005    | Edición de perfil y bloqueo de perfil ajeno         | Funcional          |
| PR-010    | Publicación de objeto válido                        | Integración        |
| PR-011    | Validación de imágenes (tamaño/MIME)                | Unitaria           |
| PR-012    | Edición/soft delete de objeto propio                | Funcional          |
| PR-013    | Búsqueda por texto y categoría                      | Integración        |
| PR-014    | Filtros geográficos y ordenamiento                  | Funcional          |
| PR-020    | Solicitud de intercambio y bloqueo de objeto propio | Funcional          |
| PR-021    | Aceptación/rechazo solo por ofertante               | Funcional          |
| PR-022    | No reapertura de solicitud rechazada                | Funcional          |
| PR-023    | Registro de historial de estados                    | Integración        |
| PR-024    | Finalización y habilitación de calificación         | Funcional          |
| PR-025    | Calificación única y cálculo de reputación          | Unitaria           |
| PR-030    | Reporte de objeto/usuario                           | Funcional          |
| PR-031    | Gestión de reportes por moderador                   | Funcional          |
| PR-040    | Marcar/listar favoritos                             | Funcional          |
| PR-041    | Mensajería entre partes                             | Integración        |
| PR-042    | Notificaciones por evento                           | Integración        |
| PR-050    | Administración de usuarios/categorías               | Funcional          |
| PR-051    | Dashboard e indicadores                             | Funcional          |
| PR-060    | Coherencia jerárquica de ubicación                  | Unitaria           |
| PR-061    | Filtro por cercanía                                 | Integración        |
| PR-070    | Auditoría de acciones CRUD                          | Integración        |
| PR-071    | Auditoría de login/intercambio                      | Integración        |
| PR-080    | Cobertura de pruebas >=90%                          | No funcional       |
| PR-081    | Mitigaciones OWASP Top 10                           | Seguridad          |
| PR-082    | Rendimiento de consultas críticas                   | No funcional       |
| PR-083    | Validación de arquitectura (Clean/CQRS/Feature)     | Revisión           |

## 5. Análisis de Completitud

| Verificación                             | Resultado                                           |
|------------------------------------------|-----------------------------------------------------|
| Objetivos sin requisitos                 | Ninguno (OE-001..OE-013 cubiertos)                  |
| Requisitos sin caso de uso o regla       | Ninguno                                             |
| Casos de uso sin objetivo de origen      | Ninguno                                             |
| Casos de uso sin prueba planificada      | Ninguno                                             |
| Reglas de negocio sin requisito asociado | Ninguno                                             |
| Historias sin criterios de aceptación    | Ninguna (núcleo); secundarias pendientes de detalle |

## 6. Módulos del Sistema (referencia)

> Vista preliminar para trazabilidad; el diseño detallado corresponde a la Fase 2.

| Módulo        | Ámbito                                         | Casos de uso principales               |
|---------------|------------------------------------------------|----------------------------------------|
| auth          | Registro, login, recuperación, sesión          | UC-001..UC-004                         |
| users         | Perfil y reputación del usuario                | UC-012, UC-022                         |
| objects       | Objetos, búsqueda, favoritos                   | UC-010, UC-011, UC-030, UC-031, UC-050 |
| exchanges     | Solicitudes, intercambios, calificaciones      | UC-020, UC-021, UC-022                 |
| notifications | Notificaciones y mensajería                    | UC-060, UC-070                         |
| admin         | Administración, reportes, auditoría, dashboard | UC-040, UC-041, UC-080, UC-081, UC-090 |

## 7. Control de Cambios y Aprobación

| Versión | Fecha      | Autor             | Descripción                         |
|---------|------------|-------------------|-------------------------------------|
| 0.1.0   | 2026-06-03 | Equipo Enterprise | Matriz inicial consolidando Fase 1. |

**Aprobación requerida (cierre de Fase 1 — Análisis):**

| Rol (RACI)                 | Responsabilidad                            | Estado    |
|----------------------------|--------------------------------------------|-----------|
| Arquitecto Empresarial (A) | Aprueba la trazabilidad y completitud      | Pendiente |
| Product Owner (R)          | Confirma cobertura de objetivos            | Pendiente |
| QA (A/R)                   | Valida el catálogo de pruebas planificadas | Pendiente |

> **Regla SDD:** Con la aprobación de esta matriz se cierra formalmente la **Fase 1 (Análisis)** y el gate habilita la **Fase 2 (Diseño) / PASO 5 SDD: `UML.md`**.
