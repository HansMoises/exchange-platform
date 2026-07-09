# Requisitos.md

> **Documento:** Especificación de Requisitos
> **Paso SDD:** 2 de 8 (Requisitos) — **Fase SDLC:** 1 (Análisis)
> **Estado:** `BORRADOR — PENDIENTE DE APROBACIÓN`
> **Versión:** 0.1.0
> **Fecha:** 2026-06-03
> **Autor:** Equipo Enterprise (Analista / Arquitecto Senior)
> **Depende de:** `VisionProyecto.md` (aprobado)
> **Trazabilidad:** Cada requisito referencia el objetivo específico (OE-XXX) que lo origina. Se consolidará en `MatrizTrazabilidad.md`.

---

## 1. Introducción

Este documento especifica los requisitos funcionales (RF) y no funcionales (RNF) de la Plataforma Inteligente de Intercambio de Objetos. Cada requisito es atómico, verificable y trazable a un objetivo específico de la Visión. Las prioridades usan **MoSCoW**: *Must* (imprescindible para el MVP), *Should* (importante), *Could* (deseable), *Won't* (fuera del alcance actual).

## 2. Convenciones de Identificadores

- **RF-XXX:** Requisito Funcional.
- **RNF-XXX:** Requisito No Funcional.
- **OE-XXX:** Objetivo Específico de origen (definido en `VisionProyecto.md`).

## 3. Requisitos Funcionales

### 3.1 Autenticación y Seguridad de Acceso

| ID     | Requisito                                                                                                 | OE     | Prioridad |
|--------|-----------------------------------------------------------------------------------------------------------|--------|-----------|
| RF-001 | El sistema permite registrar un usuario con nombres, apellidos, correo, teléfono, contraseña y ubicación. | OE-001 | Must      |
| RF-002 | El sistema valida que el correo sea único y con formato válido.                                           | OE-001 | Must      |
| RF-003 | El sistema permite iniciar sesión con correo y contraseña.                                                | OE-002 | Must      |
| RF-004 | El sistema emite un Access Token (JWT) y un Refresh Token al autenticar.                                  | OE-002 | Must      |
| RF-005 | El sistema permite recuperar la contraseña mediante un mecanismo de restablecimiento.                     | OE-002 | Must      |
| RF-006 | El sistema permite cerrar sesión e invalidar el Refresh Token.                                            | OE-002 | Must      |
| RF-007 | El sistema bloquea el acceso tras múltiples intentos fallidos (rate limiting / brute force).              | OE-002 | Should    |

### 3.2 Gestión de Perfil

| ID     | Requisito                                                                            | OE     | Prioridad |
|--------|--------------------------------------------------------------------------------------|--------|-----------|
| RF-010 | El sistema permite visualizar el perfil del usuario autenticado.                     | OE-002 | Must      |
| RF-011 | El sistema permite editar los datos del perfil (nombres, teléfono, ubicación, foto). | OE-002 | Must      |
| RF-012 | El sistema muestra la reputación y el historial público del usuario en su perfil.    | OE-007 | Should    |

### 3.3 Gestión de Objetos

| ID     | Requisito                                                                                     | OE     | Prioridad |
|--------|-----------------------------------------------------------------------------------------------|--------|-----------|
| RF-020 | El sistema permite publicar un objeto con título, descripción, categoría, estado y ubicación. | OE-003 | Must      |
| RF-021 | El sistema permite adjuntar múltiples imágenes a un objeto.                                   | OE-003 | Must      |
| RF-022 | El sistema valida tamaño, extensión y tipo MIME de cada imagen.                               | OE-003 | Must      |
| RF-023 | El sistema permite editar y eliminar (soft delete) un objeto propio.                          | OE-003 | Must      |
| RF-024 | El sistema permite listar objetos con paginación.                                             | OE-004 | Must      |
| RF-025 | El sistema asocia cada objeto a una categoría válida.                                         | OE-003 | Must      |

### 3.4 Búsqueda, Filtros y Ordenamiento

| ID     | Requisito                                                                                     | OE     | Prioridad |
|--------|-----------------------------------------------------------------------------------------------|--------|-----------|
| RF-030 | El sistema permite buscar objetos por texto (título/descripción).                             | OE-004 | Must      |
| RF-031 | El sistema permite filtrar por categoría, departamento, provincia, distrito, estado y fecha.  | OE-004 | Must      |
| RF-032 | El sistema permite ordenar resultados por fecha, nombre, popularidad o reputación (ASC/DESC). | OE-004 | Should    |
| RF-033 | El sistema permite filtrar objetos por cercanía geográfica.                                   | OE-011 | Should    |

### 3.5 Intercambios

| ID     | Requisito                                                                                  | OE     | Prioridad |
|--------|--------------------------------------------------------------------------------------------|--------|-----------|
| RF-040 | El sistema permite solicitar un intercambio sobre un objeto publicado.                     | OE-005 | Must      |
| RF-041 | El sistema permite al ofertante aceptar o rechazar una solicitud de intercambio.           | OE-005 | Must      |
| RF-042 | El sistema requiere la aceptación de ambas partes para concretar el intercambio.           | OE-005 | Must      |
| RF-043 | El sistema permite marcar un intercambio como finalizado.                                  | OE-005 | Must      |
| RF-044 | El sistema mantiene el historial de intercambios por usuario.                              | OE-006 | Must      |
| RF-045 | El sistema muestra en cada intercambio los datos e identidad de ambas partes (no anónimo). | OE-005 | Must      |

### 3.6 Reputación y Calificaciones

| ID     | Requisito                                                                                   | OE     | Prioridad |
|--------|---------------------------------------------------------------------------------------------|--------|-----------|
| RF-050 | El sistema permite calificar numéricamente a la contraparte tras un intercambio finalizado. | OE-007 | Must      |
| RF-051 | El sistema permite agregar un comentario a la calificación.                                 | OE-007 | Should    |
| RF-052 | El sistema calcula y muestra la reputación promedio del usuario.                            | OE-007 | Must      |

### 3.7 Reportes

| ID     | Requisito                                                                | OE     | Prioridad |
|--------|--------------------------------------------------------------------------|--------|-----------|
| RF-060 | El sistema permite reportar un objeto o un usuario.                      | OE-008 | Must      |
| RF-061 | El sistema permite a moderadores/administradores gestionar los reportes. | OE-008 | Must      |

### 3.8 Favoritos

| ID     | Requisito                                                     | OE     | Prioridad |
|--------|---------------------------------------------------------------|--------|-----------|
| RF-070 | El sistema permite marcar y desmarcar objetos como favoritos. | OE-009 | Should    |
| RF-071 | El sistema permite listar los favoritos del usuario.          | OE-009 | Should    |

### 3.9 Chat / Mensajería

| ID     | Requisito                                                                              | OE     | Prioridad |
|--------|----------------------------------------------------------------------------------------|--------|-----------|
| RF-080 | El sistema permite el envío de mensajes entre usuarios involucrados en un intercambio. | OE-009 | Could     |
| RF-081 | El sistema lista las conversaciones del usuario.                                       | OE-009 | Could     |

### 3.10 Notificaciones

| ID     | Requisito                                                                                                                         | OE     | Prioridad |
|--------|-----------------------------------------------------------------------------------------------------------------------------------|--------|-----------|
| RF-090 | El sistema genera notificaciones de tipo SolicitudIntercambio, IntercambioAceptado, IntercambioRechazado, NuevoMensaje y Sistema. | OE-009 | Should    |
| RF-091 | El sistema permite al usuario consultar y marcar como leídas sus notificaciones.                                                  | OE-009 | Should    |

### 3.11 Geolocalización

| ID     | Requisito                                                                                          | OE     | Prioridad |
|--------|----------------------------------------------------------------------------------------------------|--------|-----------|
| RF-100 | El sistema registra departamento, provincia, distrito, latitud y longitud para usuarios y objetos. | OE-011 | Must      |
| RF-101 | El sistema provee los datos maestros geográficos (departamentos, provincias, distritos).           | OE-011 | Must      |

### 3.12 Administración

| ID     | Requisito                                                                                                        | OE     | Prioridad |
|--------|------------------------------------------------------------------------------------------------------------------|--------|-----------|
| RF-110 | El sistema provee un panel administrativo para gestionar usuarios, objetos, intercambios, reportes y categorías. | OE-010 | Must      |
| RF-111 | El sistema provee un dashboard con indicadores (usuarios, objetos, intercambios, reportes).                      | OE-010 | Should    |
| RF-112 | El sistema gestiona roles y permisos (Administrador, Moderador, Usuario).                                        | OE-010 | Must      |

### 3.13 Auditoría

| ID     | Requisito                                                                                                      | OE     | Prioridad |
|--------|----------------------------------------------------------------------------------------------------------------|--------|-----------|
| RF-120 | El sistema registra auditoría de inicio/cierre de sesión, creación, actualización, eliminación e intercambios. | OE-012 | Must      |
| RF-121 | El sistema aplica campos de auditoría (CreatedAt/By, UpdatedAt/By) y soft delete (IsDeleted, DeletedAt/By).    | OE-012 | Must      |

## 4. Requisitos No Funcionales

### 4.1 Seguridad

| ID      | Requisito no funcional                                                              | OE     | Prioridad |
|---------|-------------------------------------------------------------------------------------|--------|-----------|
| RNF-001 | Las contraseñas se almacenan con hash seguro; nunca en texto plano.                 | OE-012 | Must      |
| RNF-002 | El acceso se controla por roles, permisos y políticas (autorización).               | OE-010 | Must      |
| RNF-003 | El sistema mitiga OWASP Top 10 (Injection, XSS, CSRF, Broken Access Control, etc.). | OE-012 | Must      |
| RNF-004 | El sistema aplica rate limiting y protección ante fuerza bruta y replay.            | OE-002 | Should    |
| RNF-005 | Las acciones sensibles validan permisos antes de ejecutarse.                        | OE-010 | Must      |

### 4.2 Rendimiento

| ID      | Requisito no funcional                                                      | OE     | Prioridad |
|---------|-----------------------------------------------------------------------------|--------|-----------|
| RNF-010 | El tiempo de respuesta promedio de la API es aceptable bajo carga normal.   | OE-013 | Should    |
| RNF-011 | Las consultas usan índices, paginación y consultas eficientes.              | OE-013 | Must      |
| RNF-012 | El frontend aplica lazy loading, code splitting y optimización de imágenes. | OE-013 | Should    |

### 4.3 Escalabilidad

| ID      | Requisito no funcional                                                                         | OE     | Prioridad |
|---------|------------------------------------------------------------------------------------------------|--------|-----------|
| RNF-020 | La arquitectura soporta escalamiento de Ayacucho a nivel nacional sin rediseños estructurales. | OE-013 | Must      |
| RNF-021 | El modelo geográfico jerárquico permite agregar nuevas regiones por datos, no por código.      | OE-011 | Must      |
| RNF-022 | El sistema está preparado para despliegue en nube (Azure/AWS/GCP).                             | OE-013 | Should    |

### 4.4 Disponibilidad y Respaldo

| ID      | Requisito no funcional                                                          | OE     | Prioridad |
|---------|---------------------------------------------------------------------------------|--------|-----------|
| RNF-030 | El sistema cuenta con respaldos diario, semanal y mensual.                      | OE-012 | Must      |
| RNF-031 | El sistema define procedimientos de recuperación ante desastres (DRP).          | OE-012 | Should    |
| RNF-032 | El sistema registra logs de errores, advertencias y eventos críticos (Serilog). | OE-012 | Must      |

### 4.5 Usabilidad y Accesibilidad

| ID      | Requisito no funcional                                                            | OE     | Prioridad |
|---------|-----------------------------------------------------------------------------------|--------|-----------|
| RNF-040 | La interfaz es responsive (Mobile First): móvil, tablet, escritorio.              | OE-004 | Must      |
| RNF-041 | La interfaz cumple accesibilidad WCAG (contraste, teclado, lectores de pantalla). | OE-004 | Should    |
| RNF-042 | La interfaz prioriza simplicidad, claridad y confianza.                           | OE-004 | Should    |

### 4.6 Mantenibilidad y Calidad

| ID      | Requisito no funcional                                                 | OE     | Prioridad |
|---------|------------------------------------------------------------------------|--------|-----------|
| RNF-050 | El backend cumple Clean Architecture, CQRS, Repository y Unit Of Work. | OE-013 | Must      |
| RNF-051 | El frontend cumple Feature Based Architecture.                         | OE-013 | Must      |
| RNF-052 | El código aplica SOLID, DRY, KISS, YAGNI y Clean Code.                 | OE-013 | Must      |
| RNF-053 | La cobertura de pruebas es mínima 90% (ideal 95%).                     | OE-013 | Must      |

## 5. Resumen de Cobertura de Objetivos

Cada objetivo específico de la Visión queda cubierto por al menos un requisito:

| OE     | Requisitos asociados                                                            |
|--------|---------------------------------------------------------------------------------|
| OE-001 | RF-001, RF-002                                                                  |
| OE-002 | RF-003, RF-004, RF-005, RF-006, RF-007, RF-010, RF-011, RNF-004                 |
| OE-003 | RF-020, RF-021, RF-022, RF-023, RF-025                                          |
| OE-004 | RF-024, RF-030, RF-031, RF-032, RNF-040, RNF-041, RNF-042                       |
| OE-005 | RF-040, RF-041, RF-042, RF-043, RF-045                                          |
| OE-006 | RF-044                                                                          |
| OE-007 | RF-012, RF-050, RF-051, RF-052                                                  |
| OE-008 | RF-060, RF-061                                                                  |
| OE-009 | RF-070, RF-071, RF-080, RF-081, RF-090, RF-091                                  |
| OE-010 | RF-110, RF-111, RF-112, RNF-002, RNF-005                                        |
| OE-011 | RF-033, RF-100, RF-101, RNF-021                                                 |
| OE-012 | RF-120, RF-121, RNF-001, RNF-003, RNF-030, RNF-031, RNF-032                     |
| OE-013 | RNF-010, RNF-011, RNF-012, RNF-020, RNF-022, RNF-050, RNF-051, RNF-052, RNF-053 |

## 6. Control de Cambios y Aprobación

| Versión | Fecha      | Autor             | Descripción                                  |
|---------|------------|-------------------|----------------------------------------------|
| 0.1.0   | 2026-06-03 | Equipo Enterprise | Versión inicial de requisitos para revisión. |

**Aprobación requerida (parte del gate de salida Fase 1):**

| Rol (RACI)                 | Responsabilidad                             | Estado    |
|----------------------------|---------------------------------------------|-----------|
| Arquitecto Empresarial (A) | Aprueba el alcance de requisitos            | Pendiente |
| Product Owner (R)          | Valida prioridades MoSCoW                   | Pendiente |
| QA (C)                     | Verifica que cada requisito sea verificable | Pendiente |

> **Regla SDD:** No se avanza al PASO 3 (`ReglasNegocio.md`) hasta que este documento sea aprobado formalmente.
