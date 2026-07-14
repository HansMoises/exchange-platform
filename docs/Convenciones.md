# Convenciones.md
# Plataforma Inteligente de Intercambio de Objetos

> **Documento:** Convenciones de Código y Trabajo
> **Fase SDLC:** 2 (Diseño) — documento complementario, base para Fase 3
> **Versión:** 1.1.0
> **Estado:** `PENDIENTE DE APROBACIÓN`
> **Fecha:** 2026-06-03
> **Autor:** Equipo Enterprise Senior (Arquitecto / Programador Senior)
> **Documentos padre:** Arquitectura.md | Glosario.md
> **Convenciones:** Documentación en español. Diagramas en ASCII.

---

## Control de Versiones

| Versión | Fecha      | Autor                    | Cambios          |
|---------|------------|--------------------------|------------------|
| 1.0.0   | 2026-06-03 | Equipo Enterprise Senior | Versión inicial. |
| 1.1.0   | 2026-07-13 | Equipo Enterprise Senior | Se añade la convención de nomenclatura para **pruebas E2E** (`*.spec.ts`) y la regla de **variables de entorno .NET con doble guion bajo** (`ConnectionStrings__DefaultConnection`), crítica para el aislamiento de base de datos (ADR-012). |

---

## Tabla de Contenidos

1. Idioma del Proyecto
2. Nomenclatura General
3. Convenciones Backend (.NET / C#)
4. Convenciones Frontend (React / TypeScript)
5. Convenciones de Base de Datos
6. Convenciones de API REST
7. Formato y Estilo de Código
8. Convenciones de Commits
9. Convenciones de Ramas (resumen)
10. Documentación de Código
11. Principios Aplicados
12. Aprobación

---

## 1. Idioma del Proyecto

| Ámbito                       | Idioma                       | Ejemplo                                    |
|------------------------------|------------------------------|--------------------------------------------|
| Documentación (/docs)        | Español                      | "Reglas de Negocio", "Casos de Uso"        |
| Entidades y código de dominio| Español                      | Usuario, Objeto, Intercambio, Calificacion |
| Clases, métodos, variables   | Español                      | CrearObjetoCommand, ObtenerUsuarios()      |
| Columnas de base de datos    | Español (snake_case)         | usuario_id, estado_objeto                  |
| Rutas REST (URLs)            | Inglés                       | /users, /objects, /exchanges               |
| Palabras clave del lenguaje  | Inglés (propio del lenguaje) | public, async, return                      |
| Mensajes al usuario final    | Español                      | "Objeto publicado exitosamente."           |

> **Decisión del proyecto:** el código y las entidades se nombran en **español** (Usuario, Objeto, Intercambio). Las rutas REST se mantienen en **inglés** por ser el estándar universal de APIs y para legibilidad de endpoints (`/api/v1/exchanges`). Esta distinción es intencional y consistente.

---

## 2. Nomenclatura General

| Elemento     | Convención                                | Ejemplo                                         |
|--------------|-------------------------------------------|-------------------------------------------------|
| Clases       | PascalCase                                | UsuarioService, IntercambioRepository           |
| Interfaces   | I + PascalCase                            | IUsuarioRepository, IUnitOfWork                 |
| Métodos      | PascalCase (C#)                           | CrearUsuario(), ObtenerObjetos()                |
| Variables    | camelCase                                 | usuarioId, nombreObjeto, estadoIntercambio      |
| Constantes   | UPPER_CASE                                | MAX_TAMANO_ARCHIVO, PAGINA_TAMANO_DEFECTO       |
| Parámetros   | camelCase                                 | usuarioId, pageNumber                           |
| Enums        | PascalCase                                | EstadoObjeto, EstadoIntercambio                 |
| Carpetas     | PascalCase (.NET) / kebab o camel (front) | Commands/ , features/                           |

---

## 3. Convenciones Backend (.NET / C#)

### 3.1 Nomenclatura por Tipo

| Tipo                  | Sufijo / patrón            | Ejemplo                          |
|-----------------------|----------------------------|----------------------------------|
| Command               | {Accion}{Entidad}Command   | CrearObjetoCommand               |
| Command Handler       | {Command}Handler           | CrearObjetoCommandHandler        |
| Query                 | Obtener{Entidad}Query      | ObtenerObjetosQuery              |
| Validator             | {Command/Query}Validator   | CrearObjetoCommandValidator      |
| Repository (interfaz) | I{Entidad}Repository       | IObjetoRepository                |
| Repository (impl)     | {Entidad}Repository        | ObjetoRepository                 |
| DTO                   | {Entidad}Dto / {Accion}Dto | ObjetoDto, CrearObjetoDto        |
| Controller            | {Entidad}Controller        | ObjetosController                |
| Servicio              | {Nombre}Service            | JwtService, NotificacionService  |
| Configuración EF      | {Entidad}Configuration     | ObjetoConfiguration              |

### 3.2 Reglas

```
- Una clase por archivo. Nombre de archivo = nombre de clase.
- async/await para toda operación de I/O. Sufijo Async en métodos asíncronos.
- Inyección de dependencias por constructor (sin service locator).
- Campos privados con prefijo _ y readonly cuando aplique: private readonly IObjetoRepository _objetoRepository;
- Sin lógica de negocio en Controllers (solo orquestan vía MediatR).
- Sin acceso directo a DbContext fuera de Infrastructure.
- Nullable reference types habilitado.
```

### 3.3 Ejemplo de Handler

```csharp
public class CrearObjetoCommandHandler
    : IRequestHandler<CrearObjetoCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CrearObjetoCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CrearObjetoCommand request,
        CancellationToken cancellationToken)
    {
        var objeto = new Objeto(/* ... */);
        await _unitOfWork.ObjetoRepository.AddAsync(objeto);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return objeto.Id;
    }
}
```

---

## 4. Convenciones Frontend (React / TypeScript)

### 4.1 Nomenclatura

| Elemento            | Convención     | Ejemplo                          |
|---------------------|----------------|----------------------------------|
| Componentes         | PascalCase     | ObjectCard.tsx, LoginForm.tsx    |
| Hooks               | use + camelCase| useAuth.ts, usePagination.ts     |
| Servicios           | camelCase      | authService.ts, objetoService.ts |
| Tipos / Interfaces  | PascalCase     | UsuarioDto, IntercambioEstado    |
| Stores (Zustand)    | camelCase + Store | authStore.ts, objetoStore.ts  |
| Constantes          | UPPER_CASE     | API_BASE_URL, MAX_IMAGENES       |
| Variables/funciones | camelCase      | handleSubmit, objetoSeleccionado |
| Archivos de schema  | camelCase + .schemas | auth.schemas.ts            |
| Pruebas de componente | PascalCase + .test    | LoginForm.test.tsx         |
| Pruebas E2E           | camelCase + .spec     | auth.spec.ts, exchanges.spec.ts |
| Fixtures E2E          | camelCase + .fixture  | auth.fixture.ts            |

> **Distinción `.test.tsx` vs `.spec.ts`:** el sufijo `.test` identifica pruebas de Vitest
> (unitarias y de componentes, viven junto al código en `src/features/*/__tests__/`). El sufijo
> `.spec` identifica pruebas de Playwright (End To End, viven en `frontend/e2e/`). No es
> intercambiable: los *runners* están configurados para reclamar solo su propio sufijo.

### 4.2 Variables de Entorno (.NET) — convención crítica

La configuración anidada de .NET se mapea a variables de entorno con **doble guion bajo** (`__`),
no con dos puntos ni con punto:

| Configuración en `appsettings.json`   | Variable de entorno                    |
|---------------------------------------|----------------------------------------|
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` |
| `Jwt:Secret`                          | `Jwt__Secret`                          |
| `Jwt:Issuer`                          | `Jwt__Issuer`                          |
| `AllowedOrigins`                      | `AllowedOrigins`                       |

> ⚠️ **Precedencia:** la variable de entorno **sobrescribe** el valor de `appsettings.json`.
> Si `ConnectionStrings__DefaultConnection` no está definida, el `DesignTimeDbContextFactory`
> de EF Core cae **silenciosamente** al puerto 5432. Ver ADR-012 (`Arquitectura.md`),
> RGO-016 (`Riesgos.md`) y `Testing.md` §12.1.

### 4.2 Reglas

```
- Componentes funcionales con Hooks (sin clases).
- Un componente por archivo. Export por defecto del componente.
- Tipado estricto: sin "any". Usar interfaces/tipos para props.
- Validación de formularios con React Hook Form + Zod.
- Estado global solo en Zustand stores; estado local con useState.
- Llamadas a la API solo a través de services (Axios), nunca fetch directo en componentes.
- Componentes reutilizables en components/ui; lógica de feature en features/.
```

### 4.3 Ejemplo de Componente

```tsx
interface ObjectCardProps {
  objeto: ObjetoDto;
  onVerDetalle: (id: string) => void;
}

export default function ObjectCard({ objeto, onVerDetalle }: ObjectCardProps) {
  return (
    <article className="rounded-md shadow-card bg-white">
      {/* ... */}
    </article>
  );
}
```

---

## 5. Convenciones de Base de Datos

| Elemento          | Convención                  | Ejemplo                  |
|-------------------|-----------------------------|--------------------------|
| Tablas            | PascalCase, plural          | Usuarios, Objetos        |
| Columnas          | snake_case                  | usuario_id, created_at   |
| Clave primaria    | id                          | id                       |
| Clave foránea     | {entidad_singular}_id       | categoria_id             |
| Índice            | IX_{Tabla}_{Columna}        | IX_Objetos_CategoriaId   |
| Índice único      | UX_{Tabla}_{Columna}        | UX_Usuarios_Email        |
| Constraint CHECK  | CK_{Tabla}_{Columna}        | CK_Objetos_EstadoObjeto  |

> Detalle completo en `BD.md`.

---

## 6. Convenciones de API REST

| Aspecto    | Convención                                                      |
|------------|-----------------------------------------------------------------|
| Recursos   | Sustantivos plural en inglés: /users, /objects                  |
| Acciones   | Verbos HTTP, no en la URL: PATCH /exchanges/{id}/accept         |
| Versionado | /api/v1/...                                                     |
| Respuesta  | Contrato estándar { success, message, data, errors, timestamp } |
| Errores    | Códigos HTTP semánticos (ver API.md §15)                        |

> Detalle completo en `API.md`.

---

## 7. Formato y Estilo de Código

| Aspecto              | Regla                                               |
|----------------------|-----------------------------------------------------|
| Indentación          | 4 espacios (C#), 2 espacios (TS/TSX).               |
| Longitud de línea    | Máx. 120 caracteres.                                |
| Llaves               | C#: Allman. TS: K&R (misma línea).                  |
| Comillas (TS)        | Comillas simples por defecto.                       |
| Punto y coma (TS)    | Obligatorio.                                        |
| Imports              | Ordenados: librerías externas, internas, relativos. |
| Formateadores        | .editorconfig (C#), Prettier + ESLint (TS).         |
| Análisis estático    | Analyzers de .NET; ESLint en frontend.              |

---

## 8. Convenciones de Commits

Formato **Conventional Commits**: `tipo(ámbito): descripción`.

| Tipo     | Uso                                              |
|----------|--------------------------------------------------|
| feat     | Nueva funcionalidad.                             |
| fix      | Corrección de error.                             |
| refactor | Cambio interno sin alterar comportamiento.       |
| docs     | Cambios en documentación.                        |
| test     | Añadir o modificar pruebas.                      |
| style    | Formato, sin cambios de lógica.                  |
| chore    | Tareas de mantenimiento, configuración.          |
| perf     | Mejora de rendimiento.                           |

**Ejemplos:**

```
feat(auth): implementar endpoint de inicio de sesión
fix(objetos): corregir validación de tamaño de imagen
docs(api): actualizar contrato de intercambios
test(intercambios): agregar pruebas de aceptación de solicitud
refactor(usuarios): extraer lógica de reputación a servicio de dominio
```

Reglas: descripción en minúscula, imperativo, sin punto final, máx. 72 caracteres en el asunto.

---

## 9. Convenciones de Ramas (resumen)

Flujo **GitFlow** (detalle completo en `GitFlow.md`).

| Rama        | Propósito                                   |
|-------------|---------------------------------------------|
| main        | Código en producción. Siempre estable.      |
| develop     | Integración de funcionalidades.             |
| feature/*   | Nueva funcionalidad. Ej: feature/auth-login |
| release/*   | Preparación de versión. Ej: release/1.0.0   |
| hotfix/*    | Corrección urgente en producción.           |
| bugfix/*    | Corrección sobre develop.                   |

Nombre de rama: kebab-case descriptivo. Ej: `feature/publicar-objeto`.

---

## 10. Documentación de Código

```
- Métodos públicos no triviales: comentario XML (C#) o JSDoc (TS).
- El "qué" lo dice el nombre; el comentario explica el "por qué" cuando no es obvio.
- Sin comentarios obvios ni código comentado muerto.
- Swagger/OpenAPI documenta la API (sincronizado con API.md).
- README por proyecto cuando aporte valor.
```

Ejemplo (C#):

```csharp
/// <summary>
/// Recalcula la reputación promedio del usuario tras una nueva calificación.
/// Implementa la regla RN-032.
/// </summary>
public void ActualizarReputacion(int nuevaPuntuacion) { /* ... */ }
```

---

## 11. Principios Aplicados

| Principio              | En la práctica                                               |
|------------------------|--------------------------------------------------------------|
| SOLID                  | Clases con responsabilidad única; dependencias por interfaz. |
| DRY                    | Sin duplicación; lógica común en servicios/behaviors.        |
| KISS                   | Soluciones simples; complejidad justificada.                 |
| YAGNI                  | Solo lo que el MVP requiere.                                 |
| Clean Code             | Nombres claros, funciones cortas, sin efectos ocultos.       |
| Boy Scout Rule         | Dejar el código mejor de como se encontró.                   |
| Fail Fast              | Validar temprano; lanzar errores claros.                     |

---

## 12. Aprobación

| Rol                       | Nombre            | Aprobación   | Fecha |
|---------------------------|-------------------|--------------|-------|
| Arquitecto de Software    | Equipo Enterprise | ☐ PENDIENTE | —     |
| Backend Developer Senior  | Equipo Enterprise | ☐ PENDIENTE | —     |
| Frontend Developer Senior | Equipo Enterprise | ☐ PENDIENTE | —     |

---

> **GATE DE CALIDAD:**
> Estas convenciones son obligatorias en toda la implementación (Fase 3).
> El code review verifica su cumplimiento. El código que no las respete no se aprueba.

---

*Documento generado bajo la metodología SDD — Plataforma Inteligente de Intercambio de Objetos — Ayacucho, Perú.*
