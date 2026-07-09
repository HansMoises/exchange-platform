# Seguridad.md
# Plataforma Inteligente de Intercambio de Objetos

> **Documento:** Especificación de Seguridad
> **Fase SDLC:** 2 (Diseño) — documento complementario
> **Versión:** 1.0.0
> **Estado:** `PENDIENTE DE APROBACIÓN`
> **Fecha:** 2026-06-03
> **Autor:** Equipo Enterprise Senior (Especialista en Seguridad / Arquitecto)
> **Documentos padre:** Requisitos.md | ReglasNegocio.md | Arquitectura.md | BD.md | API.md
> **Convenciones:** Documentación en español. Diagramas en ASCII.

---

## Control de Versiones

| Versión | Fecha      | Autor                    | Cambios          |
|---------|------------|--------------------------|------------------|
| 1.0.0   | 2026-06-03 | Equipo Enterprise Senior | Versión inicial. |

---

## Tabla de Contenidos

1. Principios de Seguridad
2. Autenticación (JWT)
3. Política de Contraseñas
4. Ciclo de Vida de Tokens
5. Autorización — Roles y Permisos
6. Matriz de Permisos por Rol
7. Matriz de Permisos por Endpoint
8. Mitigación OWASP Top 10
9. Protección de Datos Sensibles
10. Rate Limiting y Bloqueo de Cuenta
11. Matriz de Auditoría
12. Trazabilidad y Aprobación

---

## 1. Principios de Seguridad

| Principio          | Aplicación en el Proyecto                                                       |
|--------------------|---------------------------------------------------------------------------------|
| Confidencialidad   | Cifrado en tránsito (HTTPS), hashing de contraseñas, secretos fuera del código. |
| Integridad         | Validación en frontend, backend y BD; constraints; auditoría inmutable.         |
| Disponibilidad     | Rate limiting, backups, manejo de errores controlado.                           |
| Trazabilidad       | AuditLog de toda acción sensible (RN-062, RN-063).                              |
| No repudio         | Identidad no anónima + auditoría con IP y timestamp.                            |
| Defensa en profundidad | Seguridad en cada capa: red, API, aplicación, datos.                        |
| Mínimo privilegio  | Cada rol y endpoint tiene solo los permisos necesarios (RN-060).                |
| Seguridad por diseño | La seguridad se diseña desde el inicio (SDD), no se añade después.            |

---

## 2. Autenticación (JWT)

### 2.1 Esquema

El sistema usa autenticación **stateless** basada en JSON Web Tokens (ADR-004).

```
┌──────────┐   1. POST /auth/login (email + password)   ┌──────────┐
│  Cliente │ ──────────────────────────────────────────►│   API    │
│          │                                            │          │
│          │   2. { accessToken (15min), refreshToken } │          │
│          │ ◄──────────────────────────────────────────│          │
│          │                                            │          │
│          │   3. Request + Authorization: Bearer {AT}  │          │
│          │ ──────────────────────────────────────────►│          │
│          │                                            │          │
│          │   4. AT expira → POST /auth/refresh {RT}   │          │
│          │ ──────────────────────────────────────────►│          │
│          │   5. Nuevo AT + nuevo RT (rotación)        │          │
│          │ ◄──────────────────────────────────────────│          │
└──────────┘                                            └──────────┘
```

### 2.2 Estructura del Access Token (JWT)

| Claim | Descripción                                  |
|-------|----------------------------------------------|
| sub   | ID del usuario (UUID).                       |
| email | Correo del usuario.                          |
| role  | Rol: Administrador / Moderador / Usuario.    |
| iat   | Fecha de emisión (issued at).                |
| exp   | Fecha de expiración (15 minutos).            |
| iss   | Emisor: exchange-platform.                   |
| aud   | Audiencia: exchange-platform-users.          |

### 2.3 Parámetros

| Parámetro                  | Valor                          |
|----------------------------|--------------------------------|
| Algoritmo de firma         | HMAC SHA-256 (HS256)           |
| Expiración Access Token    | 15 minutos                     |
| Expiración Refresh Token   | 7 días                         |
| Rotación de Refresh Token  | Sí (cada uso genera uno nuevo) |
| Almacenamiento del secreto | Variable de entorno JWT_SECRET |

---

## 3. Política de Contraseñas

Concreta la regla RN-004 (que se dejó abierta en la Fase 1).

| Requisito              | Valor                                                    |
|------------------------|----------------------------------------------------------|
| Longitud mínima        | 8 caracteres                                             |
| Longitud máxima        | 100 caracteres                                           |
| Mayúscula              | Al menos 1 (A-Z)                                         |
| Minúscula              | Al menos 1 (a-z)                                         |
| Número                 | Al menos 1 (0-9)                                         |
| Carácter especial      | Al menos 1 (!@#$%^&* etc.)                               |
| Algoritmo de hashing   | BCrypt, work factor 12                                   |
| Almacenamiento         | Solo el hash. Nunca texto plano. Nunca en logs (RN-061). |
| Validación             | En frontend (Zod) y backend (FluentValidation).          |

---

## 4. Ciclo de Vida de Tokens

```
EMISIÓN (login):
  - Genera Access Token (15 min) firmado.
  - Genera Refresh Token (UUID opaco), se guarda hash en BD (tabla RefreshTokens).

USO (request protegido):
  - Middleware valida firma y expiración del Access Token.
  - Si válido → procesa. Si expirado → 401.

RENOVACIÓN (refresh):
  - Cliente envía Refresh Token.
  - Backend verifica que exista, no esté revocado y no haya expirado.
  - Rotación: revoca el Refresh Token usado y emite uno nuevo (RN-064).
  - Emite nuevo Access Token.

REVOCACIÓN (logout):
  - Marca is_revoked = 1 en el Refresh Token.

EXPIRACIÓN:
  - Access Token: por exp (15 min).
  - Refresh Token: por expires_at (7 días).
```

---

## 5. Autorización — Roles y Permisos

El sistema implementa autorización basada en roles (RBAC). Toda acción sensible valida el rol/permiso antes de ejecutarse (RN-060).

| Rol           | Nivel | Descripción                                             |
|---------------|-------|---------------------------------------------------------|
| Administrador | 3     | Acceso total. Gestiona usuarios, categorías, auditoría. |
| Moderador     | 2     | Gestiona reportes, suspende objetos, ve dashboard.      |
| Usuario       | 1     | Funciones estándar de la plataforma.                    |
| Anónimo       | 0     | Solo endpoints públicos.                                |

---

## 6. Matriz de Permisos por Rol

| Acción                          | Anónimo | Usuario | Moderador | Administrador |
|---------------------------------|---------|---------|-----------|---------------|
| Registrarse / Iniciar sesión    | Sí      | —       | —         | —             |
| Buscar / ver objetos            | Sí      | Sí      | Sí        | Sí            |
| Publicar / editar objeto propio | No      | Sí      | Sí        | Sí            |
| Solicitar / gestionar intercambio | No    | Sí      | Sí        | Sí            |
| Calificar tras intercambio      | No      | Sí      | Sí        | Sí            |
| Reportar objeto / usuario       | No      | Sí      | Sí        | Sí            |
| Gestionar reportes              | No      | No      | Sí        | Sí            |
| Suspender objeto                | No      | No      | Sí        | Sí            |
| Ver dashboard                   | No      | No      | Sí        | Sí            |
| Gestionar usuarios              | No      | No      | No        | Sí            |
| Gestionar categorías            | No      | No      | No        | Sí            |
| Cambiar roles                   | No      | No      | No        | Sí            |
| Consultar auditoría             | No      | No      | No        | Sí            |

---

## 7. Matriz de Permisos por Endpoint

| Endpoint                          | Anónimo | Usuario | Moderador | Admin |
|-----------------------------------|---------|---------|-----------|-------|
| POST /auth/*                      | Sí      | Sí      | Sí        | Sí    |
| GET /objects, /objects/{id}       | Sí      | Sí      | Sí        | Sí    |
| GET /geo/*                        | Sí      | Sí      | Sí        | Sí    |
| GET /ratings/user/{id}            | Sí      | Sí      | Sí        | Sí    |
| /users/me, /objects (POST/PUT/DEL)| No      | Sí      | Sí        | Sí    |
| /exchanges/*                      | No      | Sí      | Sí        | Sí    |
| /ratings (POST)                   | No      | Sí      | Sí        | Sí    |
| /favorites/*, /messages/*         | No      | Sí      | Sí        | Sí    |
| /reports (POST)                   | No      | Sí      | Sí        | Sí    |
| /notifications/*                  | No      | Sí      | Sí        | Sí    |
| /admin/dashboard                  | No      | No      | Sí        | Sí    |
| /admin/objects, /admin/reports/*  | No      | No      | Sí        | Sí    |
| /admin/objects/{id}/suspend       | No      | No      | Sí        | Sí    |
| /admin/users/*                    | No      | No      | No        | Sí    |
| /admin/categories/*               | No      | No      | No        | Sí    |
| /admin/audit-logs                 | No      | No      | No        | Sí    |

---

## 8. Mitigación OWASP Top 10
  
| #  | Riesgo OWASP 2021             | Mitigación en el proyecto                                                        | Verificación |
|----|-------------------------------|----------------------------------------------------------------------------------|--------------|
| A01| Broken Access Control         | RBAC por endpoint, validación en servidor, RoleBasedRoute en frontend (RN-060).  | PR-081       |
| A02| Cryptographic Failures        | BCrypt factor 12, HTTPS obligatorio, secretos en variables de entorno.           | PR-081       |
| A03| Injection                     | EF Core parametrizado (sin SQL crudo), validación FluentValidation/Zod.          | PR-081       |
| A04| Insecure Design               | SDD + Clean Architecture + threat modeling + revisiones por fase.                | Revisión     |
| A05| Security Misconfiguration     | Config por ambiente, Swagger off en prod, headers seguros, sin stack traces.     | PR-081       |
| A06| Vulnerable Components         | Dependencias actualizadas, GitHub Dependabot, revisión de CVE.                   | CI           |
| A07| Identification/Auth Failures  | Bloqueo tras 5 intentos, rate limiting, rotación de refresh token, BCrypt.       | PR-081       |
| A08| Software/Data Integrity       | CI/CD con checks, code review obligatorio, validación de integridad de imágenes. | CI           |
| A09| Logging/Monitoring Failures   | Serilog estructurado, AuditLog inmutable, alertas en eventos críticos.           | PR-070/071   |
| A10| SSRF                          | Sin fetch de URLs arbitrarias; dominios externos en whitelist.                   | Revisión     |

### 8.1 Headers de Seguridad HTTP

| Header                    | Valor                                  |
|---------------------------|----------------------------------------|
| Strict-Transport-Security | max-age=31536000; includeSubDomains    |
| X-Content-Type-Options    | nosniff                                |
| X-Frame-Options           | DENY                                   |
| Content-Security-Policy   | default-src 'self'                     |
| Referrer-Policy           | strict-origin-when-cross-origin        |

---

## 9. Protección de Datos Sensibles

| Dato                  | Protección                                                       |
|-----------------------|------------------------------------------------------------------|
| Contraseña            | Hash BCrypt. Nunca se devuelve ni se registra en logs.           |
| Refresh Token         | Se guarda hasheado en BD; el cliente recibe el valor opaco.      |
| Datos personales      | Visibles solo según contexto (no anónimo en intercambio, RN-021).|
| Tokens en tránsito    | Solo sobre HTTPS en Staging/Producción.                          |
| Secretos (JWT, BD)    | Variables de entorno por ambiente. Nunca en el repositorio.      |
| Logs                  | Sin datos sensibles (contraseñas, tokens, PII innecesaria).      |

---

## 10. Rate Limiting y Bloqueo de Cuenta

### 10.1 Rate Limiting

| Ámbito                 | Límite                          | Respuesta        |
|------------------------|---------------------------------|------------------|
| Login (/auth/login)    | 5 intentos / 15 min por IP+email| 429 + Retry-After|
| Endpoints generales    | 100 requests / minuto por IP    | 429 + Retry-After|
| Registro (/auth/register)| 3 / hora por IP               | 429 + Retry-After|

### 10.2 Bloqueo de Cuenta

Concreta los campos `failed_attempts` y `locked_until` de `BD.md`:

```
- Cada login fallido incrementa failed_attempts.
- Al llegar a 5 intentos fallidos consecutivos:
    locked_until = ahora + 15 minutos
    Respuesta: 423 Locked
- Un login exitoso resetea failed_attempts a 0 y locked_until a null.
- failed_attempts tiene tope 10 (CHECK en BD).
```

---

## 11. Matriz de Auditoría

Toda acción sensible se registra en `AuditLogs` (Quién, Qué, Cuándo, Dónde, Resultado) — RN-062, RN-063.

| Evento                  | Acción registrada       | Entidad     | Datos clave                    |
|-------------------------|-------------------------|-------------|--------------------------------|
| Inicio de sesión        | Login_Exitoso / Fallido | Sesion      | usuario, IP, resultado, fecha  |
| Cierre de sesión        | Logout                  | Sesion      | usuario, IP, fecha             |
| Registro de usuario     | Usuario_Creado          | Usuario     | usuario, IP, fecha             |
| Publicación de objeto   | Objeto_Publicado        | Objeto      | usuario, objetoId, fecha       |
| Edición / eliminación   | Objeto_Editado/Eliminado| Objeto      | usuario, objetoId, fecha       |
| Solicitud de intercambio| Intercambio_Solicitado  | Intercambio | usuario, intercambioId         |
| Aceptación / rechazo    | Intercambio_Aceptado/Rechazado | Intercambio | usuario, intercambioId  |
| Confirmación            | Intercambio_Confirmado  | Intercambio | usuario, intercambioId         |
| Calificación            | Calificacion_Emitida    | Calificacion| usuario, intercambioId         |
| Acción administrativa   | Admin_*                 | Variable    | admin, entidad, resultado      |
| Resolución de reporte   | Reporte_Resuelto        | Reporte     | moderador, reporteId           |

**Campos registrados:** Quién (usuario_id), Qué (accion), Cuándo (ocurrido_en), Dónde (ip_address), Resultado (resultado), entidad afectada.

---

## 12. Trazabilidad y Aprobación

### 12.1 Trazabilidad

| Elemento de seguridad     | Responde a                 |
|---------------------------|----------------------------|
| Hashing BCrypt            | RN-061, RNF-001            |
| RBAC por rol/endpoint     | RN-060, RNF-002, RNF-005   |
| Mitigaciones OWASP        | RNF-003, CE-004            |
| Rate limiting / bloqueo   | RNF-004, RN-007 (login)    |
| Rotación de refresh token | RN-064                     |
| AuditLog                  | RN-062, RN-063, RF-120/121 |
| Identidad no anónima      | RN-021, RN-003             |

### 12.2 Aprobación

| Rol                       | Nombre            | Aprobación  | Fecha |
|---------------------------|-------------------|-------------|-------|
| Especialista en Seguridad | Equipo Enterprise | ☐ PENDIENTE | —     |
| Arquitecto de Software    | Equipo Enterprise | ☐ PENDIENTE | —     |
| Backend Developer Senior  | Equipo Enterprise | ☐ PENDIENTE | —     |

---

> **GATE DE CALIDAD:**
> La política de seguridad aquí definida es obligatoria en toda la implementación.
> Las pruebas de seguridad (PR-081) validarán el cumplimiento de OWASP antes del despliegue.

---

*Documento generado bajo la metodología SDD — Plataforma Inteligente de Intercambio de Objetos — Ayacucho, Perú.*
