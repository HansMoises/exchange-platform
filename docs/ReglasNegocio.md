# ReglasNegocio.md

> **Documento:** Reglas de Negocio
> **Paso SDD:** 3 de 8 (Reglas de Negocio) — **Fase SDLC:** 1 (Análisis)
> **Estado:** `BORRADOR — PENDIENTE DE APROBACIÓN`
> **Versión:** 0.1.0
> **Fecha:** 2026-06-03
> **Autor:** Equipo Enterprise (Especialista DDD / Analista Senior)
> **Depende de:** `VisionProyecto.md`, `Requisitos.md` (aprobados)
> **Trazabilidad:** Cada regla referencia los requisitos (RF/RNF) que la originan y la capa donde reside (Dominio o Application). Se consolidará en `MatrizTrazabilidad.md`.

---

## 1. Introducción

Este documento formaliza las reglas de negocio (RN) que el sistema debe cumplir de forma invariable. Una regla expresa una restricción o política del dominio, independiente de la tecnología. Conforme a DDD, toda regla reside en la capa de **Dominio** (invariantes de entidad/agregado) o **Application** (orquestación y políticas de flujo), y **nunca** en los controladores.

Estas reglas son la fuente directa de:
- Las **validaciones** (FluentValidation en backend, Zod en frontend).
- Los **constraints** de la base de datos (`BD.md`).
- Los **criterios de aceptación** de las pruebas (`Testing.md`).

## 2. Convenciones

- **RN-XXX:** Regla de Negocio.
- **Capa:** *Dominio* (invariante intrínseca) o *Application* (política de orquestación/autorización).
- Las reglas se agrupan por subdominio.

## 3. Reglas por Subdominio

### 3.1 Usuarios y Cuentas

| ID     | Regla de negocio                                                                           | Requisitos             | Capa        |
|--------|--------------------------------------------------------------------------------------------|------------------------|-------------|
| RN-001 | Para publicar un objeto o solicitar un intercambio el usuario debe estar autenticado.      | RF-001, RF-003, RF-020 | Application |
| RN-002 | El correo de un usuario debe ser único en todo el sistema.                                 | RF-002                 | Dominio     |
| RN-003 | Todo usuario debe registrar nombres, apellidos, correo, teléfono y ubicación (no anónimo). | RF-001, RF-100         | Dominio     |
| RN-004 | La contraseña debe cumplir la política mínima de seguridad antes de aceptarse.             | RF-001, RNF-001        | Application |
| RN-005 | Un usuario solo puede editar o eliminar su propio perfil y sus propios objetos.            | RF-011, RF-023         | Application |

### 3.2 Objetos

| ID     | Regla de negocio                                                                         | Requisitos     | Capa        |
|--------|------------------------------------------------------------------------------------------|----------------|-------------|
| RN-010 | Todo objeto debe pertenecer a una categoría válida y existente.                          | RF-025         | Dominio     |
| RN-011 | Un objeto debe tener al menos una imagen válida para publicarse.                         | RF-021, RF-022 | Application |
| RN-012 | Cada imagen debe cumplir las restricciones de tamaño, extensión y tipo MIME.             | RF-022         | Application |
| RN-013 | Un objeto eliminado se marca como borrado (soft delete) y no se elimina físicamente.     | RF-023, RF-121 | Dominio     |
| RN-014 | Solo objetos en estado publicado son visibles en búsquedas y pueden recibir solicitudes. | RF-024, RF-040 | Application |

### 3.3 Intercambios

| ID     | Regla de negocio                                                                       | Requisitos     | Capa        |
|--------|----------------------------------------------------------------------------------------|----------------|-------------|
| RN-020 | Un intercambio requiere la aceptación de ambas partes para concretarse.                | RF-042         | Dominio     |
| RN-021 | No se permiten intercambios anónimos: ambas identidades quedan registradas y visibles. | RF-045         | Dominio     |
| RN-022 | Un usuario no puede solicitar un intercambio sobre su propio objeto.                   | RF-040         | Application |
| RN-023 | Solo el usuario ofertante puede aceptar o rechazar una solicitud sobre su objeto.      | RF-041         | Application |
| RN-024 | Una solicitud rechazada no puede volver a aceptarse; debe generarse una nueva.         | RF-041         | Application |
| RN-025 | Solo un intercambio finalizado habilita la calificación entre las partes.              | RF-043, RF-050 | Application |
| RN-026 | Todo cambio de estado de un intercambio queda registrado en su historial.              | RF-044, RF-120 | Application |

### 3.4 Reputación y Calificaciones

| ID     | Regla de negocio                                                                     | Requisitos | Capa    |
|--------|--------------------------------------------------------------------------------------|------------|---------|
| RN-030 | Cada parte puede calificar a la contraparte una sola vez por intercambio finalizado. | RF-050     | Dominio |
| RN-031 | La calificación numérica debe estar dentro del rango permitido.                      | RF-050     | Dominio |
| RN-032 | La reputación de un usuario es el promedio de las calificaciones recibidas.          | RF-052     | Dominio |

### 3.5 Reportes

| ID     | Regla de negocio                                                                   | Requisitos      | Capa        |
|--------|------------------------------------------------------------------------------------|-----------------|-------------|
| RN-040 | Un usuario puede reportar un objeto o usuario indicando un motivo.                 | RF-060          | Application |
| RN-041 | Solo Moderador o Administrador pueden gestionar y resolver reportes.               | RF-061, RNF-002 | Application |
| RN-042 | Un usuario no puede reportar el mismo objeto/usuario de forma duplicada y abierta. | RF-060          | Application |

### 3.6 Geolocalización

| ID     | Regla de negocio                                                                                    | Requisitos     | Capa        |
|--------|-----------------------------------------------------------------------------------------------------|----------------|-------------|
| RN-050 | La ubicación se compone de departamento, provincia y distrito válidos y jerárquicamente coherentes. | RF-100, RF-101 | Dominio     |
| RN-051 | La provincia debe pertenecer al departamento y el distrito a la provincia seleccionados.            | RF-101         | Dominio     |
| RN-052 | Las nuevas regiones se incorporan por datos maestros, sin cambios de código.                        | RNF-021        | Application |

### 3.7 Seguridad y Auditoría

| ID     | Regla de negocio                                                                              | Requisitos               | Capa        |
|--------|-----------------------------------------------------------------------------------------------|--------------------------|-------------|
| RN-060 | Toda acción sensible valida el rol y los permisos del usuario antes de ejecutarse.            | RF-112, RNF-002, RNF-005 | Application |
| RN-061 | Las contraseñas se almacenan siempre con hash; nunca en texto plano ni en logs.               | RNF-001, RNF-032         | Application |
| RN-062 | Toda creación, actualización y eliminación registra los campos de auditoría correspondientes. | RF-121                   | Dominio     |
| RN-063 | Los eventos de autenticación e intercambio se registran en la auditoría.                      | RF-120                   | Application |
| RN-064 | Los Refresh Tokens se invalidan al cerrar sesión y tienen vigencia limitada.                  | RF-006, RF-004           | Application |

## 4. Reglas Críticas (resumen ejecutivo)

Las reglas que definen la identidad del producto y no admiten excepción:

- **RN-020 + RN-021:** un intercambio solo existe con aceptación mutua y con identidades registradas (no anónimo). Son el núcleo de la confianza del sistema.
- **RN-001:** ninguna acción de publicación o intercambio ocurre sin autenticación.
- **RN-060 + RN-061:** autorización por permisos y resguardo de credenciales son obligatorios en toda operación sensible.
- **RN-013 + RN-062:** trazabilidad total mediante soft delete y auditoría.

## 5. Cobertura de Requisitos

Verificación de que las reglas cubren los requisitos que necesitan política de negocio:

| Requisito       | Reglas asociadas       |
|-----------------|------------------------|
| RF-001          | RN-001, RN-003, RN-004 |
| RF-002          | RN-002                 |
| RF-020 / RF-024 | RN-001, RN-014         |
| RF-021 / RF-022 | RN-011, RN-012         |
| RF-023          | RN-005, RN-013         |
| RF-025          | RN-010                 |
| RF-040 / RF-041 | RN-022, RN-023, RN-024 |
| RF-042          | RN-020                 |
| RF-043          | RN-025                 |
| RF-044          | RN-026                 |
| RF-045          | RN-021                 |
| RF-050 / RF-052 | RN-030, RN-031, RN-032 |
| RF-060 / RF-061 | RN-040, RN-041, RN-042 |
| RF-100 / RF-101 | RN-050, RN-051         |
| RF-112          | RN-060                 |
| RF-120 / RF-121 | RN-062, RN-063         |
| RNF-001         | RN-061                 |
| RNF-021         | RN-052                 |

## 6. Control de Cambios y Aprobación

| Versión | Fecha      | Autor             | Descripción                                         |
|---------|------------|-------------------|-----------------------------------------------------|
| 0.1.0   | 2026-06-03 | Equipo Enterprise | Versión inicial de reglas de negocio para revisión. |

**Aprobación requerida (parte del gate de salida Fase 1):**

| Rol (RACI)                 | Responsabilidad                               | Estado    |
|----------------------------|-----------------------------------------------|-----------|
| Arquitecto Empresarial (A) | Aprueba las reglas y su ubicación por capa    | Pendiente |
| Product Owner (R)          | Valida que reflejan la política de negocio    | Pendiente |
| Especialista DDD (C)       | Verifica coherencia de invariantes de dominio | Pendiente |
| QA (C)                     | Confirma que cada regla es verificable        | Pendiente |

> **Regla SDD:** No se avanza al PASO 4 (`CasosDeUso.md`) hasta que este documento sea aprobado formalmente.
