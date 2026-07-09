# API.md
# Plataforma Inteligente de Intercambio de Objetos

> **Documento:** Especificación de la API REST
> **Paso SDD:** 8 de 8 (API) — **Fase SDLC:** 2 (Diseño) — ÚLTIMO documento antes de implementar
> **Versión:** 1.0.0
> **Estado:** `PENDIENTE DE APROBACIÓN`
> **Fecha:** 2026-06-03
> **Autor:** Equipo Enterprise Senior (Arquitecto de Software / Backend / Seguridad)
> **Documentos padre:** VisionProyecto.md | Requisitos.md | ReglasNegocio.md | CasosDeUso.md | HistoriasUsuario.md | MatrizTrazabilidad.md | UML.md | Arquitectura.md | BD.md
> **Convenciones:** Documentación en español; rutas REST en inglés (sustantivos plural). Casos de uso (UC-XXX) y reglas (RN-XXX) según Fase 1 aprobada.

---

## Control de Versiones

| Versión | Fecha      | Autor                    | Cambios          |
|---------|------------|--------------------------|------------------|
| 1.0.0   | 2026-06-03 | Equipo Enterprise Senior | Versión inicial. |

---

## Tabla de Contenidos

1. Convenciones Generales
2. Contratos de Respuesta
3. Autenticación y Autorización
4. Módulo AUTH — Autenticación
5. Módulo USERS — Usuarios
6. Módulo OBJECTS — Objetos
7. Módulo EXCHANGES — Intercambios
8. Módulo RATINGS — Calificaciones
9. Módulo MESSAGES — Mensajería
10. Módulo NOTIFICATIONS — Notificaciones
11. Módulo FAVORITES — Favoritos
12. Módulo REPORTS — Reportes
13. Módulo GEO — Datos Geográficos
14. Módulo ADMIN — Administración
15. Códigos de Error Estándar
16. Matriz de Endpoints
17. Aprobación

---

## 1. Convenciones Generales

### 1.1 Base URL

| Ambiente    | Base URL                           |
|-------------|------------------------------------|
| Development | http://localhost:5000/api/v1       |
| Staging     | https://staging.exchange.pe/api/v1 |
| Production  | https://exchange.pe/api/v1         |

### 1.2 Estándares REST

| Aspecto       | Convención                                                              |
|---------------|-------------------------------------------------------------------------|
| Protocolo     | HTTPS obligatorio en Staging y Producción                               |
| Formato       | JSON (Content-Type: application/json)                                   |
| Encoding      | UTF-8                                                                   |
| Verbos HTTP   | GET (leer), POST (crear), PUT (reemplazar), PATCH (parcial), DELETE     |
| Recursos      | Sustantivos en plural, inglés, minúsculas: /objects, /users, /exchanges |
| Versionado    | En la URL: /api/v1/... Preparado para /api/v2 sin romper v1             |
| Autenticación | Bearer Token: Authorization: Bearer {accessToken}                       |
| Fechas        | ISO 8601 UTC: 2026-06-03T00:00:00Z                                      |
| IDs           | UUID: 550e8400-e29b-41d4-a716-446655440000                              |

### 1.3 Paginación

| Parámetro  | Tipo | Default | Mínimo | Máximo | Descripción          |
|------------|------|---------|--------|--------|----------------------|
| pageNumber | int  | 1       | 1      | —      | Número de página     |
| pageSize   | int  | 20      | 1      | 50     | Registros por página |

### 1.4 Ordenamiento

| Parámetro | Tipo   | Default   | Valores aceptados                       |
|-----------|--------|-----------|-----------------------------------------|
| sortBy    | string | createdAt | createdAt, titulo, calificacionPromedio |
| sortOrder | string | desc      | asc, desc                               |

### 1.5 Filtros Comunes

| Parámetro      | Tipo   | Descripción                            |
|----------------|--------|----------------------------------------|
| search         | string | Búsqueda en título y descripción       |
| categoriaId    | int    | Filtro por categoría                   |
| departamentoId | int    | Filtro por departamento                |
| provinciaId    | int    | Filtro por provincia                   |
| distritoId     | int    | Filtro por distrito                    |
| estadoObjeto   | string | Disponible / Reservado / Intercambiado |

---

## 2. Contratos de Respuesta

### 2.1 Respuesta Exitosa Simple

```json
{
  "success"   : true,
  "message"   : "Operación exitosa.",
  "data"      : { },
  "errors"    : null,
  "timestamp" : "2026-06-03T00:00:00Z"
}
```

### 2.2 Respuesta Paginada

```json
{
  "success"   : true,
  "message"   : "OK",
  "data"      : {
    "items"       : [ ],
    "pageNumber"  : 1,
    "pageSize"    : 20,
    "totalRecords": 150,
    "totalPages"  : 8,
    "hasPrevious" : false,
    "hasNext"     : true
  },
  "errors"    : null,
  "timestamp" : "2026-06-03T00:00:00Z"
}
```

### 2.3 Respuesta de Error

```json
{
  "success"   : false,
  "message"   : "Error de validación.",
  "data"      : null,
  "errors"    : [
    "El título debe tener entre 5 y 100 caracteres.",
    "La categoría no es válida."
  ],
  "timestamp" : "2026-06-03T00:00:00Z"
}
```

---

## 3. Autenticación y Autorización

### 3.1 Headers Requeridos

| Header           | Valor                        | Requerido en         |
|------------------|------------------------------|----------------------|
| Authorization    | Bearer {accessToken}         | Endpoints protegidos |
| Content-Type     | application/json             | POST, PUT, PATCH     |
| X-Correlation-Id | UUID generado por el cliente | Todos (opcional)     |

### 3.2 Roles y Acceso

| Rol           | Descripción                                 |
|---------------|---------------------------------------------|
| Administrador | Acceso total. Incluye todos los permisos.   |
| Moderador     | Gestión de reportes, suspensión, dashboard. |
| Usuario       | Funciones estándar de la plataforma.        |
| Anónimo       | Solo endpoints públicos (búsqueda, detalle).|

### 3.3 Leyenda de Acceso en Endpoints

| Símbolo | Significado                    |
|---------|--------------------------------|
| PUB     | Público — sin autenticación    |
| AUTH    | Requiere autenticación (JWT)   |
| MOD     | Solo Moderador o Administrador |
| ADMIN   | Solo Administrador             |

---

## 4. Módulo AUTH — Autenticación

**Base:** `/api/v1/auth`

### POST /api/v1/auth/register
**Acceso:** PUB | **Caso de Uso:** UC-001 | **Reglas:** RN-002, RN-003, RN-004

**Request Body:**
```json
{
  "nombres"        : "Juan Carlos",
  "apellidos"      : "Quispe Flores",
  "email"          : "juan@example.com",
  "password"       : "MiPass@123",
  "confirmPassword": "MiPass@123",
  "telefono"       : "987654321",
  "departamentoId" : 5,
  "provinciaId"    : 501,
  "distritoId"     : 50101
}
```

**Validaciones:**

| Campo           | Regla                                                                        |
|-----------------|------------------------------------------------------------------------------|
| nombres         | Requerido. Mín. 2, máx. 100 chars. Solo letras y espacios.                   |
| apellidos       | Requerido. Mín. 2, máx. 100 chars. Solo letras y espacios.                   |
| email           | Requerido. Formato email válido. Máx. 256 chars. Único.                      |
| password        | Requerido. Mín. 8 chars. Mayúscula, minúscula, número y carácter especial.   |
| confirmPassword | Requerido. Debe ser igual a password.                                        |
| telefono        | Requerido. 9 dígitos numéricos (Perú).                                       |
| departamentoId  | Requerido. ID válido en tabla Departamentos.                                 |
| provinciaId     | Requerido. ID válido y perteneciente al departamentoId.                      |
| distritoId      | Requerido. ID válido y perteneciente al provinciaId.                         |

**Response 201 Created:**
```json
{
  "success" : true,
  "message" : "Cuenta creada exitosamente.",
  "data"    : {
    "id"       : "550e8400-e29b-41d4-a716-446655440000",
    "nombres"  : "Juan Carlos",
    "apellidos": "Quispe Flores",
    "email"    : "juan@example.com"
  },
  "errors"    : null,
  "timestamp" : "2026-06-03T00:00:00Z"
}
```

**Errores:**

| HTTP | Condición                      | Mensaje                         |
|------|--------------------------------|---------------------------------|
| 409  | Correo ya registrado           | "El correo ya está registrado." |
| 422  | Campos inválidos o incompletos | Lista de errores por campo.     |

### POST /api/v1/auth/login
**Acceso:** PUB | **Caso de Uso:** UC-002 | **Reglas:** RN-061, RN-063, RN-064

**Request Body:**
```json
{
  "email"   : "juan@example.com",
  "password": "MiPass@123"
}
```

**Response 200 OK:**
```json
{
  "success" : true,
  "message" : "Inicio de sesión exitoso.",
  "data"    : {
    "accessToken"         : "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken"        : "d9f8a3b2-1c4e-5f6a-7b8c-9d0e1f2a3b4c",
    "accessTokenExpiresAt": "2026-06-03T00:15:00Z",
    "user"                : {
      "id"                  : "550e8400-e29b-41d4-a716-446655440000",
      "nombres"             : "Juan Carlos",
      "apellidos"           : "Quispe Flores",
      "email"               : "juan@example.com",
      "rol"                 : "Usuario",
      "fotoPerfil"          : null,
      "calificacionPromedio": 0.0
    }
  },
  "errors"    : null,
  "timestamp" : "2026-06-03T00:00:00Z"
}
```

**Errores:**

| HTTP | Condición                | Mensaje                                           |
|------|--------------------------|---------------------------------------------------|
| 401  | Credenciales incorrectas | "Credenciales inválidas."                         |
| 423  | Cuenta bloqueada         | "Cuenta bloqueada. Intenta nuevamente en 15 min." |
| 429  | Rate limit superado      | Header Retry-After con tiempo de espera.          |

### POST /api/v1/auth/refresh
**Acceso:** PUB (usa Refresh Token) | **Reglas:** RN-064

**Request Body:**
```json
{ "refreshToken": "d9f8a3b2-1c4e-5f6a-7b8c-9d0e1f2a3b4c" }
```

**Response 200 OK:**
```json
{
  "success" : true,
  "message" : "Token renovado exitosamente.",
  "data"    : {
    "accessToken"         : "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken"        : "nuevo-refresh-token-rotado",
    "accessTokenExpiresAt": "2026-06-03T00:30:00Z"
  },
  "errors"    : null,
  "timestamp" : "2026-06-03T00:00:00Z"
}
```

**Errores:**

| HTTP | Condición                           | Mensaje                         |
|------|-------------------------------------|---------------------------------|
| 401  | Token inválido, expirado o ya usado | "Token de renovación inválido." |

### POST /api/v1/auth/logout
**Acceso:** AUTH | **Caso de Uso:** UC-004 | **Reglas:** RN-064

**Request Body:**
```json
{ "refreshToken": "d9f8a3b2-1c4e-5f6a-7b8c-9d0e1f2a3b4c" }
```

**Response 200 OK:**
```json
{
  "success"   : true,
  "message"   : "Sesión cerrada exitosamente.",
  "data"      : null,
  "errors"    : null,
  "timestamp" : "2026-06-03T00:00:00Z"
}
```

### POST /api/v1/auth/forgot-password
**Acceso:** PUB | **Caso de Uso:** UC-003 | **Reglas:** RN-061

**Request Body:**
```json
{ "email": "juan@example.com" }
```

**Response 200 OK:** (siempre, independiente de si el correo existe, para no revelar usuarios)
```json
{
  "success"   : true,
  "message"   : "Si el correo está registrado, recibirás instrucciones en breve.",
  "data"      : null,
  "errors"    : null,
  "timestamp" : "2026-06-03T00:00:00Z"
}
```

### POST /api/v1/auth/reset-password
**Acceso:** PUB | **Caso de Uso:** UC-003 | **Reglas:** RN-004, RN-061

**Request Body:**
```json
{
  "token"          : "uuid-token-de-recuperacion",
  "password"       : "NuevoPass@456",
  "confirmPassword": "NuevoPass@456"
}
```

**Response 200 OK:**
```json
{
  "success"   : true,
  "message"   : "Contraseña actualizada exitosamente.",
  "data"      : null,
  "errors"    : null,
  "timestamp" : "2026-06-03T00:00:00Z"
}
```

**Errores:**

| HTTP | Condición                   | Mensaje                                      |
|------|-----------------------------|----------------------------------------------|
| 400  | Token expirado              | "El enlace ha expirado. Solicita uno nuevo." |
| 400  | Token ya utilizado          | "El enlace ya fue utilizado."                |
| 422  | Contraseña no cumple reglas | Lista de errores.                            |

---

## 5. Módulo USERS — Usuarios

**Base:** `/api/v1/users`

### GET /api/v1/users/{id}
**Acceso:** PUB | **Caso de Uso:** UC-012

**Path Parameters:** `id` (UUID) — ID del usuario.

**Response 200 OK:**
```json
{
  "success" : true,
  "message" : "OK",
  "data"    : {
    "id"                  : "550e8400-e29b-41d4-a716-446655440000",
    "nombres"             : "Juan Carlos",
    "apellidos"           : "Quispe Flores",
    "fotoPerfil"          : "https://storage.exchange.pe/fotos/uuid.jpg",
    "distrito"            : "Ayacucho",
    "calificacionPromedio": 4.5,
    "totalIntercambios"   : 12,
    "miembroDesde"        : "2026-01-15T00:00:00Z"
  },
  "errors"    : null,
  "timestamp" : "2026-06-03T00:00:00Z"
}
```

### GET /api/v1/users/me
**Acceso:** AUTH | **Caso de Uso:** UC-012

Devuelve el perfil completo del usuario autenticado (incluye email, teléfono y ubicación).

### PUT /api/v1/users/me
**Acceso:** AUTH | **Caso de Uso:** UC-012 | **Reglas:** RN-005, RN-050, RN-051

**Request Body:**
```json
{
  "nombres"       : "Juan Carlos",
  "apellidos"     : "Quispe Flores",
  "telefono"      : "987654321",
  "departamentoId": 5,
  "provinciaId"   : 501,
  "distritoId"    : 50101
}
```

**Response 200 OK:** perfil actualizado.

### PATCH /api/v1/users/me/photo
**Acceso:** AUTH | **Caso de Uso:** UC-012

**Request:** multipart/form-data con campo `file` (imagen). Validar tamaño y tipo MIME.

**Response 200 OK:** `{ "fotoPerfil": "https://storage.exchange.pe/fotos/uuid.jpg" }`

### PATCH /api/v1/users/me/password
**Acceso:** AUTH | **Caso de Uso:** UC-012 | **Reglas:** RN-004, RN-061

**Request Body:**
```json
{
  "passwordActual" : "MiPass@123",
  "passwordNuevo"  : "NuevoPass@456",
  "confirmPassword": "NuevoPass@456"
}
```

**Errores:** 401 si `passwordActual` es incorrecta; 422 si el nuevo no cumple política.

---

## 6. Módulo OBJECTS — Objetos

**Base:** `/api/v1/objects`

### GET /api/v1/objects
**Acceso:** PUB | **Caso de Uso:** UC-030 | **Reglas:** RN-014

**Query Parameters:** search, categoriaId, departamentoId, provinciaId, distritoId, estadoObjeto, sortBy, sortOrder, pageNumber, pageSize.

**Response 200 OK (Paginado):**
```json
{
  "success" : true,
  "message" : "OK",
  "data"    : {
    "items": [
      {
        "id"          : "uuid-objeto",
        "titulo"      : "Bicicleta de montaña rodado 26",
        "descripcion" : "Bicicleta en buen estado, poco uso.",
        "categoria"   : "Deportes",
        "estadoObjeto": "Disponible",
        "condicionFisica": "Bueno",
        "imagenPortada": "https://storage.exchange.pe/objetos/uuid-1.jpg",
        "distrito"    : "Ayacucho",
        "propietario" : { "id": "uuid", "nombres": "Juan Carlos", "calificacionPromedio": 4.5 },
        "createdAt"   : "2026-05-20T00:00:00Z"
      }
    ],
    "pageNumber": 1, "pageSize": 20, "totalRecords": 87,
    "totalPages": 5, "hasPrevious": false, "hasNext": true
  },
  "errors"    : null,
  "timestamp" : "2026-06-03T00:00:00Z"
}
```

### GET /api/v1/objects/{id}
**Acceso:** PUB | **Caso de Uso:** UC-030 | **Reglas:** RN-014

**Response 200 OK:** objeto completo con todas sus imágenes (array ordenado), datos del propietario y ubicación. **404** si no existe o está eliminado.

### POST /api/v1/objects
**Acceso:** AUTH | **Caso de Uso:** UC-010 | **Reglas:** RN-001, RN-010, RN-011, RN-012

**Request Body:**
```json
{
  "titulo"         : "Bicicleta de montaña rodado 26",
  "descripcion"    : "Bicicleta en buen estado, poco uso, frenos nuevos.",
  "categoriaId"    : 6,
  "condicionFisica": "Bueno",
  "departamentoId" : 5,
  "provinciaId"    : 501,
  "distritoId"     : 50101,
  "imagenes"       : ["url-img-1", "url-img-2"]
}
```

**Validaciones:**

| Campo           | Regla                                                       |
|-----------------|-------------------------------------------------------------|
| titulo          | Requerido. Mín. 5, máx. 100 chars.                          |
| descripcion     | Requerido. Mín. 20, máx. 1000 chars.                        |
| categoriaId     | Requerido. Categoría existente y activa (RN-010).           |
| condicionFisica | Requerido. Nuevo / Bueno / Regular.                         |
| imagenes        | Mín. 1, máx. 5. Cada una válida en tamaño/MIME (RN-011/012).|
| ubicación       | departamentoId/provinciaId/distritoId coherentes (RN-050).  |

**Response 201 Created:** objeto creado en estado `Disponible`.

**Errores:** 401 sin token; 422 validación; 409 categoría inactiva.

### PUT /api/v1/objects/{id}
**Acceso:** AUTH | **Caso de Uso:** UC-011 | **Reglas:** RN-005

Actualiza un objeto propio. **403** si el objeto no pertenece al usuario autenticado.

### DELETE /api/v1/objects/{id}
**Acceso:** AUTH | **Caso de Uso:** UC-011 | **Reglas:** RN-005, RN-013

Eliminación lógica (soft delete). **403** si no es propietario. **409** si el objeto tiene un intercambio activo.

### GET /api/v1/objects/me
**Acceso:** AUTH

Lista paginada de los objetos del usuario autenticado (cualquier estado salvo eliminados).

### GET /api/v1/objects/me/available
**Acceso:** AUTH | **Caso de Uso:** UC-020

Lista los objetos `Disponible` del usuario, usados para **ofrecer** en un intercambio.

---

## 7. Módulo EXCHANGES — Intercambios

**Base:** `/api/v1/exchanges`

> Modelo de trueque objeto-por-objeto con doble confirmación (BD.md, tabla Intercambios).

### POST /api/v1/exchanges
**Acceso:** AUTH | **Caso de Uso:** UC-020 | **Reglas:** RN-001, RN-014, RN-022

**Request Body:**
```json
{
  "objetoSolicitadoId": "uuid-objeto-ajeno",
  "objetoOfrecidoId"  : "uuid-objeto-propio",
  "mensajeInicial"    : "Hola, me interesa tu bicicleta. Te ofrezco mi guitarra."
}
```

**Validaciones:**

| Campo              | Regla                                                               |
|--------------------|---------------------------------------------------------------------|
| objetoSolicitadoId | Requerido. Objeto existente, Disponible, NO propio (RN-022).        |
| objetoOfrecidoId   | Requerido. Objeto existente, Disponible y propiedad del solicitante.|
| mensajeInicial     | Opcional. Máx. 500 chars.                                           |

**Response 201 Created:** intercambio en estado `Pendiente`. Notifica al propietario (SolicitudRecibida).

**Errores:**

| HTTP | Condición                                        | Mensaje                                  |
|------|--------------------------------------------------|------------------------------------------|
| 409  | El objeto solicitado es propio                   | "No puedes solicitar tu propio objeto."  |
| 409  | Alguno de los objetos no está disponible         | "El objeto no está disponible."          |
| 409  | Ya existe una solicitud activa entre esos objetos| "Ya existe una solicitud activa."        |

### GET /api/v1/exchanges
**Acceso:** AUTH

Lista paginada de los intercambios del usuario (como solicitante o propietario). Filtros: `estadoIntercambio`, `rol` (enviados/recibidos).

### GET /api/v1/exchanges/{id}
**Acceso:** AUTH

Detalle de un intercambio. **403** si el usuario no es parte. Incluye objetos, ambas partes y estado.

### PATCH /api/v1/exchanges/{id}/accept
**Acceso:** AUTH | **Caso de Uso:** UC-021 | **Reglas:** RN-020, RN-023

Solo el **propietario** del objeto solicitado puede aceptar. Cambia estado a `Aceptado`, fija `fechaAceptacion`, reserva ambos objetos y notifica al solicitante.

**Errores:** 403 si no es el propietario (RN-023); 409 si el estado no es `Pendiente`.

### PATCH /api/v1/exchanges/{id}/reject
**Acceso:** AUTH | **Caso de Uso:** UC-021 | **Reglas:** RN-023, RN-024

Solo el propietario puede rechazar. Estado → `Rechazado` (final). Notifica al solicitante.

### PATCH /api/v1/exchanges/{id}/confirm
**Acceso:** AUTH | **Caso de Uso:** UC-022 | **Reglas:** RN-020, RN-026

Cada parte confirma la recepción. Marca `confirmacionSolicitante` o `confirmacionPropietario`. Cuando **ambas** son true: estado → `Completado`, objetos → `Intercambiado`, incrementa `totalIntercambios` de ambos.

**Request Body:** vacío (el usuario se identifica por el token).

**Response 200 OK:**
```json
{
  "success" : true,
  "message" : "Confirmación registrada.",
  "data"    : {
    "id"                     : "uuid-intercambio",
    "estadoIntercambio"      : "PendienteConfirmacion",
    "confirmacionSolicitante": true,
    "confirmacionPropietario": false
  },
  "errors"    : null,
  "timestamp" : "2026-06-03T00:00:00Z"
}
```

**Errores:** 403 si no es parte; 409 si el estado no permite confirmar.

### PATCH /api/v1/exchanges/{id}/cancel
**Acceso:** AUTH | **Caso de Uso:** UC-021

Cualquiera de las partes puede cancelar mientras no esté `Completado`/`Rechazado`. Estado → `Cancelado`, libera los objetos a `Disponible`.

---

## 8. Módulo RATINGS — Calificaciones

**Base:** `/api/v1/ratings`

### POST /api/v1/ratings
**Acceso:** AUTH | **Caso de Uso:** UC-022 | **Reglas:** RN-025, RN-030, RN-031, RN-032

**Request Body:**
```json
{
  "intercambioId": "uuid-intercambio",
  "puntuacion"   : 5,
  "comentario"   : "Excelente intercambio, todo según lo acordado."
}
```

**Validaciones:**

| Campo         | Regla                                                              |
|---------------|--------------------------------------------------------------------|
| intercambioId | Requerido. Intercambio Completado en el que participa el usuario.  |
| puntuacion    | Requerido. Entero 1–5 (RN-031).                                    |
| comentario    | Opcional. Máx. 500 chars.                                          |

**Response 201 Created:** calificación registrada; recalcula la reputación del calificado (RN-032).

**Errores:**
 
| HTTP | Condición                          | Mensaje                                           |
|------|------------------------------------|---------------------------------------------------|
| 409  | Ya calificó este intercambio       | "Ya has calificado este intercambio."             |
| 422  | Intercambio no completado          | "Solo puedes calificar intercambios completados." |
| 403  | No participó en el intercambio     | "No participaste en este intercambio."            |

### GET /api/v1/ratings/user/{userId}
**Acceso:** PUB

Lista paginada de las calificaciones recibidas por un usuario (puntuación, comentario, fecha).

---

## 9. Módulo MESSAGES — Mensajería

**Base:** `/api/v1/messages`

### GET /api/v1/messages/exchange/{exchangeId}
**Acceso:** AUTH | **Caso de Uso:** UC (chat, V2)

Lista los mensajes de un intercambio (orden cronológico). **403** si el usuario no es parte del intercambio.

### POST /api/v1/messages/exchange/{exchangeId}
**Acceso:** AUTH

**Request Body:**
```json
{ "contenido": "Perfecto, nos vemos mañana a las 3pm en el centro." }
```

**Validaciones:** contenido requerido, no vacío, máx. 1000 chars. Solo partes del intercambio.

**Response 201 Created:** mensaje creado; notifica al destinatario (NuevoMensaje).

---
## 10. Módulo NOTIFICATIONS — Notificaciones

**Base:** `/api/v1/notifications`

### GET /api/v1/notifications
**Acceso:** AUTH

Lista paginada de las notificaciones del usuario autenticado. Filtro opcional `isLeida` (true/false).

**Response 200 OK (Paginado):**
```json
{
  "success" : true, "message": "OK",
  "data"    : {
    "items": [
      {
        "id"         : "uuid-notif",
        "tipo"       : "SolicitudRecibida",
        "titulo"     : "Nueva solicitud de intercambio",
        "mensaje"    : "Juan Carlos quiere intercambiar por tu bicicleta.",
        "isLeida"    : false,
        "entidadTipo": "Intercambio",
        "entidadId"  : "uuid-intercambio",
        "creadaEn"   : "2026-06-03T00:00:00Z"
      }
    ],
    "pageNumber": 1, "pageSize": 20, "totalRecords": 5,
    "totalPages": 1, "hasPrevious": false, "hasNext": false
  },
  "errors": null, "timestamp": "2026-06-03T00:00:00Z"
}
```

### PATCH /api/v1/notifications/{id}/read
**Acceso:** AUTH

Marca una notificación como leída. **403** si no pertenece al usuario.

### PATCH /api/v1/notifications/read-all
**Acceso:** AUTH

Marca todas las notificaciones del usuario como leídas.

---

## 11. Módulo FAVORITES — Favoritos

**Base:** `/api/v1/favorites`

### GET /api/v1/favorites
**Acceso:** AUTH

Lista paginada de los objetos marcados como favoritos por el usuario.

### POST /api/v1/favorites
**Acceso:** AUTH | **Reglas:** RN-042

**Request Body:**
```json
{ "objetoId": "uuid-objeto" }
```

**Response 201 Created:** favorito agregado.

**Errores:** 409 si ya está en favoritos (RN-042, único por usuario+objeto).

### DELETE /api/v1/favorites/{objetoId}
**Acceso:** AUTH

Quita un objeto de favoritos. **404** si no estaba marcado.

---

## 12. Módulo REPORTS — Reportes

**Base:** `/api/v1/reports`

### POST /api/v1/reports
**Acceso:** AUTH | **Caso de Uso:** UC-040 | **Reglas:** RN-040, RN-042

**Request Body:**
```json
{
  "entidadTipo": "Objeto",
  "entidadId"  : "uuid-objeto",
  "motivo"     : "ContenidoInapropiado",
  "descripcion": "El objeto publicado infringe las normas de la comunidad."
}
```

**Validaciones:**

| Campo       | Regla                                                                      |
|-------------|----------------------------------------------------------------------------|
| entidadTipo | Requerido. "Objeto" o "Usuario".                                           |
| entidadId   | Requerido. Entidad existente.                                              |
| motivo      | Requerido. ContenidoInapropiado / Fraude / Spam / InformacionFalsa / Otro. |
| descripcion | Opcional. Máx. 500 chars.                                                  |

**Response 201 Created:** reporte en estado `Pendiente`.

**Errores:** 409 si ya existe un reporte abierto del mismo usuario sobre la misma entidad.

---

## 13. Módulo GEO — Datos Geográficos

**Base:** `/api/v1/geo`

> Datos maestros UBIGEO Perú. Públicos para poblar selectores en formularios.

### GET /api/v1/geo/departamentos
**Acceso:** PUB | **Reglas:** RN-050

**Response 200 OK:** `[{ "id": 5, "ubigeo": "05", "nombre": "Ayacucho" }, ...]`

### GET /api/v1/geo/provincias?departamentoId={id}
**Acceso:** PUB | **Reglas:** RN-051

Lista las provincias de un departamento. Requiere query `departamentoId`.

### GET /api/v1/geo/distritos?provinciaId={id}
**Acceso:** PUB | **Reglas:** RN-051

Lista los distritos de una provincia. Requiere query `provinciaId`.

### GET /api/v1/geo/categorias
**Acceso:** PUB | **Reglas:** RN-010

Lista las categorías activas. `[{ "id": 6, "nombre": "Deportes", "icono": "..." }, ...]`

---

## 14. Módulo ADMIN — Administración

**Base:** `/api/v1/admin`

### GET /api/v1/admin/dashboard
**Acceso:** MOD | **Caso de Uso:** UC-081 | **Reglas:** RN-060

**Response 200 OK:**
```json
{
  "success": true, "message": "OK",
  "data": {
    "totalUsuarios"          : 1250,
    "totalObjetos"           : 870,
    "intercambiosCompletados": 340,
    "intercambiosPendientes" : 45,
    "reportesPendientes"     : 8,
    "usuariosActivos30d"     : 410
  },
  "errors": null, "timestamp": "2026-06-03T00:00:00Z"
}
```

### GET /api/v1/admin/users
**Acceso:** ADMIN | **Caso de Uso:** UC-080 | **Reglas:** RN-060

Lista paginada de usuarios con filtros (search, rol, isActive).

### PATCH /api/v1/admin/users/{id}/activate
**Acceso:** ADMIN | **Caso de Uso:** UC-080

Reactiva una cuenta desactivada.

### PATCH /api/v1/admin/users/{id}/deactivate
**Acceso:** ADMIN | **Caso de Uso:** UC-080

Desactiva una cuenta (no la elimina).

### DELETE /api/v1/admin/users/{id}
**Acceso:** ADMIN | **Caso de Uso:** UC-080 | **Reglas:** RN-013

Eliminación lógica (soft delete) de un usuario.

### PATCH /api/v1/admin/users/{id}/role
**Acceso:** ADMIN | **Caso de Uso:** UC-080

**Request Body:** `{ "rolId": 2 }`. Cambia el rol del usuario.

### GET /api/v1/admin/objects
**Acceso:** MOD | **Caso de Uso:** UC-080

Lista paginada de todos los objetos (incluye suspendidos) con filtros.

### PATCH /api/v1/admin/objects/{id}/suspend
**Acceso:** MOD | **Caso de Uso:** UC-041

Suspende un objeto (estado → Suspendido). No visible en búsquedas.

### PATCH /api/v1/admin/objects/{id}/restore
**Acceso:** ADMIN

Restaura un objeto suspendido a Disponible.

### GET /api/v1/admin/categories
**Acceso:** ADMIN | **Caso de Uso:** UC-080

Lista todas las categorías (activas e inactivas).

### POST /api/v1/admin/categories
**Acceso:** ADMIN | **Caso de Uso:** UC-080

**Request Body:** `{ "nombre": "Instrumentos Musicales", "descripcion": "...", "icono": "..." }`

### PATCH /api/v1/admin/categories/{id}
**Acceso:** ADMIN | **Caso de Uso:** UC-080

Actualiza una categoría.

### PATCH /api/v1/admin/categories/{id}/deactivate
**Acceso:** ADMIN | **Caso de Uso:** UC-080 | **Reglas:** RN-010

Desactiva una categoría (is_active = 0). No se elimina físicamente.

### GET /api/v1/admin/reports
**Acceso:** MOD | **Caso de Uso:** UC-041 | **Reglas:** RN-041

Lista paginada de reportes con filtro por estado.

### PATCH /api/v1/admin/reports/{id}/review
**Acceso:** MOD | **Caso de Uso:** UC-041 | **Reglas:** RN-041

Marca un reporte como EnRevision.

### PATCH /api/v1/admin/reports/{id}/resolve
**Acceso:** MOD | **Caso de Uso:** UC-041 | **Reglas:** RN-041

Resuelve un reporte (estado → Resuelto, registra resuelto_por y fecha).

### PATCH /api/v1/admin/reports/{id}/discard
**Acceso:** MOD | **Caso de Uso:** UC-041 | **Reglas:** RN-041

Descarta un reporte (estado → Descartado).

### GET /api/v1/admin/audit-logs
**Acceso:** ADMIN | **Caso de Uso:** UC-090 | **Reglas:** RN-063

**Query Parameters:** usuarioId, accion, fechaDesde, fechaHasta, pageNumber, pageSize.

**Response 200 OK (Paginado):**
```json
{
  "success": true, "message": "OK",
  "data": {
    "items": [
      {
        "id"         : "uuid-log",
        "usuario"    : { "id": "uuid", "nombres": "Juan Carlos", "email": "juan@example.com" },
        "accion"     : "Login_Exitoso",
        "entidadTipo": "Sesion",
        "resultado"  : "Exitoso",
        "ipAddress"  : "192.168.1.1",
        "ocurridoEn" : "2026-06-03T00:00:00Z"
      }
    ],
    "pageNumber": 1, "pageSize": 50, "totalRecords": 1250,
    "totalPages": 25, "hasPrevious": false, "hasNext": true
  },
  "errors": null, "timestamp": "2026-06-03T00:00:00Z"
}
```

---

## 15. Códigos de Error Estándar

| Código HTTP | Nombre                | Cuándo se usa                                             |
|-------------|-----------------------|-----------------------------------------------------------|
| 200         | OK                    | Operación exitosa (GET, PUT, PATCH, DELETE).              |
| 201         | Created               | Recurso creado exitosamente (POST).                       |
| 400         | Bad Request           | Request malformado. Token expirado/usado.                 |
| 401         | Unauthorized          | Sin token o token inválido. Credenciales incorrectas.     |
| 403         | Forbidden             | Autenticado pero sin permisos para la acción.             |
| 404         | Not Found             | Recurso no encontrado o eliminado.                        |
| 409         | Conflict              | Conflicto de estado: duplicado, objeto no disponible.     |
| 422         | Unprocessable Entity  | Validación de negocio fallida. Campos inválidos.          |
| 423         | Locked                | Cuenta bloqueada por intentos fallidos.                   |
| 429         | Too Many Requests     | Rate Limit superado. Header Retry-After incluido.         |
| 500         | Internal Server Error | Error no controlado del servidor. Se registra en Serilog. |

---

## 16. Matriz de Endpoints

| Método | Endpoint                            | Acceso | Caso de Uso | Reglas                 |
|--------|-------------------------------------|--------|-------------|------------------------|
| POST   | /auth/register                      | PUB    | UC-001      | RN-002, RN-003, RN-004 |
| POST   | /auth/login                         | PUB    | UC-002      | RN-061, RN-063, RN-064 |
| POST   | /auth/refresh                       | PUB    | —           | RN-064                 |
| POST   | /auth/logout                        | AUTH   | UC-004      | RN-064                 |
| POST   | /auth/forgot-password               | PUB    | UC-003      | RN-061                 |
| POST   | /auth/reset-password                | PUB    | UC-003      | RN-004, RN-061         |
| GET    | /users/{id}                         | PUB    | UC-012      | —                      |
| GET    | /users/me                           | AUTH   | UC-012      | —                      |
| PUT    | /users/me                           | AUTH   | UC-012      | RN-005, RN-050, RN-051 |
| PATCH  | /users/me/photo                     | AUTH   | UC-012      | —                      |
| PATCH  | /users/me/password                  | AUTH   | UC-012      | RN-004, RN-061         |
| GET    | /objects                            | PUB    | UC-030      | RN-014                 |
| GET    | /objects/{id}                       | PUB    | UC-030      | RN-014                 |
| POST   | /objects                            | AUTH   | UC-010      | RN-001, RN-010, RN-011 |
| PUT    | /objects/{id}                       | AUTH   | UC-011      | RN-005                 |
| DELETE | /objects/{id}                       | AUTH   | UC-011      | RN-005, RN-013         |
| GET    | /objects/me                         | AUTH   | —           | —                      |
| GET    | /objects/me/available               | AUTH   | UC-020      | —                      |
| POST   | /exchanges                          | AUTH   | UC-020      | RN-001, RN-014, RN-022 |
| GET    | /exchanges                          | AUTH   | —           | —                      |
| GET    | /exchanges/{id}                     | AUTH   | —           | —                      |
| PATCH  | /exchanges/{id}/accept              | AUTH   | UC-021      | RN-020, RN-023         |
| PATCH  | /exchanges/{id}/reject              | AUTH   | UC-021      | RN-023, RN-024         |
| PATCH  | /exchanges/{id}/confirm             | AUTH   | UC-022      | RN-020, RN-026         |
| PATCH  | /exchanges/{id}/cancel              | AUTH   | UC-021      | —                      |
| POST   | /ratings                            | AUTH   | UC-022      | RN-025, RN-030, RN-031 |
| GET    | /ratings/user/{userId}              | PUB    | —           | —                      |
| GET    | /messages/exchange/{exchangeId}     | AUTH   | —           | —                      |
| POST   | /messages/exchange/{exchangeId}     | AUTH   | —           | —                      |
| GET    | /notifications                      | AUTH   | —           | —                      |
| PATCH  | /notifications/{id}/read            | AUTH   | —           | —                      |
| PATCH  | /notifications/read-all             | AUTH   | —           | —                      |
| GET    | /favorites                          | AUTH   | —           | —                      |
| POST   | /favorites                          | AUTH   | —           | RN-042                 |
| DELETE | /favorites/{objetoId}               | AUTH   | —           | —                      |
| POST   | /reports                            | AUTH   | UC-040      | RN-040, RN-042         |
| GET    | /geo/departamentos                  | PUB    | —           | RN-050                 |
| GET    | /geo/provincias                     | PUB    | —           | RN-051                 |
| GET    | /geo/distritos                      | PUB    | —           | RN-051                 |
| GET    | /geo/categorias                     | PUB    | —           | RN-010                 |
| GET    | /admin/dashboard                    | MOD    | UC-081      | RN-060                 |
| GET    | /admin/users                        | ADMIN  | UC-080      | RN-060                 |
| PATCH  | /admin/users/{id}/activate          | ADMIN  | UC-080      | RN-060                 |
| PATCH  | /admin/users/{id}/deactivate        | ADMIN  | UC-080      | RN-060                 |
| DELETE | /admin/users/{id}                   | ADMIN  | UC-080      | RN-013                 |
| PATCH  | /admin/users/{id}/role              | ADMIN  | UC-080      | RN-060                 |
| GET    | /admin/objects                      | MOD    | UC-080      | RN-060                 |
| PATCH  | /admin/objects/{id}/suspend         | MOD    | UC-041      | RN-041                 |
| PATCH  | /admin/objects/{id}/restore         | ADMIN  | —           | RN-060                 |
| GET    | /admin/categories                   | ADMIN  | UC-080      | RN-010                 |
| POST   | /admin/categories                   | ADMIN  | UC-080      | RN-010                 |
| PATCH  | /admin/categories/{id}              | ADMIN  | UC-080      | RN-010                 |
| PATCH  | /admin/categories/{id}/deactivate   | ADMIN  | UC-080      | RN-010                 |
| GET    | /admin/reports                      | MOD    | UC-041      | RN-041                 |
| PATCH  | /admin/reports/{id}/review          | MOD    | UC-041      | RN-041                 |
| PATCH  | /admin/reports/{id}/resolve         | MOD    | UC-041      | RN-041                 |
| PATCH  | /admin/reports/{id}/discard         | MOD    | UC-041      | RN-041                 |
| GET    | /admin/audit-logs                   | ADMIN  | UC-090      | RN-063                 |

**Total de endpoints: 57**

---

## 17. Aprobación

| Rol                           | Nombre            | Aprobación  | Fecha |
|-------------------------------|-------------------|-------------|-------|
| Product Owner                 | —                 | ☐ PENDIENTE | —     |
| Arquitecto de Software Senior | Equipo Enterprise | ☐ PENDIENTE | —     |
| Backend Developer Senior      | Equipo Enterprise | ☐ PENDIENTE | —     |
| Especialista en Seguridad     | Equipo Enterprise | ☐ PENDIENTE | —     |

---

> **GATE DE CALIDAD — FASE 2, PASO 8:**
> Este es el último documento de la **Fase 2 — Diseño**.
> Con su aprobación, la documentación SDD queda completa para iniciar la **Fase 3 — Implementación**.
> El código fuente debe implementarse estrictamente según los contratos aquí definidos.
> Ningún endpoint puede existir sin estar documentado en este archivo.

---

*Documento generado bajo la metodología SDD — Plataforma Inteligente de Intercambio de Objetos — Ayacucho, Perú.*
