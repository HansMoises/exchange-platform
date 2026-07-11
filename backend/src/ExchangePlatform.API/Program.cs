using ExchangePlatform.API.Middlewares;
using ExchangePlatform.Application.Common.Extensions;
using ExchangePlatform.Domain.Interfaces.Services;
using ExchangePlatform.Infrastructure.Extensions;
using ExchangePlatform.Infrastructure.Persistence;
using ExchangePlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

// El WebRootFileProvider que usa UseStaticFiles() se fija en base a esta
// carpeta al momento de construir el host: debe existir ANTES de
// WebApplication.CreateBuilder, o los archivos subidos despues del arranque
// no se sirven (quedan en 404) aunque la carpeta se cree mas tarde.
// ContentRootPath (con "dotnet run") es el directorio actual del proyecto,
// no el bin/ de salida.
Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads"));

var builder = WebApplication.CreateBuilder(args);

// Validación temprana de configuración crítica. En producción appsettings.json
// no viaja en la imagen (está gitignored): TODO viene de variables de entorno
// de Render (formato Jwt__Secret, ConnectionStrings__DefaultConnection, etc.).
// Sin esto, una variable faltante no se nota al desplegar sino recién cuando
// un usuario ejerce el flujo que la usa (p. ej. login -> 500). Mejor fallar el
// arranque con un mensaje accionable en los logs del deploy.
var configCritica = new Dictionary<string, string?>
{
    ["ConnectionStrings:DefaultConnection"] = builder.Configuration.GetConnectionString("DefaultConnection"),
    ["Jwt:Secret"] = builder.Configuration["Jwt:Secret"],
    ["Jwt:Issuer"] = builder.Configuration["Jwt:Issuer"],
    ["Jwt:Audience"] = builder.Configuration["Jwt:Audience"],
};
var faltantes = configCritica.Where(kv => string.IsNullOrWhiteSpace(kv.Value))
                             .Select(kv => kv.Key)
                             .ToList();
if (faltantes.Count > 0)
    throw new InvalidOperationException(
        "Faltan variables de configuración críticas: " + string.Join(", ", faltantes) +
        ". En Render defínelas como variables de entorno con '__' (p. ej. Jwt__Secret).");
if (builder.Configuration["Jwt:Secret"]!.Length < 32)
    throw new InvalidOperationException(
        "Jwt:Secret debe tener al menos 32 caracteres.");

// Render (y otras plataformas cloud) inyectan el puerto via la variable PORT.
// Si existe, la app escucha en ese puerto en todas las interfaces (0.0.0.0).
var puerto = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(puerto))
    builder.WebHost.UseUrls($"http://0.0.0.0:{puerto}");

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Almacenamiento de imagenes: se registra aqui (no en AddInfrastructure)
// porque necesita IWebHostEnvironment, que solo esta disponible en el
// proyecto API (Infrastructure es una class library plana sin ASP.NET Core).
builder.Services.AddSingleton<IAlmacenamientoService>(_ =>
{
    var webRoot = builder.Environment.WebRootPath
        ?? Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
    var rutaUploads = Path.Combine(webRoot, "uploads");
    var baseUrl = builder.Configuration["BackendUrl"] ?? "https://localhost:7149";
    return new AlmacenamientoLocalService(rutaUploads, baseUrl);
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Healthcheck: verifica que la API responde Y que la conexion a la BD
// (PostgreSQL/Supabase) funciona. Lo consumen el healthcheck de Docker, los
// smoke tests de despliegue y el workflow keep-alive de Supabase (CICD.md).
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ExchangePlatformDbContext>("database");

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Plataforma de Intercambio de Objetos API",
        Version = "v1"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa tu JWT token"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    };

    c.AddSecurityRequirement(securityRequirement);
});

builder.Services.AddCors(options =>
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
                builder.Configuration["AllowedOrigins"] ?? "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()));

var app = builder.Build();

// Aplica automaticamente las migraciones pendientes al arrancar. En produccion
// (Render) esto crea/actualiza el esquema en Supabase en cada despliegue, sin
// el paso manual "dotnet ef database update" (ver Deployment.md seccion 4).
// Las pruebas de integracion aplican sus propias migraciones contra su
// contenedor Testcontainers, por eso lo desactivan con RunMigrationsAtStartup=false.
if (app.Configuration.GetValue("RunMigrationsAtStartup", true))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ExchangePlatformDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Exchange Platform v1"));
}

// En produccion (Render) el HTTPS lo termina el proxy de la plataforma; dentro
// del contenedor la app corre en HTTP. Redirigir aqui causaria bucles o fallos
// en el healthcheck. Solo se aplica redireccion HTTPS en desarrollo local.
if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Necesario para que WebApplicationFactory<Program> (pruebas de integracion)
// pueda referenciar el entry point generado por los top-level statements.
public partial class Program { }