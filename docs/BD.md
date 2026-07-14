# BD.md
# Plataforma Inteligente de Intercambio de Objetos

> **Documento:** Diseño de Base de Datos
> **Paso SDD:** 7 de 8 (Base de Datos) — **Fase SDLC:** 2 (Diseño)
> **Versión:** 1.2.2
> **Estado:** `PENDIENTE DE APROBACIÓN`
> **Fecha:** 2026-07-08
> **Autor:** Equipo Enterprise Senior (Arquitecto de Datos / DBA PostgreSQL / Supabase)
> **Documentos padre:** VisionProyecto.md | Requisitos.md | ReglasNegocio.md | CasosDeUso.md | UML.md | Arquitectura.md
> **Motor:** PostgreSQL 15+ (Supabase, servicio gestionado). **Acceso:** Entity Framework Core + Npgsql (Code First + migraciones).
> **Convenciones:** Documentación y nomenclatura en español. Diagramas en ASCII.

---

## Control de Versiones

| Versión | Fecha      | Autor                    | Cambios                                                              |
|---------|------------|--------------------------|----------------------------------------------------------------------|
| 1.0.0   | 2026-06-03 | Equipo Enterprise Senior | Versión inicial (DDL, índices, seed, backup).                        |
| 1.1.0   | 2026-06-03 | Equipo Enterprise Senior | Modelo conceptual ER en ASCII y matriz de trazabilidad BD→RN/RNF.    |
| 1.2.0   | 2026-07-08 | Equipo Enterprise Senior | Migración de motor: SQL Server 2022 → PostgreSQL 15+ (Supabase). DDL, tipos de datos, seed y estrategia de backup adaptados. Ver ADR-010 en `Arquitectura.md`. |
| 1.2.1   | 2026-07-08 | Equipo Enterprise Senior | Corrección post-revisión: paginación documentada como LIMIT/OFFSET (no OFFSET/FETCH, sintaxis T-SQL); aclarada la semántica de `gen_random_uuid()` (UUID aleatorio, no secuencial como `NEWSEQUENTIALID()` — ya no se afirma que reduce fragmentación de índices). |
| 1.2.2   | 2026-07-14 | Equipo Enterprise Senior | Aclarado el origen de `ImagenesObjeto.url`: CDN de Supabase Storage en Staging/Prod, disco local en Dev/Test. Ver ADR-013 en `Arquitectura.md`. |

---

## Tabla de Contenidos

1. Principios de Diseño de la Base de Datos
2. Modelo Conceptual (Entidad-Relación)
3. Normalización
4. Convenciones de Nomenclatura
5. Tablas Maestras
6. Tablas de Seguridad y Acceso
7. Tablas Transaccionales
8. Tabla de Auditoría
9. Índices
10. Diccionario de Datos Resumido
11. Scripts de Datos Iniciales (Seed)
12. Estrategia de Backup
13. Consideraciones de Rendimiento
14. Trazabilidad (BD → Reglas/Requisitos)
15. Aprobación

---

## 1. Principios de Diseño de la Base de Datos

| Principio                | Aplicación                                                                      |
|--------------------------|---------------------------------------------------------------------------------|
| Normalización hasta 3FN  | Eliminar redundancia. Garantizar integridad referencial.                        |
| Soft Delete universal    | is_deleted + deleted_at + deleted_by en todas las entidades transaccionales.    |
| Auditoría completa       | created_at + created_by + updated_at + updated_by en entidades transaccionales. |
| Claves primarias UUID    | UUID para entidades transaccionales. INT para tablas maestras.      |
| Integridad referencial   | FOREIGN KEY con ON DELETE NO ACTION. Sin borrado en cascada.                    |
| Constraints de dominio   | CHECK constraints para enums y rangos de valores.                               |
| Índices estratégicos     | En columnas de búsqueda, filtrado y JOIN frecuentes.                            |
| Preparación para escala  | UBIGEO completo del Perú desde V1. Sin cambios de esquema al crecer.            |

---

## 2. Modelo Conceptual (Entidad-Relación)

Vista de alto nivel de entidades y relaciones (independiente de tipos físicos). El modelo lógico y físico se detalla en las secciones 5 a 8.

```
                   ┌───────────────┐
                   │ Departamentos │
                   └───────┬───────┘
                           │ 1:N
                   ┌───────▼───────┐
                   │  Provincias   │
                   └───────┬───────┘
                           │ 1:N
                   ┌───────▼───────┐
                   │   Distritos   │
                   └───┬───────┬───┘
              ubica 1:N│       │1:N ubica
            ┌──────────▼──┐ ┌──▼──────────┐
            │  Usuarios   │ │   Objetos   │
            └──┬───┬───┬──┘ └──┬───┬───┬──┘
   1:N publica │   │   │       │   │   │
              ┌▼───┴┐  │       │   │  ┌▼──────────────┐
              │Roles│  │       │   │  │ ImagenesObjeto│ (1:N)
              └─────┘  │       │   │  └───────────────┘
                       │       │   │ N:1
                       │       │  ┌▼──────────┐
                       │       │  │Categorias │
                       │       │  └───────────┘
                       │       │
    ┌──────────────────┴───────┴────────────────────┐
    │                 Intercambios                  │
    │  (objeto_solicitado, objeto_ofrecido,         │
    │   usuario_solicitante, usuario_propietario)   │
    └───┬────────────┬───────────────┬──────────────┘
   1:N  │       1:N  │          1:N  │
  ┌─────▼─────┐ ┌────▼────┐  ┌───────▼──────┐
  │Calificac. │ │Mensajes │  │(estado/flujo)│
  └───────────┘ └─────────┘  └──────────────┘

  Otras relaciones de Usuarios:
   Usuarios 1:N Favoritos N:1 Objetos
   Usuarios 1:N Notificaciones
   Usuarios 1:N Reportes
   Usuarios 1:N RefreshTokens
   Usuarios 1:N AuditLogs
```

> **Modelo de intercambio:** trueque objeto-por-objeto. Cada intercambio referencia el objeto solicitado y el ofrecido, con confirmación de ambas partes (RN-020, RN-021).

---

## 3. Normalización

### 3.1 Primera Forma Normal (1FN)
- Todos los atributos son atómicos (un valor por celda).
- No existen grupos repetitivos.
- Cada tabla tiene clave primaria definida.
- Las imágenes de objetos se almacenan en tabla separada `ImagenesObjeto`.

### 3.2 Segunda Forma Normal (2FN)
- Cumple 1FN.
- Todos los atributos no clave dependen completamente de la clave primaria.
- No existen dependencias parciales (aplicable a claves compuestas).
- Los datos geográficos están separados en `Departamentos`, `Provincias` y `Distritos`.

### 3.3 Tercera Forma Normal (3FN)
- Cumple 2FN.
- No existen dependencias transitivas entre atributos no clave.
- El nombre del departamento no se almacena en `Usuarios` (se obtiene por JOIN).
- El nombre de categoría no se repite en `Objetos` (se referencia por FK).
- `calificacion_promedio` en `Usuarios` es un campo calculado y persistido (desnormalización controlada y justificada por rendimiento — ver sección 13).

---

## 4. Convenciones de Nomenclatura

| Elemento          | Convención                       | Ejemplo                         |
|-------------------|----------------------------------|---------------------------------|
| Tablas            | PascalCase, plural               | Usuarios, Objetos, Intercambios |
| Columnas          | snake_case                       | created_at, usuario_id          |
| Claves primarias  | id                               | id UUID / id INT    |
| Claves foráneas   | {tabla_referenciada_singular}_id | usuario_id, categoria_id        |
| Índices           | IX_{Tabla}_{Columna(s)}          | IX_Usuarios_Email               |
| Índices únicos    | UX_{Tabla}_{Columna(s)}          | UX_Usuarios_Email               |
| Constraints CHECK | CK_{Tabla}_{Columna}             | CK_Objetos_EstadoObjeto         |
| Proyecto Supabase | exchange-platform-{ambiente}     | exchange-platform-dev, exchange-platform-prod |

> **Nota sobre identificadores en PostgreSQL:** a diferencia de SQL Server, PostgreSQL pliega a minúsculas los identificadores no citados (`Usuarios` se guardaría como `usuarios`). Entity Framework Core con el proveedor Npgsql genera automáticamente los identificadores citados (`"Usuarios"`, `"created_at"`, etc.), por lo que la convención PascalCase para tablas y snake_case para columnas **se mantiene sin cambios** en las migraciones generadas. Si se ejecuta SQL manual fuera de EF Core, los nombres de tabla deben ir entre comillas dobles (`"Usuarios"`) para respetar el casing.

> **Nota sobre ambientes:** a diferencia de SQL Server (una instancia con múltiples bases `ExchangePlatformDev/Test/Stg/Prod`), Supabase aísla por **proyecto completo** (cada ambiente es un proyecto Supabase separado, con su propia URL, credenciales y `connection string`). El aislamiento total entre ambientes se mantiene, pero a nivel de proyecto en vez de a nivel de base de datos dentro de una misma instancia.

---

## 5. Tablas Maestras

Las tablas maestras contienen datos de referencia estables. Usan `INT` como PK. No tienen Soft Delete (se desactivan con `is_active` cuando aplica).

### 5.1 Departamentos

```sql
CREATE TABLE Departamentos (
    id       INT          NOT NULL,
    ubigeo   CHAR(2)      NOT NULL,
    nombre   VARCHAR(100) NOT NULL,

    CONSTRAINT PK_Departamentos PRIMARY KEY (id),
    CONSTRAINT UX_Departamentos_Ubigeo UNIQUE (ubigeo)
);
```

| Columna | Tipo          | Nulo | Descripción                                 |
|---------|---------------|------|---------------------------------------------|
| id      | INT           | NO   | PK. Código numérico del departamento.       |
| ubigeo  | CHAR(2)       | NO   | Código UBIGEO oficial. Ej: "05" (Ayacucho). |
| nombre  | VARCHAR(100) | NO   | Nombre del departamento. Ej: "Ayacucho".    |

### 5.2 Provincias

```sql
CREATE TABLE Provincias (
    id               INT          NOT NULL,
    ubigeo           CHAR(4)      NOT NULL,
    nombre           VARCHAR(100) NOT NULL,
    departamento_id  INT          NOT NULL,

    CONSTRAINT PK_Provincias PRIMARY KEY (id),
    CONSTRAINT UX_Provincias_Ubigeo UNIQUE (ubigeo),
    CONSTRAINT FK_Provincias_Departamentos
        FOREIGN KEY (departamento_id) REFERENCES Departamentos(id)
        ON DELETE NO ACTION ON UPDATE NO ACTION
);
```

| Columna         | Tipo          | Nulo | Descripción                           |
|-----------------|---------------|------|---------------------------------------|
| id              | INT           | NO   | PK.                                   |
| ubigeo          | CHAR(4)       | NO   | Código UBIGEO. Ej: "0501" (Huamanga). |
| nombre          | VARCHAR(100) | NO   | Nombre de la provincia.               |
| departamento_id | INT           | NO   | FK → Departamentos.id.                |

### 5.3 Distritos

```sql
CREATE TABLE Distritos (
    id           INT          NOT NULL,
    ubigeo       CHAR(6)      NOT NULL,
    nombre       VARCHAR(100) NOT NULL,
    provincia_id INT          NOT NULL,

    CONSTRAINT PK_Distritos PRIMARY KEY (id),
    CONSTRAINT UX_Distritos_Ubigeo UNIQUE (ubigeo),
    CONSTRAINT FK_Distritos_Provincias
        FOREIGN KEY (provincia_id) REFERENCES Provincias(id)
        ON DELETE NO ACTION ON UPDATE NO ACTION
);
```

| Columna      | Tipo          | Nulo | Descripción                             |
|--------------|---------------|------|-----------------------------------------|
| id           | INT           | NO   | PK.                                     |
| ubigeo       | CHAR(6)       | NO   | Código UBIGEO. Ej: "050101" (Ayacucho). |
| nombre       | VARCHAR(100) | NO   | Nombre del distrito.                    |
| provincia_id | INT           | NO   | FK → Provincias.id.                     |

### 5.4 Roles

```sql
CREATE TABLE Roles (
    id          INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    nombre      VARCHAR(50)  NOT NULL,
    descripcion VARCHAR(200) NULL,

    CONSTRAINT PK_Roles PRIMARY KEY (id),
    CONSTRAINT UX_Roles_Nombre UNIQUE (nombre)
);
```

| Columna     | Tipo          | Nulo | Descripción                                        |
|-------------|---------------|------|----------------------------------------------------|
| id          | INT           | NO   | PK. Auto-incremental.                              |
| nombre      | VARCHAR(50)  | NO   | Nombre del rol: Administrador, Moderador, Usuario. |
| descripcion | VARCHAR(200) | SÍ   | Descripción del rol.                               |

**Datos iniciales:**

| id | nombre        | descripcion                       |
|----|---------------|-----------------------------------|
| 1  | Administrador | Acceso total al sistema.          |
| 2  | Moderador     | Gestión de reportes y contenidos. |
| 3  | Usuario       | Usuario estándar de la plataforma.|

### 5.5 Categorias

```sql
CREATE TABLE Categorias (
    id          INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    nombre      VARCHAR(100) NOT NULL,
    descripcion VARCHAR(500) NULL,
    icono       VARCHAR(100) NULL,
    is_active   BOOLEAN           NOT NULL DEFAULT TRUE,

    CONSTRAINT PK_Categorias PRIMARY KEY (id),
    CONSTRAINT UX_Categorias_Nombre UNIQUE (nombre)
);
```

| Columna     | Tipo          | Nulo | Descripción                                                |
|-------------|---------------|------|------------------------------------------------------------|
| id          | INT           | NO   | PK. Auto-incremental.                                      |
| nombre      | VARCHAR(100) | NO   | Nombre de la categoría. Único.                             |
| descripcion | VARCHAR(500) | SÍ   | Descripción de la categoría.                               |
| icono       | VARCHAR(100) | SÍ   | Nombre del ícono (ej. clase CSS o emoji).                  |
| is_active   | BOOLEAN           | NO   | TRUE = activa, FALSE = desactivada. Nunca se elimina físicamente. |

**Categorías iniciales:**

| id | nombre              | id | nombre              |
|----|---------------------|----|---------------------|
| 1  | Electrónica         | 6  | Deportes            |
| 2  | Ropa y Accesorios   | 7  | Herramientas        |
| 3  | Hogar y Muebles     | 8  | Vehículos y Partes  |
| 4  | Juguetes y Juegos   | 9  | Arte y Manualidades |
| 5  | Libros y Educación  | 10 | Otros               |

---
## 6. Tablas de Seguridad y Acceso

### 6.1 Usuarios

Tabla central del sistema. Almacena todos los datos de los usuarios registrados.

```sql
CREATE TABLE Usuarios (
    id                    UUID NOT NULL DEFAULT gen_random_uuid(),
    nombres               VARCHAR(100)    NOT NULL,
    apellidos             VARCHAR(100)    NOT NULL,
    email                 VARCHAR(256)    NOT NULL,
    password_hash         VARCHAR(500)    NOT NULL,
    telefono              VARCHAR(20)     NOT NULL,
    foto_perfil           VARCHAR(500)    NULL,
    rol_id                INT              NOT NULL DEFAULT 3,
    departamento_id       INT              NOT NULL,
    provincia_id          INT              NOT NULL,
    distrito_id           INT              NOT NULL,
    latitud               DECIMAL(10,7)    NULL,
    longitud              DECIMAL(10,7)    NULL,
    is_active             BOOLEAN              NOT NULL DEFAULT TRUE,
    calificacion_promedio DECIMAL(3,1)     NOT NULL DEFAULT 0.0,
    total_intercambios    INT              NOT NULL DEFAULT 0,
    failed_attempts       INT              NOT NULL DEFAULT 0,
    locked_until          TIMESTAMPTZ        NULL,
    created_at            TIMESTAMPTZ        NOT NULL DEFAULT now(),
    created_by            UUID NOT NULL,
    updated_at            TIMESTAMPTZ        NULL,
    updated_by            UUID NULL,
    is_deleted            BOOLEAN              NOT NULL DEFAULT FALSE,
    deleted_at            TIMESTAMPTZ        NULL,
    deleted_by            UUID NULL,

    CONSTRAINT PK_Usuarios PRIMARY KEY (id),
    CONSTRAINT UX_Usuarios_Email UNIQUE (email),
    CONSTRAINT FK_Usuarios_Roles FOREIGN KEY (rol_id) REFERENCES Roles(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT FK_Usuarios_Departamentos FOREIGN KEY (departamento_id) REFERENCES Departamentos(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT FK_Usuarios_Provincias FOREIGN KEY (provincia_id) REFERENCES Provincias(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT FK_Usuarios_Distritos FOREIGN KEY (distrito_id) REFERENCES Distritos(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT CK_Usuarios_CalificacionPromedio CHECK (calificacion_promedio >= 0.0 AND calificacion_promedio <= 5.0),
    CONSTRAINT CK_Usuarios_TotalIntercambios CHECK (total_intercambios >= 0),
    CONSTRAINT CK_Usuarios_FailedAttempts CHECK (failed_attempts >= 0 AND failed_attempts <= 10)
);
```

| Columna               | Tipo             | Nulo | Descripción                                          |
|-----------------------|------------------|------|------------------------------------------------------|
| id                    | UUID | NO   | PK. Generado con gen_random_uuid() (UUIDv4 aleatorio). |
| nombres               | VARCHAR(100)    | NO   | Nombres del usuario.                                 |
| apellidos             | VARCHAR(100)    | NO   | Apellidos del usuario.                               |
| email                 | VARCHAR(256)    | NO   | Correo único. Usado para login.                      |
| password_hash         | VARCHAR(500)    | NO   | Hash BCrypt (factor 12). Nunca en texto plano.       |
| telefono              | VARCHAR(20)     | NO   | Teléfono de contacto.                                |
| foto_perfil           | VARCHAR(500)    | SÍ   | URL de la foto de perfil.                            |
| rol_id                | INT              | NO   | FK -> Roles.id. Por defecto: 3 (Usuario).            |
| departamento_id       | INT              | NO   | FK -> Departamentos.id.                              |
| provincia_id          | INT              | NO   | FK -> Provincias.id.                                 |
| distrito_id           | INT              | NO   | FK -> Distritos.id.                                  |
| latitud               | DECIMAL(10,7)    | SÍ   | Coordenada geográfica opcional.                      |
| longitud              | DECIMAL(10,7)    | SÍ   | Coordenada geográfica opcional.                      |
| is_active             | BOOLEAN              | NO   | TRUE = activo, FALSE = desactivado por administrador.       |
| calificacion_promedio | DECIMAL(3,1)     | NO   | Promedio de calificaciones recibidas. Rango 0.0-5.0. |
| total_intercambios    | INT              | NO   | Contador de intercambios completados.                |
| failed_attempts       | INT              | NO   | Intentos fallidos consecutivos de login.             |
| locked_until          | TIMESTAMPTZ        | SÍ   | Fecha hasta la que la cuenta está bloqueada.         |
| created_at            | TIMESTAMPTZ        | NO   | Fecha de creación UTC.                               |
| created_by            | UUID | NO   | ID del usuario que creó el registro.                 |
| updated_at            | TIMESTAMPTZ        | SÍ   | Fecha de última modificación UTC.                    |
| updated_by            | UUID | SÍ   | ID del usuario que modificó el registro.             |
| is_deleted            | BOOLEAN              | NO   | Soft Delete. FALSE = activo, TRUE = eliminado.              |
| deleted_at            | TIMESTAMPTZ        | SÍ   | Fecha de eliminación lógica.                         |
| deleted_by            | UUID | SÍ   | ID del usuario que eliminó el registro.              |

### 6.2 RefreshTokens

```sql
CREATE TABLE RefreshTokens (
    id            UUID NOT NULL DEFAULT gen_random_uuid(),
    token         VARCHAR(500)    NOT NULL,
    usuario_id    UUID NOT NULL,
    expires_at    TIMESTAMPTZ        NOT NULL,
    is_revoked    BOOLEAN              NOT NULL DEFAULT FALSE,
    created_at    TIMESTAMPTZ        NOT NULL DEFAULT now(),
    created_by_ip VARCHAR(50)     NOT NULL,

    CONSTRAINT PK_RefreshTokens PRIMARY KEY (id),
    CONSTRAINT UX_RefreshTokens_Token UNIQUE (token),
    CONSTRAINT FK_RefreshTokens_Usuarios FOREIGN KEY (usuario_id) REFERENCES Usuarios(id) ON DELETE NO ACTION ON UPDATE NO ACTION
);
```

| Columna       | Tipo             | Nulo | Descripción                                 |
|---------------|------------------|------|---------------------------------------------|
| id            | UUID | NO   | PK.                                         |
| token         | VARCHAR(500)    | NO   | Token opaco UUID. Único.                    |
| usuario_id    | UUID | NO   | FK -> Usuarios.id.                          |
| expires_at    | TIMESTAMPTZ        | NO   | Fecha de expiración (7 días desde emisión). |
| is_revoked    | BOOLEAN              | NO   | TRUE = revocado (logout o rotación).           |
| created_at    | TIMESTAMPTZ        | NO   | Fecha de emisión.                           |
| created_by_ip | VARCHAR(50)     | NO   | IP desde donde se generó el token.          |

---

## 7. Tablas Transaccionales

Todas incluyen campos de auditoría y Soft Delete (salvo relaciones simples o append-only).

### 7.1 Objetos

```sql
CREATE TABLE Objetos (
    id               UUID NOT NULL DEFAULT gen_random_uuid(),
    titulo           VARCHAR(100)    NOT NULL,
    descripcion      VARCHAR(1000)   NOT NULL,
    categoria_id     INT              NOT NULL,
    usuario_id       UUID NOT NULL,
    estado_objeto    VARCHAR(20)     NOT NULL DEFAULT 'Disponible',
    condicion_fisica VARCHAR(20)     NOT NULL,
    departamento_id  INT              NOT NULL,
    provincia_id     INT              NOT NULL,
    distrito_id      INT              NOT NULL,
    latitud          DECIMAL(10,7)    NULL,
    longitud         DECIMAL(10,7)    NULL,
    created_at       TIMESTAMPTZ        NOT NULL DEFAULT now(),
    created_by       UUID NOT NULL,
    updated_at       TIMESTAMPTZ        NULL,
    updated_by       UUID NULL,
    is_deleted       BOOLEAN              NOT NULL DEFAULT FALSE,
    deleted_at       TIMESTAMPTZ        NULL,
    deleted_by       UUID NULL,

    CONSTRAINT PK_Objetos PRIMARY KEY (id),
    CONSTRAINT FK_Objetos_Categorias FOREIGN KEY (categoria_id) REFERENCES Categorias(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT FK_Objetos_Usuarios FOREIGN KEY (usuario_id) REFERENCES Usuarios(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT FK_Objetos_Departamentos FOREIGN KEY (departamento_id) REFERENCES Departamentos(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT FK_Objetos_Provincias FOREIGN KEY (provincia_id) REFERENCES Provincias(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT FK_Objetos_Distritos FOREIGN KEY (distrito_id) REFERENCES Distritos(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT CK_Objetos_EstadoObjeto CHECK (estado_objeto IN ('Disponible','Reservado','Intercambiado','Eliminado','Suspendido')),
    CONSTRAINT CK_Objetos_CondicionFisica CHECK (condicion_fisica IN ('Nuevo','Bueno','Regular')),
    CONSTRAINT CK_Objetos_Titulo CHECK (LENGTH(titulo) >= 5),
    CONSTRAINT CK_Objetos_Descripcion CHECK (LENGTH(descripcion) >= 20)
);
```

| Columna          | Tipo             | Nulo | Descripción                                                      |
|------------------|------------------|------|------------------------------------------------------------------|
| id               | UUID | NO   | PK.                                                              |
| titulo           | VARCHAR(100)    | NO   | Título del objeto. Mín. 5, máx. 100 chars.                       |
| descripcion      | VARCHAR(1000)   | NO   | Descripción. Mín. 20, máx. 1000 chars.                           |
| categoria_id     | INT              | NO   | FK -> Categorias.id. Categoría activa requerida.                 |
| usuario_id       | UUID | NO   | FK -> Usuarios.id. Propietario del objeto.                       |
| estado_objeto    | VARCHAR(20)     | NO   | Disponible / Reservado / Intercambiado / Eliminado / Suspendido. |
| condicion_fisica | VARCHAR(20)     | NO   | Nuevo / Bueno / Regular.                                         |
| departamento_id  | INT              | NO   | FK -> Departamentos.id.                                          |
| provincia_id     | INT              | NO   | FK -> Provincias.id.                                             |
| distrito_id      | INT              | NO   | FK -> Distritos.id.                                              |
| latitud          | DECIMAL(10,7)    | SÍ   | Coordenada opcional.                                             |
| longitud         | DECIMAL(10,7)    | SÍ   | Coordenada opcional.                                             |

### 7.2 ImagenesObjeto

```sql
CREATE TABLE ImagenesObjeto (
    id         UUID NOT NULL DEFAULT gen_random_uuid(),
    objeto_id  UUID NOT NULL,
    url        VARCHAR(500)    NOT NULL,
    orden      INT              NOT NULL DEFAULT 1,
    tamano_kb  INT              NOT NULL,

    CONSTRAINT PK_ImagenesObjeto PRIMARY KEY (id),
    CONSTRAINT FK_ImagenesObjeto_Objetos FOREIGN KEY (objeto_id) REFERENCES Objetos(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT CK_ImagenesObjeto_Orden CHECK (orden >= 1 AND orden <= 5),
    CONSTRAINT CK_ImagenesObjeto_Tamano CHECK (tamano_kb > 0 AND tamano_kb <= 5120)
);
```

| Columna   | Tipo             | Nulo | Descripción                                      |
|-----------|------------------|------|--------------------------------------------------|
| id        | UUID | NO   | PK.                                              |
| objeto_id | UUID | NO   | FK -> Objetos.id.                                |
| url       | VARCHAR(500)    | NO   | URL pública de la imagen. En Staging/Prod apunta al CDN de Supabase Storage; en Dev/Test, al disco local del backend (ADR-013). |
| orden     | INT              | NO   | Posición de la imagen (1-5). Imagen 1 = portada. |
| tamano_kb | INT              | NO   | Tamaño del archivo en KB. Máx. 5120 KB (5MB).    |

### 7.3 Intercambios

Tabla más crítica del sistema. Gestiona el ciclo completo de intercambio (trueque objeto-por-objeto).

```sql
CREATE TABLE Intercambios (
    id                       UUID NOT NULL DEFAULT gen_random_uuid(),
    objeto_solicitado_id     UUID NOT NULL,
    objeto_ofrecido_id       UUID NOT NULL,
    usuario_solicitante_id   UUID NOT NULL,
    usuario_propietario_id   UUID NOT NULL,
    estado_intercambio       VARCHAR(30)     NOT NULL DEFAULT 'Pendiente',
    mensaje_inicial          VARCHAR(500)    NULL,
    confirmacion_solicitante BOOLEAN              NOT NULL DEFAULT FALSE,
    confirmacion_propietario BOOLEAN              NOT NULL DEFAULT FALSE,
    fecha_aceptacion         TIMESTAMPTZ        NULL,
    fecha_completado         TIMESTAMPTZ        NULL,
    created_at               TIMESTAMPTZ        NOT NULL DEFAULT now(),
    created_by               UUID NOT NULL,
    updated_at               TIMESTAMPTZ        NULL,
    updated_by               UUID NULL,
    is_deleted               BOOLEAN              NOT NULL DEFAULT FALSE,
    deleted_at               TIMESTAMPTZ        NULL,
    deleted_by               UUID NULL,

    CONSTRAINT PK_Intercambios PRIMARY KEY (id),
    CONSTRAINT FK_Intercambios_ObjetoSolicitado FOREIGN KEY (objeto_solicitado_id) REFERENCES Objetos(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT FK_Intercambios_ObjetoOfrecido FOREIGN KEY (objeto_ofrecido_id) REFERENCES Objetos(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT FK_Intercambios_Solicitante FOREIGN KEY (usuario_solicitante_id) REFERENCES Usuarios(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT FK_Intercambios_Propietario FOREIGN KEY (usuario_propietario_id) REFERENCES Usuarios(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT CK_Intercambios_Estado CHECK (estado_intercambio IN ('Pendiente','Aceptado','PendienteConfirmacion','Completado','Rechazado','Cancelado')),
    CONSTRAINT CK_Intercambios_ObjetosDiferentes CHECK (objeto_solicitado_id <> objeto_ofrecido_id),
    CONSTRAINT CK_Intercambios_UsuariosDiferentes CHECK (usuario_solicitante_id <> usuario_propietario_id)
);
```

| Columna                  | Tipo             | Nulo | Descripción                                                 |
|--------------------------|------------------|------|-------------------------------------------------------------|
| id                       | UUID | NO   | PK.                                                         |
| objeto_solicitado_id     | UUID | NO   | FK -> Objetos.id. Objeto que el solicitante desea.          |
| objeto_ofrecido_id       | UUID | NO   | FK -> Objetos.id. Objeto que el solicitante ofrece.         |
| usuario_solicitante_id   | UUID | NO   | FK -> Usuarios.id. Quien envía la solicitud.                |
| usuario_propietario_id   | UUID | NO   | FK -> Usuarios.id. Dueño del objeto solicitado.             |
| estado_intercambio       | VARCHAR(30)     | NO   | Estado actual del ciclo de intercambio.                     |
| mensaje_inicial          | VARCHAR(500)    | SÍ   | Mensaje opcional del solicitante al crear la solicitud.     |
| confirmacion_solicitante | BOOLEAN              | NO   | TRUE = el solicitante confirmó haber recibido el objeto.       |
| confirmacion_propietario | BOOLEAN              | NO   | TRUE = el propietario confirmó haber recibido el objeto.       |
| fecha_aceptacion         | TIMESTAMPTZ        | SÍ   | Timestamp de cuando el propietario aceptó la solicitud.     |
| fecha_completado         | TIMESTAMPTZ        | SÍ   | Timestamp de cuando ambos confirmaron el intercambio.       |

### 7.4 Calificaciones

```sql
CREATE TABLE Calificaciones (
    id             UUID NOT NULL DEFAULT gen_random_uuid(),
    intercambio_id UUID NOT NULL,
    calificador_id UUID NOT NULL,
    calificado_id  UUID NOT NULL,
    puntuacion     INT              NOT NULL,
    comentario     VARCHAR(500)    NULL,
    created_at     TIMESTAMPTZ        NOT NULL DEFAULT now(),
    created_by     UUID NOT NULL,

    CONSTRAINT PK_Calificaciones PRIMARY KEY (id),
    CONSTRAINT FK_Calificaciones_Intercambios FOREIGN KEY (intercambio_id) REFERENCES Intercambios(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT FK_Calificaciones_Calificador FOREIGN KEY (calificador_id) REFERENCES Usuarios(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT FK_Calificaciones_Calificado FOREIGN KEY (calificado_id) REFERENCES Usuarios(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT CK_Calificaciones_Puntuacion CHECK (puntuacion >= 1 AND puntuacion <= 5),
    CONSTRAINT CK_Calificaciones_UsuariosDiferentes CHECK (calificador_id <> calificado_id),
    CONSTRAINT UX_Calificaciones_UnaXIntercambio UNIQUE (intercambio_id, calificador_id)
);
```

| Columna        | Tipo             | Nulo | Descripción                                     |
|----------------|------------------|------|-------------------------------------------------|
| id             | UUID | NO   | PK.                                             |
| intercambio_id | UUID | NO   | FK -> Intercambios.id.                          |
| calificador_id | UUID | NO   | FK -> Usuarios.id. Quien emite la calificación. |
| calificado_id  | UUID | NO   | FK -> Usuarios.id. Quien recibe la calificación.|
| puntuacion     | INT              | NO   | Valor entre 1 y 5 (constraint CHECK).           |
| comentario     | VARCHAR(500)    | SÍ   | Comentario opcional. Máx. 500 chars.            |
| created_at     | TIMESTAMPTZ        | NO   | Fecha de emisión.                               |
| created_by     | UUID | NO   | ID del calificador (auditoría directa).         |

**Constraint clave:** `UX_Calificaciones_UnaXIntercambio` garantiza una sola calificación por usuario por intercambio (RN-030).

### 7.5 Mensajes

```sql
CREATE TABLE Mensajes (
    id             UUID NOT NULL DEFAULT gen_random_uuid(),
    intercambio_id UUID NOT NULL,
    remitente_id   UUID NOT NULL,
    contenido      VARCHAR(1000)   NOT NULL,
    enviado_en     TIMESTAMPTZ        NOT NULL DEFAULT now(),
    is_leido       BOOLEAN              NOT NULL DEFAULT FALSE,

    CONSTRAINT PK_Mensajes PRIMARY KEY (id),
    CONSTRAINT FK_Mensajes_Intercambios FOREIGN KEY (intercambio_id) REFERENCES Intercambios(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT FK_Mensajes_Remitente FOREIGN KEY (remitente_id) REFERENCES Usuarios(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT CK_Mensajes_Contenido CHECK (LENGTH(TRIM(contenido)) > 0 AND LENGTH(contenido) <= 1000)
);
```

| Columna        | Tipo             | Nulo | Descripción                                   |
|----------------|------------------|------|-----------------------------------------------|
| id             | UUID | NO   | PK.                                           |
| intercambio_id | UUID | NO   | FK -> Intercambios.id. Contexto del mensaje.  |
| remitente_id   | UUID | NO   | FK -> Usuarios.id. Quien envía el mensaje.    |
| contenido      | VARCHAR(1000)   | NO   | Texto del mensaje. Máx. 1000 chars. No vacío. |
| enviado_en     | TIMESTAMPTZ        | NO   | Timestamp de envío UTC.                       |
| is_leido       | BOOLEAN              | NO   | FALSE = no leído, TRUE = leído por el destinatario.  |

### 7.6 Notificaciones

```sql
CREATE TABLE Notificaciones (
    id           UUID NOT NULL DEFAULT gen_random_uuid(),
    usuario_id   UUID NOT NULL,
    tipo         VARCHAR(50)     NOT NULL,
    titulo       VARCHAR(200)    NOT NULL,
    mensaje      VARCHAR(500)    NOT NULL,
    is_leida     BOOLEAN              NOT NULL DEFAULT FALSE,
    entidad_tipo VARCHAR(50)     NULL,
    entidad_id   UUID NULL,
    creada_en    TIMESTAMPTZ        NOT NULL DEFAULT now(),

    CONSTRAINT PK_Notificaciones PRIMARY KEY (id),
    CONSTRAINT FK_Notificaciones_Usuarios FOREIGN KEY (usuario_id) REFERENCES Usuarios(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT CK_Notificaciones_Tipo CHECK (tipo IN ('SolicitudRecibida','SolicitudAceptada','SolicitudRechazada','IntercambioCompletado','NuevoMensaje','NuevaCalificacion','Sistema'))
);
```

| Columna      | Tipo             | Nulo | Descripción                                            |
|--------------|------------------|------|--------------------------------------------------------|
| id           | UUID | NO   | PK.                                                    |
| usuario_id   | UUID | NO   | FK -> Usuarios.id. Destinatario.                       |
| tipo         | VARCHAR(50)     | NO   | Tipo de notificación (enum controlado por CHECK).      |
| titulo       | VARCHAR(200)    | NO   | Título breve de la notificación.                       |
| mensaje      | VARCHAR(500)    | NO   | Cuerpo de la notificación.                             |
| is_leida     | BOOLEAN              | NO   | FALSE = no leída, TRUE = leída. Nunca se elimina físicamente. |
| entidad_tipo | VARCHAR(50)     | SÍ   | Tipo de entidad relacionada. Ej: "Intercambio".        |
| entidad_id   | UUID | SÍ   | ID de la entidad relacionada para navegación directa.  |
| creada_en    | TIMESTAMPTZ        | NO   | Timestamp de creación UTC.                             |

### 7.7 Favoritos

```sql
CREATE TABLE Favoritos (
    id          UUID NOT NULL DEFAULT gen_random_uuid(),
    usuario_id  UUID NOT NULL,
    objeto_id   UUID NOT NULL,
    agregado_en TIMESTAMPTZ        NOT NULL DEFAULT now(),

    CONSTRAINT PK_Favoritos PRIMARY KEY (id),
    CONSTRAINT UX_Favoritos_UsuarioObjeto UNIQUE (usuario_id, objeto_id),
    CONSTRAINT FK_Favoritos_Usuarios FOREIGN KEY (usuario_id) REFERENCES Usuarios(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT FK_Favoritos_Objetos FOREIGN KEY (objeto_id) REFERENCES Objetos(id) ON DELETE NO ACTION ON UPDATE NO ACTION
);
```

| Columna     | Tipo             | Nulo | Descripción                               |
|-------------|------------------|------|-------------------------------------------|
| id          | UUID | NO   | PK.                                       |
| usuario_id  | UUID | NO   | FK -> Usuarios.id.                        |
| objeto_id   | UUID | NO   | FK -> Objetos.id.                         |
| agregado_en | TIMESTAMPTZ        | NO   | Timestamp en que se guardó como favorito. |

**Constraint clave:** `UX_Favoritos_UsuarioObjeto` previene favoritos duplicados (RN-042).

### 7.8 Reportes

```sql
CREATE TABLE Reportes (
    id               UUID NOT NULL DEFAULT gen_random_uuid(),
    reportante_id    UUID NOT NULL,
    entidad_tipo     VARCHAR(20)     NOT NULL,
    entidad_id       UUID NOT NULL,
    motivo           VARCHAR(100)    NOT NULL,
    descripcion      VARCHAR(500)    NULL,
    estado_reporte   VARCHAR(20)     NOT NULL DEFAULT 'Pendiente',
    resuelto_por_id  UUID NULL,
    fecha_resolucion TIMESTAMPTZ        NULL,
    created_at       TIMESTAMPTZ        NOT NULL DEFAULT now(),
    created_by       UUID NOT NULL,
    updated_at       TIMESTAMPTZ        NULL,
    updated_by       UUID NULL,
    is_deleted       BOOLEAN              NOT NULL DEFAULT FALSE,
    deleted_at       TIMESTAMPTZ        NULL,
    deleted_by       UUID NULL,

    CONSTRAINT PK_Reportes PRIMARY KEY (id),
    CONSTRAINT FK_Reportes_Reportante FOREIGN KEY (reportante_id) REFERENCES Usuarios(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT FK_Reportes_Resuelto FOREIGN KEY (resuelto_por_id) REFERENCES Usuarios(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT CK_Reportes_EntidadTipo CHECK (entidad_tipo IN ('Objeto','Usuario')),
    CONSTRAINT CK_Reportes_EstadoReporte CHECK (estado_reporte IN ('Pendiente','EnRevision','Resuelto','Descartado')),
    CONSTRAINT CK_Reportes_Motivo CHECK (motivo IN ('ContenidoInapropiado','Fraude','Spam','InformacionFalsa','Otro'))
);
```

| Columna          | Tipo             | Nulo | Descripción                                     |
|------------------|------------------|------|-------------------------------------------------|
| id               | UUID | NO   | PK.                                             |
| reportante_id    | UUID | NO   | FK -> Usuarios.id. Quien realiza el reporte.    |
| entidad_tipo     | VARCHAR(20)     | NO   | "Objeto" o "Usuario".                           |
| entidad_id       | UUID | NO   | ID del objeto o usuario reportado.              |
| motivo           | VARCHAR(100)    | NO   | Motivo del reporte (controlado por CHECK).      |
| descripcion      | VARCHAR(500)    | SÍ   | Detalle adicional del reporte.                  |
| estado_reporte   | VARCHAR(20)     | NO   | Pendiente / EnRevision / Resuelto / Descartado. |
| resuelto_por_id  | UUID | SÍ   | FK -> Usuarios.id. Moderador/Admin que resolvió.|
| fecha_resolucion | TIMESTAMPTZ        | SÍ   | Fecha de resolución del reporte.                |

---

## 8. Tabla de Auditoría

### 8.1 AuditLogs

Tabla de solo inserción (append-only). Sin UPDATE ni DELETE permitidos (RN-063).

```sql
CREATE TABLE AuditLogs (
    id           UUID NOT NULL DEFAULT gen_random_uuid(),
    usuario_id   UUID NULL,
    accion       VARCHAR(100)    NOT NULL,
    entidad_tipo VARCHAR(100)    NOT NULL,
    entidad_id   VARCHAR(100)    NULL,
    detalle      TEXT    NULL,
    resultado    VARCHAR(20)     NOT NULL,
    ip_address   VARCHAR(50)     NOT NULL,
    ocurrido_en  TIMESTAMPTZ        NOT NULL DEFAULT now(),

    CONSTRAINT PK_AuditLogs PRIMARY KEY (id),
    CONSTRAINT CK_AuditLogs_Resultado CHECK (resultado IN ('Exitoso','Fallido','Parcial'))
);
```

| Columna      | Tipo             | Nulo | Descripción                                                     |
|--------------|------------------|------|-----------------------------------------------------------------|
| id           | UUID | NO   | PK.                                                             |
| usuario_id   | UUID | SÍ   | ID del usuario que ejecutó la acción. Null en errores pre-auth. |
| accion       | VARCHAR(100)    | NO   | Nombre del evento. Ej: "Login_Exitoso", "Objeto_Publicado".     |
| entidad_tipo | VARCHAR(100)    | NO   | Tipo de entidad afectada. Ej: "Usuario", "Objeto".              |
| entidad_id   | VARCHAR(100)    | SÍ   | ID de la entidad afectada (string para flexibilidad).           |
| detalle      | TEXT    | SÍ   | JSON con contexto adicional. Sin datos sensibles.               |
| resultado    | VARCHAR(20)     | NO   | Exitoso / Fallido / Parcial.                                    |
| ip_address   | VARCHAR(50)     | NO   | IP del solicitante.                                             |
| ocurrido_en  | TIMESTAMPTZ        | NO   | Timestamp UTC del evento.                                       |

**Nota:** Sin campos updated_at ni is_deleted. Es append-only por diseño.

---

## 9. Índices

Diseñados sobre las columnas más usadas en WHERE, JOIN y ORDER BY. Índices filtrados (`WHERE is_deleted = FALSE`) en tablas con Soft Delete.

### 9.1 Usuarios

```sql
CREATE UNIQUE INDEX UX_Usuarios_Email ON Usuarios(email) WHERE is_deleted = FALSE;
CREATE INDEX IX_Usuarios_RolId ON Usuarios(rol_id) WHERE is_deleted = FALSE;
CREATE INDEX IX_Usuarios_DistritoId ON Usuarios(distrito_id) WHERE is_deleted = FALSE;
CREATE INDEX IX_Usuarios_IsActive_IsDeleted ON Usuarios(is_active, is_deleted);
```

### 9.2 Objetos

```sql
CREATE INDEX IX_Objetos_UsuarioId ON Objetos(usuario_id) WHERE is_deleted = FALSE;
CREATE INDEX IX_Objetos_CategoriaId ON Objetos(categoria_id) WHERE is_deleted = FALSE;
CREATE INDEX IX_Objetos_EstadoObjeto ON Objetos(estado_objeto) WHERE is_deleted = FALSE;
CREATE INDEX IX_Objetos_DistritoId ON Objetos(distrito_id) WHERE is_deleted = FALSE;
CREATE INDEX IX_Objetos_ProvinciaId ON Objetos(provincia_id) WHERE is_deleted = FALSE;
CREATE INDEX IX_Objetos_DepartamentoId ON Objetos(departamento_id) WHERE is_deleted = FALSE;
CREATE INDEX IX_Objetos_Busqueda ON Objetos(estado_objeto, categoria_id, distrito_id, created_at DESC) WHERE is_deleted = FALSE;
CREATE INDEX IX_Objetos_Titulo ON Objetos(titulo) WHERE is_deleted = FALSE;
```

### 9.3 Intercambios

```sql
CREATE INDEX IX_Intercambios_ObjetoSolicitado ON Intercambios(objeto_solicitado_id) WHERE is_deleted = FALSE;
CREATE INDEX IX_Intercambios_ObjetoOfrecido ON Intercambios(objeto_ofrecido_id) WHERE is_deleted = FALSE;
CREATE INDEX IX_Intercambios_Solicitante ON Intercambios(usuario_solicitante_id) WHERE is_deleted = FALSE;
CREATE INDEX IX_Intercambios_Propietario ON Intercambios(usuario_propietario_id) WHERE is_deleted = FALSE;
CREATE INDEX IX_Intercambios_Estado ON Intercambios(estado_intercambio) WHERE is_deleted = FALSE;
CREATE INDEX IX_Intercambios_ActiveRequest ON Intercambios(usuario_solicitante_id, objeto_solicitado_id, estado_intercambio) WHERE is_deleted = FALSE;
```

### 9.4 Otras Tablas

```sql
CREATE INDEX IX_Calificaciones_Calificado ON Calificaciones(calificado_id);
CREATE INDEX IX_Calificaciones_IntercambioId ON Calificaciones(intercambio_id);
CREATE INDEX IX_Mensajes_IntercambioId ON Mensajes(intercambio_id);
CREATE INDEX IX_Mensajes_RemitenteId ON Mensajes(remitente_id);
CREATE INDEX IX_Notificaciones_UsuarioId_IsLeida ON Notificaciones(usuario_id, is_leida);
CREATE INDEX IX_Favoritos_UsuarioId ON Favoritos(usuario_id);
CREATE INDEX IX_Reportes_EstadoReporte ON Reportes(estado_reporte) WHERE is_deleted = FALSE;
CREATE INDEX IX_RefreshTokens_UsuarioId ON RefreshTokens(usuario_id);
CREATE INDEX IX_RefreshTokens_ExpiresAt ON RefreshTokens(expires_at);
CREATE INDEX IX_AuditLogs_UsuarioId ON AuditLogs(usuario_id);
CREATE INDEX IX_AuditLogs_OcurridoEn ON AuditLogs(ocurrido_en DESC);
CREATE INDEX IX_AuditLogs_Accion ON AuditLogs(accion);
```

---

## 10. Diccionario de Datos Resumido

| Tabla          | Filas estimadas MVP  | Tipo          | Soft Delete     | Auditoría        |
|----------------|----------------------|---------------|-----------------|------------------|
| Departamentos  | 25                   | Maestra       | NO              | NO               |
| Provincias     | 196                  | Maestra       | NO              | NO               |
| Distritos      | 1,874                | Maestra       | NO              | NO               |
| Roles          | 3                    | Maestra       | NO              | NO               |
| Categorias     | 10+                  | Maestra       | NO (is_active)  | NO               |
| Usuarios       | 100-1,000            | Seguridad     | SÍ              | SÍ               |
| RefreshTokens  | ~3 por usuario       | Seguridad     | NO (is_revoked) | NO               |
| Objetos        | 50-500               | Transaccional | SÍ              | SÍ               |
| ImagenesObjeto | 1-5 por objeto       | Transaccional | NO              | NO               |
| Intercambios   | 20-200               | Transaccional | SÍ              | SÍ               |
| Calificaciones | ~2 por intercambio   | Transaccional | NO              | Parcial          |
| Mensajes       | 5-50 por intercambio | Transaccional | NO              | NO               |
| Notificaciones | 3-10 por intercambio | Transaccional | NO              | NO               |
| Favoritos      | Variable             | Transaccional | NO              | NO               |
| Reportes       | Variable             | Transaccional | SÍ              | SÍ               |
| AuditLogs      | Alto volumen         | Auditoría     | NO              | NO (append-only) |

---

## 11. Scripts de Datos Iniciales (Seed)

Cargar al iniciar la aplicación por primera vez mediante EF Core Data Seeding o scripts SQL de migración.

### 11.1 Orden de Carga

```
1. Departamentos     (25 registros - UBIGEO oficial del Perú)
2. Provincias        (196 registros)
3. Distritos         (1,874 registros)
4. Roles             (3 registros)
5. Categorias        (10 registros iniciales)
6. Usuario Admin     (1 usuario administrador por defecto)
```

### 11.2 Usuario Administrador por Defecto

```sql
-- SOLO para ambiente Development. Cambiar credenciales en Staging/Production.
INSERT INTO Usuarios (
    id, nombres, apellidos, email, password_hash, telefono,
    rol_id, departamento_id, provincia_id, distrito_id,
    is_active, calificacion_promedio, total_intercambios,
    failed_attempts, created_at, created_by, is_deleted
) VALUES (
    gen_random_uuid(),
    'Admin', 'Sistema',
    'admin@exchange.pe',
    '$2a$12$examplehashgeneratedbyapplication',  -- BCrypt - CAMBIAR EN PRODUCCIÓN
    '000000000',
    1, 5, 501, 50101,
    TRUE, 0.0, 0, 0,
    now(),
    '00000000-0000-0000-0000-000000000000',
    FALSE
);
```

---

## 12. Estrategia de Backup

Supabase gestiona el motor PostgreSQL como servicio administrado; los backups automáticos dependen del plan contratado. Se complementan con backups manuales (`pg_dump`) como respaldo adicional bajo control del equipo.

| Tipo                          | Frecuencia                              | Retención                    | Herramienta                                  |
|-------------------------------|------------------------------------------|-------------------------------|-----------------------------------------------|
| Backup automático (Supabase)  | Diario (según plan)                      | 7 días (Free/Pro) — configurable en planes superiores | Backups gestionados de Supabase.              |
| Point-in-Time Recovery (PITR) | Continuo (solo planes con PITR habilitado) | Según plan contratado         | Supabase PITR (WAL continuo).                 |
| Backup manual complementario  | Semanal                                  | 4 semanas, almacenado fuera de Supabase (ej. bucket externo) | `pg_dump` desde CI/CD o script programado.    |

> **Nota:** el plan Free de Supabase no incluye PITR ni backups descargables sin límite; para Producción real se recomienda evaluar el plan Pro. Documentar la decisión final como ADR si se contrata un plan de pago.

### 12.1 Comando de Backup Manual (pg_dump)

```bash
# Backup completo vía pg_dump usando el connection string del pooler de Supabase
pg_dump "postgresql://postgres.[ref]:[password]@aws-0-[region].pooler.supabase.com:5432/postgres" \
  --format=custom \
  --file="backup_$(date +%Y%m%d).dump"

# Restauración
pg_restore --clean --if-exists \
  -d "postgresql://postgres.[ref]:[password]@aws-0-[region].pooler.supabase.com:5432/postgres" \
  backup_YYYYMMDD.dump
```

> El `connection string` real (con `[ref]`, `[password]`, `[region]`) se obtiene desde el dashboard de Supabase → Project Settings → Database, y se inyecta como secreto (GitHub Secrets), nunca hardcodeado.

### 12.2 Tiempo Objetivo de Recuperación (RTO)

| Escenario                          | RTO Objetivo |
|-------------------------------------|--------------|
| Restaurar backup automático Supabase | < 2 horas    |
| Restaurar vía PITR (si el plan lo permite) | < 30 minutos |
| Restaurar desde `pg_dump` manual     | < 4 horas    |

---

## 13. Consideraciones de Rendimiento

| Consideración             | Implementación                                                        |
|---------------------------|-----------------------------------------------------------------------|
| Paginación obligatoria    | Todos los queries de lista usan LIMIT/OFFSET (EF Core con Npgsql traduce `.Skip()/.Take()` a `LIMIT/OFFSET`, no a `OFFSET/FETCH`). Máx. 50 registros. |
| Filtro Soft Delete global | EF Core Query Filter. Sin registros eliminados en queries normales.   |
| PK aleatoria (gen_random_uuid()) | A diferencia de `NEWSEQUENTIALID()` en SQL Server, `gen_random_uuid()` genera UUIDv4 **aleatorios**, no secuenciales — puede causar más fragmentación en el índice B-tree del PK bajo alta escritura. Aceptable para el volumen del MVP (tabla 10, cientos de filas); si el volumen crece significativamente, evaluar UUIDv7 (orden temporal) o un `BIGINT GENERATED ALWAYS AS IDENTITY` como PK con UUID como columna secundaria expuesta en la API. |
| Índices filtrados         | WHERE is_deleted = FALSE en índices de tablas con Soft Delete.            |
| Proyecciones              | Queries usan SELECT de columnas específicas. Sin SELECT *.            |
| Eager Loading controlado  | EF Core Include() solo cuando se necesita. Sin over-fetching.         |
| Calificación promedio     | Campo precalculado en Usuarios. Evita AVG() en cada request de perfil.|
| Full-Text Search (futuro) | Columna titulo en Objetos preparada para FTS en versiones futuras.    |

---

## 14. Trazabilidad (BD -> Reglas/Requisitos)

| Elemento de BD                              | Responde a              |
|---------------------------------------------|-------------------------|
| Soft delete + columnas de auditoría         | RN-013, RN-062          |
| AuditLogs append-only                       | RN-063, RF-120, RF-121  |
| Geografía Departamento/Provincia/Distrito   | RN-050, RN-051, RNF-021 |
| UX_Usuarios_Email                           | RN-002                  |
| CK_Calificaciones_Puntuacion (1-5)          | RN-031                  |
| UX_Calificaciones_UnaXIntercambio           | RN-030                  |
| CK_Intercambios_UsuariosDiferentes          | RN-022                  |
| confirmacion_solicitante/propietario        | RN-020                  |
| FK Usuarios/Objetos (identidad no anónima)  | RN-021, RN-003          |
| UX_Favoritos_UsuarioObjeto                  | RN-042                  |
| RefreshTokens (expires_at, is_revoked)      | RN-064                  |
| password_hash (BCrypt)                      | RN-061, RNF-001         |
| Índices de búsqueda y paginación            | RNF-011                 |
| CK_Objetos_EstadoObjeto                     | RN-014                  |

---

## 15. Aprobación

| Rol                        | Nombre            | Aprobación  | Fecha |
|----------------------------|-------------------|-------------|-------|
| Product Owner              | —                 | ☐ PENDIENTE | —     |
| Arquitecto de Datos Senior | Equipo Enterprise | ☐ PENDIENTE | —     |
| DBA Senior                 | Equipo Enterprise | ☐ PENDIENTE | —     |
| Arquitecto de Software     | Equipo Enterprise | ☐ PENDIENTE | —     |

---

> **GATE DE CALIDAD — FASE 2, PASO 7:**
> Este documento debe ser revisado y **formalmente aprobado** antes de iniciar el Paso 8 (`API.md`).
> El modelo de base de datos aquí definido es la fuente oficial para las migraciones de EF Core
> y cualquier script SQL del proyecto. Ninguna tabla o columna puede crearse sin estar
> documentada y aprobada en este documento.

---

*Documento generado bajo la metodología SDD — Plataforma Inteligente de Intercambio de Objetos — Ayacucho, Perú.*
