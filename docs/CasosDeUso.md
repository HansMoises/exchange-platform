# CasosDeUso.md

> **Documento:** Casos de Uso
> **Paso SDD:** 4 de 8 (Casos de Uso) — **Fase SDLC:** 1 (Análisis)
> **Estado:** `BORRADOR — PENDIENTE DE APROBACIÓN`
> **Versión:** 0.1.0
> **Fecha:** 2026-06-03
> **Autor:** Equipo Enterprise (Analista / Arquitecto Senior)
> **Depende de:** `Requisitos.md`, `ReglasNegocio.md` (aprobados)
> **Trazabilidad:** Cada caso de uso referencia requisitos (RF) y reglas (RN). Se consolidará en `MatrizTrazabilidad.md` y se modelará en `UML.md`.

---

## 1. Introducción

Este documento especifica los casos de uso (UC) de la plataforma. Cada caso describe la interacción de un actor con el sistema para lograr un objetivo, incluyendo flujo principal, flujos alternativos y excepciones. Los flujos alternativos y las excepciones son la base de los criterios de aceptación y los casos de prueba (`Testing.md`).

Para el MVP se detallan a fondo los casos núcleo (registro, autenticación, publicación e intercambio completo). Los casos secundarios se listan con todos sus campos clave en la sección 4 y se detallarán en iteraciones posteriores según el Roadmap.

## 2. Actores del Sistema

| Actor         | Tipo       | Descripción                                                |
|---------------|------------|------------------------------------------------------------|
| Usuario       | Primario   | Ciudadano registrado que publica e intercambia objetos.    |
| Visitante     | Primario   | Persona no autenticada que puede registrarse y explorar.   |
| Moderador     | Primario   | Gestiona reportes y modera contenido.                      |
| Administrador | Primario   | Administra usuarios, categorías, intercambios y auditoría. |
| Sistema       | Secundario | Genera notificaciones, registra auditoría y aplica reglas. |

## 3. Casos de Uso Detallados (núcleo del MVP)

### UC-001 — Registrar usuario

| Campo               | Valor                                  |
|---------------------|----------------------------------------|
| Código              | UC-001                                 |
| Nombre              | Registrar usuario                      |
| Actor principal     | Visitante                              |
| Actores secundarios | Sistema                                |
| Prioridad           | Must                                   |
| Requisitos          | RF-001, RF-002, RF-100                 |
| Reglas asociadas    | RN-002, RN-003, RN-004, RN-050, RN-051 |

**Descripción:** Permite a un visitante crear una cuenta con identidad y ubicación.

**Precondiciones:**
- El visitante no posee una cuenta.
- Los datos maestros geográficos están disponibles.

**Postcondiciones:**
- La cuenta queda creada y el usuario puede autenticarse.
- Se registra la auditoría de creación.

**Flujo principal:**
1. El visitante accede al formulario de registro.
2. Ingresa nombres, apellidos, correo, teléfono, contraseña y ubicación (departamento/provincia/distrito).
3. El sistema valida formato, unicidad del correo y política de contraseña.
4. El sistema crea la cuenta y registra los campos de auditoría.
5. El sistema confirma el registro exitoso.

**Flujos alternativos:**
- 3a. Si la ubicación es jerárquicamente incoherente, el sistema solicita corrección (RN-051).

**Excepciones:**
- E1. Correo ya registrado → se informa y se solicita otro (RN-002).
- E2. Contraseña no cumple la política → se rechaza con mensaje (RN-004).

### UC-002 — Iniciar sesión

| Campo               | Valor                  |
|---------------------|------------------------|
| Código              | UC-002                 |
| Nombre              | Iniciar sesión         |
| Actor principal     | Usuario                |
| Actores secundarios | Sistema                |
| Prioridad           | Must                   |
| Requisitos          | RF-003, RF-004, RF-007 |
| Reglas asociadas    | RN-061, RN-063, RN-064 |

**Descripción:** Permite a un usuario autenticarse y obtener tokens de acceso.

**Precondiciones:**
- El usuario posee una cuenta activa.

**Postcondiciones:**
- El usuario queda autenticado con Access y Refresh Token.
- Se registra el evento de inicio de sesión.

**Flujo principal:**
1. El usuario ingresa correo y contraseña.
2. El sistema valida las credenciales.
3. El sistema emite Access Token y Refresh Token.
4. El sistema registra el inicio de sesión en auditoría.

**Flujos alternativos:**
- —

**Excepciones:**
- E1. Credenciales inválidas → se informa sin revelar cuál campo falló.
- E2. Múltiples intentos fallidos → se aplica rate limiting (RNF-004).

### UC-010 — Publicar objeto

| Campo               | Valor                                  |
|---------------------|----------------------------------------|
| Código              | UC-010                                 |
| Nombre              | Publicar objeto                        |
| Actor principal     | Usuario                                |
| Actores secundarios | Sistema                                |
| Prioridad           | Must                                   |
| Requisitos          | RF-020, RF-021, RF-022, RF-025         |
| Reglas asociadas    | RN-001, RN-010, RN-011, RN-012, RN-014 |

**Descripción:** Permite a un usuario publicar un objeto con imágenes y ubicación.

**Precondiciones:**
- El usuario está autenticado.
- Existen categorías válidas disponibles.

**Postcondiciones:**
- El objeto queda publicado y visible en búsquedas.
- Se registra la auditoría de creación.

**Flujo principal:**
1. El usuario abre el formulario de publicación.
2. Ingresa título, descripción, categoría, estado y ubicación.
3. Adjunta una o más imágenes.
4. El sistema valida la categoría, las imágenes (tamaño/extensión/MIME) y los campos requeridos.
5. El sistema crea el objeto en estado publicado y registra auditoría.
6. El sistema confirma la publicación.

**Flujos alternativos:**
- 4a. Si falta imagen válida, el sistema impide publicar (RN-011).

**Excepciones:**
- E1. Imagen inválida → se rechaza indicando el motivo (RN-012).
- E2. Categoría inexistente → se solicita una categoría válida (RN-010).

### UC-020 — Solicitar intercambio

| Campo               | Valor                  |
|---------------------|------------------------|
| Código              | UC-020                 |
| Nombre              | Solicitar intercambio  |
| Actor principal     | Usuario                |
| Actores secundarios | Sistema                |
| Prioridad           | Must                   |
| Requisitos          | RF-040, RF-090         |
| Reglas asociadas    | RN-001, RN-014, RN-022 |

**Descripción:** Permite a un usuario solicitar un intercambio sobre un objeto ajeno.

**Precondiciones:**
- El usuario está autenticado.
- El objeto está publicado y no pertenece al solicitante.

**Postcondiciones:**
- Se crea la solicitud en estado pendiente.
- Se notifica al ofertante.

**Flujo principal:**
1. El usuario visualiza el detalle de un objeto.
2. Selecciona solicitar intercambio.
3. El sistema valida que el objeto no sea propio y esté publicado.
4. El sistema crea la solicitud en estado pendiente.
5. El sistema notifica al usuario ofertante (SolicitudIntercambio).

**Flujos alternativos:**
- —

**Excepciones:**
- E1. El objeto es propio → se impide la solicitud (RN-022).
- E2. El objeto no está publicado → se impide la solicitud (RN-014).

### UC-021 — Aceptar o rechazar intercambio

| Campo               | Valor                          |
|---------------------|--------------------------------|
| Código              | UC-021                         |
| Nombre              | Aceptar o rechazar intercambio |
| Actor principal     | Usuario (ofertante)            |
| Actores secundarios | Sistema                        |
| Prioridad           | Must                           |
| Requisitos          | RF-041, RF-090                 |
| Reglas asociadas    | RN-020, RN-023, RN-024, RN-026 |

**Descripción:** Permite al ofertante responder a una solicitud de intercambio.

**Precondiciones:**
- Existe una solicitud pendiente sobre un objeto del ofertante.

**Postcondiciones:**
- La solicitud queda aceptada o rechazada.
- Se notifica al solicitante y se registra el cambio.

**Flujo principal:**
1. El ofertante revisa la solicitud pendiente.
2. Decide aceptar o rechazar.
3. El sistema valida que sea el ofertante quien responde.
4. El sistema actualiza el estado y registra el historial.
5. El sistema notifica al solicitante (IntercambioAceptado/Rechazado).

**Flujos alternativos:**
- 2a. Si acepta, el intercambio avanza a coordinación entre ambas partes (RN-020).

**Excepciones:**
- E1. Responde quien no es el ofertante → se deniega (RN-023).
- E2. Solicitud ya rechazada → no puede reabrirse (RN-024).

### UC-022 — Finalizar intercambio y calificar

| Campo               | Valor                                  |
|---------------------|----------------------------------------|
| Código              | UC-022                                 |
| Nombre              | Finalizar intercambio y calificar      |
| Actor principal     | Usuario                                |
| Actores secundarios | Sistema                                |
| Prioridad           | Must                                   |
| Requisitos          | RF-043, RF-044, RF-050, RF-051, RF-052 |
| Reglas asociadas    | RN-025, RN-026, RN-030, RN-031, RN-032 |

**Descripción:** Permite cerrar un intercambio aceptado y calificar a la contraparte.

**Precondiciones:**
- El intercambio está aceptado por ambas partes.

**Postcondiciones:**
- El intercambio queda finalizado.
- Cada parte puede registrar una calificación.
- Se actualiza la reputación.

**Flujo principal:**
1. Las partes coordinan y concretan el intercambio.
2. Un usuario marca el intercambio como finalizado.
3. El sistema habilita la calificación a ambas partes.
4. Cada parte registra calificación (y comentario opcional) una sola vez.
5. El sistema recalcula la reputación promedio.

**Flujos alternativos:**
- —

**Excepciones:**
- E1. Calificación fuera de rango → se rechaza (RN-031).
- E2. Intento de calificar dos veces → se impide (RN-030).
- E3. Intercambio no finalizado → no se habilita calificación (RN-025).

## 4. Casos de Uso Secundarios (resumen)

> Mismo nivel de formalidad que los detallados; su flujo completo se elaborará en iteraciones siguientes. Se incluyen aquí con actor, requisitos, reglas y prioridad para mantener la trazabilidad.

| Código | Nombre                      | Actor         | Requisitos             | Reglas         | Prioridad |
|--------|-----------------------------|---------------|------------------------|----------------|-----------|
| UC-003 | Recuperar contraseña        | Usuario       | RF-005                 | RN-061         | Must      |
| UC-004 | Cerrar sesión               | Usuario       | RF-006                 | RN-064         | Must      |
| UC-011 | Editar/eliminar objeto      | Usuario       | RF-023                 | RN-005, RN-013 | Must      |
| UC-012 | Gestionar perfil            | Usuario       | RF-010, RF-011         | RN-005         | Must      |
| UC-030 | Buscar y filtrar objetos    | Usuario       | RF-030, RF-031, RF-032 | RN-014         | Must      |
| UC-031 | Filtrar por cercanía        | Usuario       | RF-033                 | RN-050         | Should    |
| UC-040 | Reportar objeto/usuario     | Usuario       | RF-060                 | RN-040, RN-042 | Should    |
| UC-041 | Gestionar reportes          | Moderador     | RF-061                 | RN-041         | Should    |
| UC-050 | Marcar/listar favoritos     | Usuario       | RF-070, RF-071         | —              | Should    |
| UC-060 | Enviar/leer mensajes        | Usuario       | RF-080, RF-081         | —              | Could     |
| UC-070 | Consultar notificaciones    | Usuario       | RF-090, RF-091         | —              | Should    |
| UC-080 | Administrar plataforma      | Administrador | RF-110, RF-112         | RN-060         | Must      |
| UC-081 | Ver dashboard e indicadores | Administrador | RF-111                 | RN-060         | Should    |
| UC-090 | Consultar auditoría         | Administrador | RF-120, RF-121         | RN-062, RN-063 | Should    |

## 5. Cobertura de Requisitos por Caso de Uso

| Caso de uso     | Requisitos cubiertos                   |
|-----------------|----------------------------------------|
| UC-001          | RF-001, RF-002, RF-100                 |
| UC-002          | RF-003, RF-004, RF-007                 |
| UC-003          | RF-005                                 |
| UC-004          | RF-006                                 |
| UC-010          | RF-020, RF-021, RF-022, RF-025         |
| UC-011          | RF-023                                 |
| UC-012          | RF-010, RF-011                         |
| UC-020          | RF-040, RF-090                         |
| UC-021          | RF-041, RF-090                         |
| UC-022          | RF-043, RF-044, RF-050, RF-051, RF-052 |
| UC-030          | RF-030, RF-031, RF-032                 |
| UC-031          | RF-033                                 |
| UC-040 / UC-041 | RF-060, RF-061                         |
| UC-050          | RF-070, RF-071                         |
| UC-060          | RF-080, RF-081                         |
| UC-070          | RF-090, RF-091                         |
| UC-080 / UC-081 | RF-110, RF-111, RF-112                 |
| UC-090          | RF-120, RF-121                         |

## 6. Control de Cambios y Aprobación

| Versión | Fecha      | Autor             | Descripción                                    |
|---------|------------|-------------------|------------------------------------------------|
| 0.1.0   | 2026-06-03 | Equipo Enterprise | Versión inicial de casos de uso para revisión. |

**Aprobación requerida (gate de salida Fase 1 — Análisis):**

| Rol (RACI)                 | Responsabilidad                                | Estado    |
|----------------------------|------------------------------------------------|-----------|
| Arquitecto Empresarial (A) | Aprueba los casos de uso                       | Pendiente |
| Product Owner (R)          | Valida flujos y prioridades                    | Pendiente |
| QA (C)                     | Confirma que flujos/excepciones son testeables | Pendiente |

> **Regla SDD:** Con la aprobación de este documento se cierra el bloque de Análisis (Pasos 1–4). No se avanza a la Fase 2 (Diseño) / PASO 5 (`UML.md`) hasta su aprobación formal y la de `HistoriasUsuario.md` y `MatrizTrazabilidad.md`.

<!--
  Configuración de markdownlint para este documento.
  Se desactivan las reglas que chocan con el estilo de documentación del
  proyecto (compartido por todos los .md de docs/), no defectos de contenido:
    MD013 line-length          — tablas y flujos de caso de uso largos por diseño.
    MD032 blanks-around-lists  — listas compactas dentro de cada caso de uso.
-->
<!-- markdownlint-configure-file {
  "MD013": false,
  "MD032": false
} -->
