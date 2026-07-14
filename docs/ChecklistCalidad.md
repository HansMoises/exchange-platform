# ChecklistCalidad.md
# Plataforma Inteligente de Intercambio de Objetos

> **Documento:** Checklist y Gates de Calidad
> **Fase SDLC:** Transversal — instrumento de verificación
> **Versión:** 1.1.0
> **Estado:** `PENDIENTE DE APROBACIÓN`
> **Fecha:** 2026-07-13
> **Autor:** Equipo Enterprise Senior (QA / Arquitecto / DevOps)
> **Documentos padre:** Testing.md | Seguridad.md | Deployment.md | Convenciones.md | Arquitectura.md
> **Convenciones:** Documentación en español.

---

## Control de Versiones

| Versión | Fecha      | Autor                    | Cambios          |
|---------|------------|--------------------------|------------------|
| 1.0.0   | 2026-06-03 | Equipo Enterprise Senior | Versión inicial. |
| 1.1.0   | 2026-07-13 | Equipo Enterprise Senior | Se incorpora la verificación de **pruebas E2E** (Playwright) a la Definición de Terminado y al gate de la Fase 4. Se añade la comprobación de **aislamiento de base de datos** (ADR-012): ninguna prueba escribe en producción. |

---

## Tabla de Contenidos

1. Gates de Calidad por Fase (SDLC + SDD)
2. Checklist "Antes de Implementar"
3. Checklist "Antes de Desplegar"
4. Definición de Terminado (DoD)
5. Checklist de Revisión de Código
6. Checklist de Seguridad
7. Checklist de Base de Datos
8. Registro de Deuda Técnica
9. Aprobación

---

## 1. Gates de Calidad por Fase (SDLC + SDD)

Ninguna fase avanza sin que la anterior haya pasado su gate (aprobación documental).

| Fase SDLC      | Paso SDD                       | Gate de salida                                                           |
|----------------|--------------------------------|--------------------------------------------------------------------------|
| Análisis       | 1-4 (+Historias, Trazabilidad) | Requisitos completos, reglas y casos aprobados, trazabilidad cerrada.    |
| Diseño         | 5-8 (UML→API)                  | UML, Arquitectura, BD y API aprobados; checklist "Antes de Implementar". |
| Desarrollo     | —                              | Código según diseño, code review aprobado, build CI verde.               |
| Pruebas        | —                              | Cobertura ≥90%, **suite E2E 13/13 en verde**, 0 defectos críticos, DoD cumplido, OWASP verificado, **0 escrituras sobre la BD de producción**. |
| Despliegue     | —                              | Checklist "Antes de Desplegar", backup, aprobación manual.               |
| Mantenimiento  | —                              | Cada cambio documentado, probado, desplegado y trazable.                 |

> Regla maestra: ANALIZAR → DOCUMENTAR → VALIDAR → DISEÑAR → APROBAR → IMPLEMENTAR → PROBAR → DESPLEGAR → MANTENER. Nunca invertir el flujo.

---

## 2. Checklist "Antes de Implementar"

Verificar que el diseño está completo antes de escribir código de una funcionalidad.

```
□ Requisitos de la funcionalidad documentados y aprobados.
□ Casos de uso definidos (flujo principal, alternativos, excepciones).
□ Reglas de negocio asociadas identificadas (RN-XXX).
□ Modelado UML pertinente disponible.
□ Arquitectura/capas claras para la funcionalidad.
□ Modelo de base de datos cubierto (tablas, constraints).
□ Contrato de API definido (endpoint, request, response, errores).
□ Aspectos de seguridad considerados (permisos, validaciones).
□ UI/UX disponible (si aplica a la funcionalidad).
□ Criterios de aceptación y pruebas planificadas (PR-XXX).
```

---

## 3. Checklist "Antes de Desplegar"

```
□ Build exitoso en CI.
□ Todas las pruebas pasan (unitarias, integración).
□ Cobertura mínima alcanzada (>= 90%).
□ Vulnerabilidades revisadas (sin críticas — OWASP).
□ Variables de entorno del ambiente configuradas.
□ Backups configurados y backup pre-despliegue tomado.
□ Migraciones de BD revisadas y reversibles.
□ Logs y monitoreo configurados.
□ Documentación actualizada (incluye Swagger / API.md).
□ Smoke tests definidos.
□ Aprobación manual obtenida (Producción).
```

---

## 4. Definición de Terminado (DoD)

Una funcionalidad/historia está "Terminada" cuando:

```
□ Código implementado según el diseño aprobado.
□ Cumple convenciones de código (Convenciones.md).
□ Code review aprobado (técnico, funcional, seguridad, arquitectura).
□ Pruebas unitarias y de integración escritas y pasando.
□ Si toca un flujo crítico: prueba E2E (Playwright) escrita y pasando.
□ Suite E2E completa en verde (13/13 — Testing.md §6.3).
□ Cobertura del ámbito cumplida.
□ Criterios de aceptación de la historia verificados.
□ Sin defectos críticos ni altos abiertos.
□ Documentación actualizada (Backend.md / Frontend.md / API.md / Testing.md).
□ Build verde en CI (incluida la etapa E2E — CICD.md §3).
□ Trazabilidad mantenida (requisito ↔ código ↔ prueba).

VERIFICACIÓN DE AISLAMIENTO DE DATOS (ADR-012)
□ Ninguna prueba se ha conectado a Supabase (producción).
□ La BD de test (:5433) es efímera — sin volumen persistente.
□ `ConnectionStrings__DefaultConnection` fijada explícitamente antes de migrar.
□ Confirmación visual del puerto activo antes de `dotnet ef database update`.
```

---

## 5. Checklist de Revisión de Código

Para cada Pull Request:

```
ARQUITECTURA
□ Respeta Clean Architecture (dependencias hacia el dominio).
□ Sin lógica de negocio en controllers.
□ CQRS correcto (command/query en su lugar).

CÓDIGO
□ Cumple convenciones de nomenclatura y estilo.
□ Aplica SOLID, DRY, KISS, YAGNI.
□ Sin código muerto, comentado o duplicado.
□ Sin magic numbers/strings (constantes/enums).
□ Manejo de errores adecuado.

SEGURIDAD
□ Validaciones en backend (no solo frontend).
□ Permisos verificados en acciones sensibles.
□ Sin secretos ni datos sensibles en código/logs.

PRUEBAS
□ Pruebas nuevas para el cambio.
□ Cobertura del ámbito cumplida.
□ Casos de error y borde cubiertos.

DOCUMENTACIÓN
□ Documentación y/o Swagger actualizados.
□ Commit messages siguen Conventional Commits.
```

---

## 6. Checklist de Seguridad

```
□ Contraseñas con hash (BCrypt); nunca en texto plano ni logs.
□ JWT con expiración correcta; refresh token con rotación.
□ Roles y permisos validados por endpoint (RBAC).
□ Protección contra Injection (EF parametrizado), XSS, CSRF.
□ Rate limiting y bloqueo de cuenta activos.
□ Headers de seguridad HTTP configurados.
□ Secretos en variables de entorno, no en el repo.
□ Auditoría de acciones sensibles registrada (AuditLog).
□ OWASP Top 10 revisado (PR-081).
```

---

## 7. Checklist de Base de Datos

```
□ Tablas normalizadas (3FN) salvo desnormalización justificada.
□ Claves primarias y foráneas definidas.
□ Constraints (UNIQUE, CHECK) que implementan reglas de negocio.
□ Índices en columnas de búsqueda/filtro/JOIN.
□ Soft delete + auditoría en entidades transaccionales.
□ Migración reversible (Down()).
□ Seed de datos maestros (UBIGEO, roles, categorías) disponible.
□ Backup configurado.
```

---

## 8. Registro de Deuda Técnica

Toda deuda técnica detectada se registra con este formato:

| Campo              | Descripción                               |
|--------------------|-------------------------------------------|
| ID                 | DT-XXX                                    |
| Descripción        | Qué se hizo de forma subóptima y por qué. |
| Impacto            | Riesgo/consecuencia si no se corrige.     |
| Prioridad          | Alta / Media / Baja.                      |
| Fecha de registro  | Cuándo se detectó.                        |
| Responsable        | Quién la gestiona.                        |
| Plan de corrección | Cómo y cuándo se resolverá.               |

Ejemplo:

| ID     | Descripción                          | Impacto | Prioridad | Plan           |
|--------|--------------------------------------|---------|-----------|----------------|
| DT-001 | (ejemplo) Validación duplicada en X  | Medio   | Media     | Refactor en V2 |

> La deuda técnica se revisa periódicamente (mantenimiento). No se ignora ni se acumula sin control.

---

## 9. Aprobación

| Rol                    | Nombre            | Aprobación  | Fecha |
|------------------------|-------------------|-------------|-------|
| QA Senior              | Equipo Enterprise | ☐ PENDIENTE | —     |
| Arquitecto de Software | Equipo Enterprise | ☐ PENDIENTE | —     |
| Especialista DevOps    | Equipo Enterprise | ☐ PENDIENTE | —     |

---

> **GATE DE CALIDAD:**
> Este documento es el instrumento de auto-verificación del equipo. Los checklists
> son obligatorios en sus respectivos momentos. La calidad tiene prioridad sobre la velocidad.

---

*Documento generado bajo la metodología SDD — Plataforma Inteligente de Intercambio de Objetos — Ayacucho, Perú.*
