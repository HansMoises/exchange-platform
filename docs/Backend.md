# Backend.md
# Plataforma Inteligente de Intercambio de Objetos

> **Documento:** Guía de Implementación del Backend
> **Fase SDLC:** 3 (Desarrollo / Implementación)
> **Versión:** 1.2.0
> **Estado:** `PENDIENTE DE APROBACIÓN`
> **Fecha:** 2026-07-14
> **Autor:** Equipo Enterprise Senior (Backend Developer Senior / Arquitecto / Esp. EF Core)
> **Documentos padre:** Arquitectura.md | BD.md | API.md | Seguridad.md | Convenciones.md | UML.md
> **Convenciones:** Documentación y código en español. Diagramas en ASCII. Stack: .NET 10.

---

## Control de Versiones

| Versión | Fecha      | Autor                    | Cambios          |
|---------|------------|--------------------------|------------------|
| 1.0.0   | 2026-06-03 | Equipo Enterprise Senior | Versión inicial. |
| 1.1.0   | 2026-07-08 | Equipo Enterprise Senior | BD migrada de SQL Server a PostgreSQL/Supabase (proveedor Npgsql). Ver ADR-010 en Arquitectura.md. |
| 1.2.0   | 2026-07-14 | Equipo Enterprise Senior | Nueva §5.4 — almacenamiento de imágenes tras `IAlmacenamientoService` con dos implementaciones (`AlmacenamientoLocalService` para Dev/Test, `AlmacenamientoSupabaseService` para Staging/Prod). Corrige la referencia genérica al almacenamiento. Ver ADR-013 en Arquitectura.md. |

---

## Tabla de Contenidos

1. Alcance y Stack
2. Estructura de la Solución
3. Capa Domain
4. Capa Application
5. Capa Infrastructure
6. Capa API
7. Configuración Transversal
8. Flujo Completo de un Caso de Uso (ejemplo)
9. Orden de Implementación
10. Mapeo Módulo → Componentes
11. Aprobación

---

## 1. Alcance y Stack

Este documento guía la implementación del backend según el diseño aprobado. No redefine la arquitectura ni la BD; explica **cómo** construirlas en .NET.

| Componente        | Tecnología                                  |
|-------------------|---------------------------------------------|
| Framework         | .NET 10 · ASP.NET Core Web API              |
| ORM               | Entity Framework Core (Code First) + Npgsql |
| Base de datos     | PostgreSQL 15+ (Supabase, gestionado)       |
| CQRS / Mediación  | MediatR                                     |
| Mapeo             | AutoMapper                                  |
| Validación        | FluentValidation                            |
| Logging           | Serilog                                     |
| Documentación API | Swagger / OpenAPI                           |
| Autenticación     | JWT + Refresh Tokens                        |
| Pruebas           | xUnit · Moq · FluentAssertions              |

---

## 2. Estructura de la Solución

Cuatro proyectos + pruebas (detalle en Arquitectura.md §3.1).

```
ExchangePlatform.sln
├── ExchangePlatform.Domain          (núcleo, sin dependencias)
├── ExchangePlatform.Application      (depende de Domain)
├── ExchangePlatform.Infrastructure   (depende de Domain; implementa interfaces)
└── ExchangePlatform.API              (depende de Application; compone todo)
```

Referencias entre proyectos:

```
API ───────────► Application ───────────► Domain
 │                                          ▲
 └──────► Infrastructure ───────────────────┘
          (implementa interfaces de Domain)
```

---

## 3. Capa Domain

Contiene las entidades, value objects, enums, eventos de dominio, reglas/invariantes e interfaces de repositorio. **Sin dependencias de frameworks.**

### 3.1 Entidad Base

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    public void MarcarEliminado(Guid usuarioId)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = usuarioId;
    }
}
```

### 3.2 Entidad con Reglas de Dominio (ejemplo Intercambio)

La lógica de negocio vive en la entidad (modelo rico, no anémico). Implementa RN-020 a RN-026.

```csharp
public class Intercambio : BaseEntity
{
    public Guid ObjetoSolicitadoId { get; private set; }
    public Guid ObjetoOfrecidoId { get; private set; }
    public Guid UsuarioSolicitanteId { get; private set; }
    public Guid UsuarioPropietarioId { get; private set; }
    public EstadoIntercambio Estado { get; private set; }
    public bool ConfirmacionSolicitante { get; private set; }
    public bool ConfirmacionPropietario { get; private set; }
    public DateTime? FechaAceptacion { get; private set; }
    public DateTime? FechaCompletado { get; private set; }

    // RN-022: no se puede solicitar el propio objeto (se valida también en Application)
    public Intercambio(Guid objetoSolicitadoId, Guid objetoOfrecidoId,
                       Guid solicitanteId, Guid propietarioId)
    {
        if (solicitanteId == propietarioId)
            throw new DomainException("No puedes intercambiar contigo mismo.");
        ObjetoSolicitadoId = objetoSolicitadoId;
        ObjetoOfrecidoId = objetoOfrecidoId;
        UsuarioSolicitanteId = solicitanteId;
        UsuarioPropietarioId = propietarioId;
        Estado = EstadoIntercambio.Pendiente;
    }

    // RN-023: solo el propietario acepta (el quién se valida en Application)
    public void Aceptar()
    {
        if (Estado != EstadoIntercambio.Pendiente)
            throw new DomainException("Solo se puede aceptar una solicitud pendiente.");
        Estado = EstadoIntercambio.Aceptado;
        FechaAceptacion = DateTime.UtcNow;
    }

    // RN-024: una rechazada no se reabre
    public void Rechazar()
    {
        if (Estado != EstadoIntercambio.Pendiente)
            throw new DomainException("Solo se puede rechazar una solicitud pendiente.");
        Estado = EstadoIntercambio.Rechazado;
    }

    // RN-020: ambas confirmaciones completan el intercambio
    public void Confirmar(Guid usuarioId)
    {
        if (Estado != EstadoIntercambio.Aceptado &&
            Estado != EstadoIntercambio.PendienteConfirmacion)
            throw new DomainException("El intercambio no está en estado confirmable.");

        if (usuarioId == UsuarioSolicitanteId) ConfirmacionSolicitante = true;
        else if (usuarioId == UsuarioPropietarioId) ConfirmacionPropietario = true;
        else throw new DomainException("Usuario ajeno al intercambio.");

        Estado = (ConfirmacionSolicitante && ConfirmacionPropietario)
            ? EstadoIntercambio.Completado
            : EstadoIntercambio.PendienteConfirmacion;

        if (Estado == EstadoIntercambio.Completado)
            FechaCompletado = DateTime.UtcNow;
    }
}
```

### 3.3 Interfaces de Repositorio

```csharp
public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void SoftDelete(T entity, Guid usuarioId);
}

public interface IIntercambioRepository : IGenericRepository<Intercambio>
{
    Task<bool> TieneSolicitudActivaAsync(Guid solicitanteId, Guid objetoSolicitadoId);
}

public interface IUnitOfWork
{
    IUsuarioRepository Usuarios { get; }
    IObjetoRepository Objetos { get; }
    IIntercambioRepository Intercambios { get; }
    // ...otros repositorios
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

---

## 4. Capa Application

Commands, Queries, Handlers, Validators, DTOs, Behaviors y Mappings. Orquesta el dominio; **sin acceso directo a la BD** (usa interfaces).

### 4.1 Command + Handler + Validator (ejemplo)

```csharp
// Command
public record CrearIntercambioCommand(
    Guid ObjetoSolicitadoId,
    Guid ObjetoOfrecidoId,
    Guid SolicitanteId,
    string? MensajeInicial) : IRequest<Guid>;

// Validator (FluentValidation)
public class CrearIntercambioCommandValidator
    : AbstractValidator<CrearIntercambioCommand>
{
    public CrearIntercambioCommandValidator()
    {
        RuleFor(x => x.ObjetoSolicitadoId).NotEmpty();
        RuleFor(x => x.ObjetoOfrecidoId).NotEmpty()
            .NotEqual(x => x.ObjetoSolicitadoId)
            .WithMessage("Los objetos deben ser distintos.");
        RuleFor(x => x.MensajeInicial).MaximumLength(500);
    }
}

// Handler
public class CrearIntercambioCommandHandler
    : IRequestHandler<CrearIntercambioCommand, Guid>
{
    private readonly IUnitOfWork _uow;

    public CrearIntercambioCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Guid> Handle(CrearIntercambioCommand request, CancellationToken ct)
    {
        var objetoSolicitado = await _uow.Objetos.GetByIdAsync(request.ObjetoSolicitadoId)
            ?? throw new NotFoundException("Objeto solicitado no encontrado.");

        // RN-022: no solicitar objeto propio
        if (objetoSolicitado.UsuarioId == request.SolicitanteId)
            throw new ConflictException("No puedes solicitar tu propio objeto.");

        // RN-014: el objeto debe estar disponible
        if (objetoSolicitado.Estado != EstadoObjeto.Disponible)
            throw new ConflictException("El objeto no está disponible.");

        var intercambio = new Intercambio(
            request.ObjetoSolicitadoId, request.ObjetoOfrecidoId,
            request.SolicitanteId, objetoSolicitado.UsuarioId);

        await _uow.Intercambios.AddAsync(intercambio);
        await _uow.SaveChangesAsync(ct);
        return intercambio.Id;
    }
}
```

### 4.2 Pipeline Behaviors (cross-cutting)

```csharp
// ValidationBehavior: ejecuta los validators antes del handler
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> v) => _validators = v;

    public async Task<TResponse> Handle(TRequest request,
        RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        // valida y lanza ValidationException si hay errores
        // ...
        return await next();
    }
}
// Behaviors adicionales: LoggingBehavior (Serilog), AuditBehavior (AuditLog).
```

---

## 5. Capa Infrastructure

EF Core (DbContext, configuraciones, migraciones), implementación de repositorios y Unit Of Work, y servicios (JWT, hashing, almacenamiento de imágenes tras `IAlmacenamientoService` — §5.4, ADR-013, notificaciones, auditoría).

### 5.1 DbContext con Soft Delete global

```csharp
public class ExchangePlatformDbContext : DbContext
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Objeto> Objetos => Set<Objeto>();
    public DbSet<Intercambio> Intercambios => Set<Intercambio>();
    // ...

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExchangePlatformDbContext).Assembly);

        // Filtro global de Soft Delete (RN-013)
        modelBuilder.Entity<Usuario>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Objeto>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Intercambio>().HasQueryFilter(e => !e.IsDeleted);
        // ...para cada entidad con soft delete
    }
}
```

### 5.2 Configuración de Entidad (Fluent API)

```csharp
public class ObjetoConfiguration : IEntityTypeConfiguration<Objeto>
{
    public void Configure(EntityTypeBuilder<Objeto> builder)
    {
        builder.ToTable("Objetos");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Titulo).IsRequired().HasMaxLength(100);
        builder.Property(o => o.Descripcion).IsRequired().HasMaxLength(1000);
        builder.HasOne<Categoria>().WithMany().HasForeignKey(o => o.CategoriaId);
        // índices, constraints según BD.md
    }
}
```

### 5.3 Unit Of Work

```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly ExchangePlatformDbContext _context;
    public IUsuarioRepository Usuarios { get; }
    public IObjetoRepository Objetos { get; }
    public IIntercambioRepository Intercambios { get; }

    public UnitOfWork(ExchangePlatformDbContext context, /* repos */)
    {
        _context = context;
        // inicializa repositorios
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
```

### 5.4 Almacenamiento de imágenes (`IAlmacenamientoService`, ADR-013)

La subida de imágenes (objetos y foto de perfil) se abstrae tras la interfaz
`IAlmacenamientoService` (`Domain`), con dos implementaciones seleccionadas por
ambiente en `Program.cs`:

| Implementación                 | Ambiente            | Comportamiento                                                                 |
|--------------------------------|---------------------|-------------------------------------------------------------------------------|
| `AlmacenamientoLocalService`   | Desarrollo / Test   | Guarda en `wwwroot/uploads` y sirve el archivo como estático (`UseStaticFiles`). |
| `AlmacenamientoSupabaseService`| Staging / Producción| Sube a un bucket público de **Supabase Storage** (API REST) y devuelve la URL pública del CDN. |

```csharp
public interface IAlmacenamientoService
{
    // Devuelve la URL pública del archivo guardado.
    Task<string> GuardarAsync(Stream contenido, string extension, CancellationToken ct = default);
}
```

> **Por qué Supabase Storage en producción (ADR-013).** El filesystem del
> contenedor de Render es efímero: todo lo escrito en `wwwroot/uploads` se pierde
> en cada deploy o reinicio, dejando las imágenes en 404. `AlmacenamientoSupabaseService`
> se activa cuando `Supabase__Url` y `Supabase__ServiceKey` están definidas; sin
> ellas se usa el disco local. La `service_role` key vive **solo** en el backend.
> La URL resultante se persiste en `ImagenesObjeto.url` (ver `BD.md`).

---

## 6. Capa API

Controllers (delgados), middlewares, filtros, configuración. **Sin lógica de negocio.**

### 6.1 Controller (ejemplo)

```csharp
[ApiController]
[Route("api/v1/exchanges")]
public class ExchangesController : ControllerBase
{
    private readonly IMediator _mediator;
    public ExchangesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Crear([FromBody] CrearIntercambioRequest req)
    {
        var solicitanteId = User.GetUserId(); // del JWT
        var id = await _mediator.Send(new CrearIntercambioCommand(
            req.ObjetoSolicitadoId, req.ObjetoOfrecidoId, solicitanteId, req.MensajeInicial));
        return Created($"/api/v1/exchanges/{id}", ApiResponse.Ok(new { id }));
    }

    [HttpPatch("{id}/accept")]
    [Authorize]
    public async Task<IActionResult> Aceptar(Guid id)
        => Ok(await _mediator.Send(new AceptarIntercambioCommand(id, User.GetUserId())));
}
```

### 6.2 Middleware de Manejo Global de Errores

```csharp
// Captura excepciones y devuelve el contrato estándar ApiResponse
// DomainException/ConflictException -> 409/422; NotFoundException -> 404;
// ValidationException -> 422; no controlada -> 500 (registrada en Serilog).
public class ExceptionHandlingMiddleware { /* ... */ }
```

---

## 7. Configuración Transversal

| Aspecto                   | Implementación (en Program.cs / extensiones)                        |
|---------------------------------------|-----------------------------------------------------------------------------------|
| Inyección de dependencias | Registro de DbContext, UoW, repositorios, servicios, MediatR.       |
| MediatR + Behaviors       | Registro de handlers y pipeline (Validation, Logging, Audit).       |
| EF Core                   | Connection string desde variable de entorno; migraciones.           |
| JWT                       | Esquema Bearer, validación de issuer/audience/firma (Seguridad.md). |
| Autorización              | Políticas por rol (Administrador, Moderador, Usuario).              |
| Serilog                   | Logging estructurado a consola y archivo (volumen logs).            |
| Swagger                   | Solo en Development/Staging.                                        |
| CORS                      | Orígenes permitidos por variable de entorno.                        |
| Rate Limiting             | Límites por endpoint (Seguridad.md §10).                            |
| Global Exception          | Middleware que devuelve el contrato estándar.                       |

---

## 8. Flujo Completo de un Caso de Uso (ejemplo)

UC-020 "Solicitar intercambio" de extremo a extremo:

```
1. POST /api/v1/exchanges  →  ExchangesController.Crear
2. Controller crea CrearIntercambioCommand y llama _mediator.Send
3. Pipeline: ValidationBehavior (FluentValidation) → LoggingBehavior → AuditBehavior
4. CrearIntercambioCommandHandler:
     - obtiene objeto solicitado (repo)
     - valida RN-022 (no propio) y RN-014 (disponible)
     - crea entidad Intercambio (constructor valida invariantes)
     - uow.Intercambios.AddAsync + uow.SaveChangesAsync
5. (Evento/servicio) genera Notificacion al propietario
6. Controller devuelve 201 Created con ApiResponse
```

---

## 9. Orden de Implementación

Recomendado para construir incrementalmente respetando dependencias:

```
1. Solución y proyectos (4 capas + tests) con referencias correctas.
2. Domain: BaseEntity, enums, entidades, value objects, interfaces.
3. Infrastructure: DbContext + configuraciones + migración inicial + seed (UBIGEO).
4. Application: DTOs, ApiResponse, behaviors, mappings.
5. Transversal API: DI, MediatR, JWT, Serilog, Swagger, middleware de errores.
6. Módulo AUTH (registro, login, refresh, logout) + pruebas.
7. Módulo USERS (perfil).
8. Módulo OBJECTS (publicar, editar, buscar) + pruebas.
9. Módulo EXCHANGES (solicitar, aceptar, confirmar, calificar) + pruebas.
10. Módulos NOTIFICATIONS, FAVORITES, REPORTS, MESSAGES.
11. Módulo ADMIN (dashboard, gestión, auditoría).
12. Endurecimiento: rate limiting, headers, cobertura >= 90%.
```

> Cada módulo se implementa con su rama feature/*, pruebas y PR (GitFlow.md), respetando el DoD (ChecklistCalidad.md).

---

## 10. Mapeo Módulo → Componentes

| Módulo        | Commands/Queries principales                                  | Endpoints (API.md) |
|---------------|---------------------------------------------------------------|--------------------|
| AUTH          | RegistrarUsuario, IniciarSesion, RenovarToken, CerrarSesion   | /auth/*            |
| USERS         | ObtenerPerfil, ActualizarPerfil, CambiarPassword              | /users/*           |
| OBJECTS       | CrearObjeto, ActualizarObjeto, EliminarObjeto, ObtenerObjetos | /objects/*         |
| EXCHANGES     | CrearIntercambio, Aceptar, Rechazar, Confirmar, Cancelar      | /exchanges/*       |
| RATINGS       | CrearCalificacion, ObtenerCalificaciones                      | /ratings/*         |
| MESSAGES      | EnviarMensaje, ObtenerMensajes                                | /messages/*        |
| NOTIFICATIONS | ObtenerNotificaciones, MarcarLeida                            | /notifications/*   |
| FAVORITES     | AgregarFavorito, QuitarFavorito, ObtenerFavoritos             | /favorites/*       |
| REPORTS       | CrearReporte                                                  | /reports           |
| GEO           | ObtenerDepartamentos/Provincias/Distritos/Categorias          | /geo/*             |
| ADMIN         | Dashboard, GestionUsuarios, GestionReportes, Auditoria        | /admin/*           |

---

## 11. Aprobación

| Rol                       | Nombre            | Aprobación  | Fecha |
|---------------------------|-------------------|-------------|-------|
| Backend Developer Senior  | Equipo Enterprise | ☐ PENDIENTE | —     |
| Arquitecto de Software    | Equipo Enterprise | ☐ PENDIENTE | —     |
| Especialista EF Core      | Equipo Enterprise | ☐ PENDIENTE | —     |

---

> **GATE DE CALIDAD — FASE 3 (Backend):**
> El código debe seguir esta guía y los documentos de diseño. Cada módulo pasa por
> code review, pruebas y cobertura antes de integrarse (ChecklistCalidad.md).

---

*Documento generado bajo la metodología SDD — Plataforma Inteligente de Intercambio de Objetos — Ayacucho, Perú.*
