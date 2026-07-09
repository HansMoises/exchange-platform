using ExchangePlatform.API.Middlewares;
using ExchangePlatform.Application.Common.Extensions;
using ExchangePlatform.Domain.Interfaces.Services;
using ExchangePlatform.Infrastructure.Extensions;
using ExchangePlatform.Infrastructure.Persistence;
using ExchangePlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Exchange Platform v1"));
}

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