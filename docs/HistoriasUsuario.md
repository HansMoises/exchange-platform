# HistoriasUsuario.md

> **Documento:** Historias de Usuario
> **Fase SDLC:** 1 (Análisis) — complemento ágil de los Casos de Uso
> **Estado:** `BORRADOR — PENDIENTE DE APROBACIÓN`
> **Versión:** 0.1.0
> **Fecha:** 2026-06-03
> **Autor:** Equipo Enterprise (Product Owner / Analista Senior)
> **Depende de:** `Requisitos.md`, `ReglasNegocio.md`, `CasosDeUso.md` (aprobados)
> **Trazabilidad:** Cada historia referencia su caso de uso (UC), requisitos (RF) y reglas (RN). Se consolidará en `MatrizTrazabilidad.md`.

---

## 1. Introducción

Este documento expresa las necesidades del usuario como historias en formato **Como / Quiero / Para**, acompañadas de **criterios de aceptación** en estilo Dado-Cuando-Entonces. Las historias complementan a los casos de uso: el caso describe el flujo de interacción; la historia captura el valor y define cuándo está "terminada". Los criterios de aceptación son la fuente directa de las pruebas de aceptación (`Testing.md`).

Las historias se agrupan por **épica** (módulo funcional). Prioridad en **MoSCoW**.

## 2. Épicas

| Épica    | Descripción                               | Historias       |
|----------|-------------------------------------------|-----------------|
| E-AUTH   | Registro, acceso y recuperación           | HU-001 a HU-003 |
| E-PERFIL | Gestión de perfil                         | HU-010          |
| E-OBJ    | Publicación y gestión de objetos          | HU-020, HU-021  |
| E-BUSQ   | Búsqueda y filtros                        | HU-030          |
| E-INT    | Intercambios y reputación                 | HU-040 a HU-042 |
| E-SEC    | Reportes, favoritos, chat, notificaciones | HU-050 a HU-080 |
| E-ADMIN  | Administración, dashboard y auditoría     | HU-090 a HU-092 |

## 3. Historias Detalladas (núcleo del MVP)

### 3.1 Épica E-AUTH — Acceso

#### HU-001

**Como** un visitante **quiero** registrarme con mis datos y ubicación **para** tener una cuenta y poder intercambiar objetos.

| UC     | Requisitos     | Reglas                 | Prioridad |
|--------|----------------|------------------------|-----------|
| UC-001 | RF-001, RF-002 | RN-002, RN-003, RN-004 | Must      |

**Criterios de aceptación:**
- Dado un correo no registrado, cuando completo datos válidos, entonces la cuenta se crea.
- Dado un correo ya registrado, cuando intento registrarme, entonces se me informa y no se crea (RN-002).
- Dada una contraseña que no cumple la política, cuando la envío, entonces se rechaza (RN-004).
#### HU-002

**Como** un usuario **quiero** iniciar sesión con mi correo y contraseña **para** acceder a la plataforma de forma segura.

| UC     | Requisitos             | Reglas         | Prioridad |
|--------|------------------------|----------------|-----------|
| UC-002 | RF-003, RF-004, RF-007 | RN-061, RN-063 | Must      |

**Criterios de aceptación:**
- Dadas credenciales válidas, cuando inicio sesión, entonces recibo acceso (tokens).
- Dadas credenciales inválidas, cuando inicio sesión, entonces se deniega sin revelar el campo erróneo.
- Dados varios intentos fallidos, cuando reintento, entonces se aplica limitación (RNF-004).
#### HU-003

**Como** un usuario **quiero** recuperar mi contraseña **para** volver a acceder si la olvido.

| UC     | Requisitos | Reglas | Prioridad |
|--------|------------|--------|-----------|
| UC-003 | RF-005     | RN-061 | Must      |

**Criterios de aceptación:**
- Dado un correo válido, cuando solicito recuperación, entonces recibo un medio para restablecerla.
- Dado un enlace/código vencido, cuando lo uso, entonces se rechaza.

### 3.2 Épica E-PERFIL — Perfil

#### HU-010

**Como** un usuario **quiero** ver y editar mi perfil **para** mantener mis datos actualizados.

| UC     | Requisitos     | Reglas | Prioridad |
|--------|----------------|--------|-----------|
| UC-012 | RF-010, RF-011 | RN-005 | Must      |

**Criterios de aceptación:**
- Dado mi perfil, cuando edito datos válidos, entonces se guardan.
- Dado un perfil ajeno, cuando intento editarlo, entonces se deniega (RN-005).

### 3.3 Épica E-OBJ — Objetos

#### HU-020

**Como** un usuario **quiero** publicar un objeto con fotos y ubicación **para** ofrecerlo para intercambio.

| UC     | Requisitos                     | Reglas                 | Prioridad |
|--------|--------------------------------|------------------------|-----------|
| UC-010 | RF-020, RF-021, RF-022, RF-025 | RN-010, RN-011, RN-012 | Must      |

**Criterios de aceptación:**
- Dado un objeto con categoría e imagen válidas, cuando publico, entonces queda visible.
- Dado un objeto sin imagen válida, cuando publico, entonces se impide (RN-011).
- Dada una imagen que excede tamaño/tipo, cuando la subo, entonces se rechaza (RN-012).
#### HU-021

**Como** un usuario **quiero** editar o eliminar mis objetos **para** mantener mis publicaciones al día.

| UC     | Requisitos | Reglas         | Prioridad |
|--------|------------|----------------|-----------|
| UC-011 | RF-023     | RN-005, RN-013 | Must      |

**Criterios de aceptación:**
- Dado un objeto propio, cuando lo edito, entonces se actualiza.
- Dado un objeto propio, cuando lo elimino, entonces se marca como borrado (RN-013).
- Dado un objeto ajeno, cuando intento modificarlo, entonces se deniega (RN-005).

### 3.4 Épica E-BUSQ — Búsqueda

#### HU-030

**Como** un usuario **quiero** buscar y filtrar objetos por texto, categoría y ubicación **para** encontrar lo que necesito cerca de mí.

| UC     | Requisitos             | Reglas | Prioridad |
|--------|------------------------|--------|-----------|
| UC-030 | RF-030, RF-031, RF-032 | RN-014 | Must      |

**Criterios de aceptación:**
- Dado un término, cuando busco, entonces se listan objetos publicados que coinciden.
- Dados filtros geográficos, cuando los aplico, entonces los resultados se acotan.
- Dado un orden seleccionado, cuando lo aplico, entonces los resultados se reordenan.

### 3.5 Épica E-INT — Intercambios y Reputación

#### HU-040

**Como** un usuario **quiero** solicitar un intercambio sobre un objeto **para** iniciar un trato con su dueño.

| UC     | Requisitos     | Reglas | Prioridad |
|--------|----------------|--------|-----------|
| UC-020 | RF-040, RF-090 | RN-022 | Must      |

**Criterios de aceptación:**
- Dado un objeto ajeno publicado, cuando solicito, entonces se crea solicitud pendiente y se notifica al dueño.
- Dado mi propio objeto, cuando intento solicitar, entonces se impide (RN-022).
#### HU-041

**Como** un usuario ofertante **quiero** aceptar o rechazar solicitudes **para** decidir con quién intercambio.

| UC     | Requisitos     | Reglas                 | Prioridad |
|--------|----------------|------------------------|-----------|
| UC-021 | RF-041, RF-090 | RN-020, RN-023, RN-024 | Must      |

**Criterios de aceptación:**
- Dada una solicitud sobre mi objeto, cuando acepto/rechazo, entonces se actualiza y se notifica al solicitante.
- Dada una solicitud ajena, cuando intento responderla, entonces se deniega (RN-023).
- Dada una solicitud ya rechazada, cuando intento reabrirla, entonces se impide (RN-024).
#### HU-042

**Como** un usuario **quiero** finalizar el intercambio y calificar a la contraparte **para** construir confianza en la comunidad.

| UC     | Requisitos                     | Reglas                         | Prioridad |
|--------|--------------------------------|--------------------------------|-----------|
| UC-022 | RF-043, RF-050, RF-051, RF-052 | RN-025, RN-030, RN-031, RN-032 | Must      |

**Criterios de aceptación:**
- Dado un intercambio aceptado, cuando lo finalizo, entonces se habilita la calificación.
- Dada una calificación dentro de rango, cuando la envío una vez, entonces actualiza la reputación.
- Dado un intento de calificar dos veces, cuando lo hago, entonces se impide (RN-030).

## 4. Historias Secundarias (resumen)

> Mismas reglas de trazabilidad; sus criterios de aceptación se detallarán en la iteración correspondiente del Roadmap.

| ID     | Como…         | Quiero…                           | Para…                              | UC     | Prioridad |
|--------|---------------|-----------------------------------|------------------------------------|--------|-----------|
| HU-050 | usuario       | reportar objetos/usuarios         | mantener la plataforma segura      | UC-040 | Should    |
| HU-051 | moderador     | gestionar reportes                | moderar el contenido               | UC-041 | Should    |
| HU-060 | usuario       | marcar favoritos                  | guardar objetos de interés         | UC-050 | Should    |
| HU-070 | usuario       | chatear con la contraparte        | coordinar el intercambio           | UC-060 | Could     |
| HU-080 | usuario       | recibir notificaciones            | estar al tanto de mis intercambios | UC-070 | Should    |
| HU-090 | administrador | administrar usuarios y categorías | gobernar la plataforma             | UC-080 | Must      |
| HU-091 | administrador | ver dashboard e indicadores       | tomar decisiones con datos         | UC-081 | Should    |
| HU-092 | administrador | consultar la auditoría            | garantizar trazabilidad            | UC-090 | Should    |

## 5. Control de Cambios y Aprobación

| Versión | Fecha      | Autor             | Descripción                                            |
|---------|------------|-------------------|--------------------------------------------------------|
| 0.1.0   | 2026-06-03 | Equipo Enterprise | Versión inicial de historias de usuario para revisión. |

**Aprobación requerida (Fase 1 — Análisis):**

| Rol (RACI)                 | Responsabilidad                             | Estado    |
|----------------------------|---------------------------------------------|-----------|
| Product Owner (R)          | Aprueba historias y criterios de aceptación | Pendiente |
| Arquitecto Empresarial (A) | Valida alineación con objetivos             | Pendiente |
| QA (C)                     | Confirma que los criterios son verificables | Pendiente |

> **Regla SDD:** Pendiente consolidar la trazabilidad en `MatrizTrazabilidad.md` antes de cerrar la Fase 1 y abrir el Diseño (`UML.md`).
