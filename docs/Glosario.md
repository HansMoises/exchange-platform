# Glosario.md

> **Documento:** Glosario de Términos
> **Fase SDLC:** Transversal (inicia en Fase 1, se mantiene en todo el ciclo)
> **Estado:** `BORRADOR — PENDIENTE DE APROBACIÓN`
> **Versión:** 0.4.0 (amplía la versión semilla de `VisionProyecto.md`)
> **Fecha:** 2026-07-08
> **Autor:** Equipo Enterprise
> **Propósito:** Unificar el lenguaje del proyecto (lenguaje ubicuo de DDD) entre negocio y técnica.
> **Nota de versión:** término "SQL Server" reemplazado por "PostgreSQL / Supabase" (migración de motor de BD, ver ADR-010 en Arquitectura.md).

---

## 1. Introducción

Este glosario define el **lenguaje ubicuo** del proyecto: el vocabulario común que comparten stakeholders, analistas y desarrolladores. Un término aquí significa lo mismo en la documentación, el código y las conversaciones. Es un documento vivo que se amplía a medida que avanza el proyecto.

## 2. Términos de Negocio

| Término                  | Definición                                                             |
|--------------------------|------------------------------------------------------------------------|
| Objeto                   | Bien físico en desuso publicado para intercambio.                      |
| Intercambio              | Acuerdo entre dos usuarios para ceder/recibir objetos.                 |
| Solicitud de intercambio | Petición de un usuario para intercambiar sobre un objeto ajeno.        |
| Ofertante                | Usuario dueño del objeto sobre el que se solicita intercambio.         |
| Solicitante              | Usuario que inicia una solicitud de intercambio.                       |
| Reputación               | Valor promedio acumulado a partir de calificaciones recibidas.         |
| Calificación             | Valoración numérica (y comentario opcional) tras un intercambio.       |
| No anónimo               | Toda identidad y ubicación es verificable y visible en el intercambio. |
| Economía circular        | Modelo que prolonga la vida útil de los bienes mediante reutilización. |
| Favorito                 | Objeto marcado por un usuario para seguimiento.                        |
| Reporte                  | Aviso de un usuario sobre objeto/usuario que infringe normas.          |
| Moderador                | Rol que gestiona reportes y modera contenido.                          |
| Administrador            | Rol con control total de la plataforma.                                |

## 3. Términos de Metodología y Proceso

| Término                | Definición                                                                                 |
|------------------------|--------------------------------------------------------------------------------------------|
| SDD                    | Spec Driven Development: la especificación documentada precede y gobierna al código.       |
| SDLC                   | Software Development Life Cycle: ciclo de vida del software (Análisis→…→Mantenimiento).    |
| Gate                   | Punto de control de aprobación obligatorio entre fases.                                    |
| Trazabilidad           | Capacidad de seguir un elemento desde su origen (objetivo) hasta su verificación (prueba). |
| MoSCoW                 | Priorización: Must, Should, Could, Won't.                                                  |
| DoD                    | Definition of Done: criterios para considerar algo terminado.                              |
| Criterio de aceptación | Condición verificable (Dado-Cuando-Entonces) que valida una historia.                      |
| ADR                    | Architecture Decision Record: registro de una decisión arquitectónica.                     |
| RACI                   | Matriz de responsabilidades: Responsible, Accountable, Consulted, Informed.                |
| BCP                    | Business Continuity Plan: plan de continuidad del negocio.                                 |
| DRP                    | Disaster Recovery Plan: plan de recuperación ante desastres.                               |

## 4. Términos de Arquitectura y Diseño

| Término                    | Definición                                                                                    |
|----------------------------|-----------------------------------------------------------------------------------------------|
| Clean Architecture         | Arquitectura en capas con dependencias hacia el dominio.                                      |
| CQRS                       | Command Query Responsibility Segregation: separa comandos (escritura) de consultas (lectura). |
| DDD                        | Domain Driven Design: diseño guiado por el dominio del negocio.                               |
| Repository Pattern         | Abstracción de acceso a datos.                                                                |
| Unit Of Work               | Patrón que agrupa operaciones en una transacción coherente.                                   |
| Entidad                    | Objeto del dominio con identidad propia.                                                      |
| Value Object               | Objeto sin identidad, definido por sus valores.                                               |
| Aggregate                  | Conjunto de entidades tratadas como una unidad de consistencia.                               |
| Domain Service             | Lógica de dominio que no pertenece a una entidad concreta.                                    |
| Command                    | Operación que cambia estado (CQRS).                                                           |
| Query                      | Operación de solo lectura (CQRS).                                                             |
| Handler                    | Componente que procesa un Command o Query (MediatR).                                          |
| Feature Based Architecture | Organización del frontend por funcionalidades (features).                                     |
| Modelo C4                  | Niveles de diagramación: Context, Container, Component, Code.                                 |

## 5. Términos de Tecnología

| Término               | Definición                                                       |
|-----------------------|------------------------------------------------------------------|
| .NET 10               | Plataforma de desarrollo backend utilizada.                      |
| ASP.NET Core Web API  | Framework para construir la API REST.                            |
| Entity Framework Core | ORM para acceso a datos sobre PostgreSQL (proveedor Npgsql).     |
| PostgreSQL / Supabase | Sistema gestor de base de datos relacional gestionado (Supabase). |
| MediatR               | Librería para implementar CQRS y mensajería interna.             |
| AutoMapper            | Mapeo entre entidades y DTOs.                                    |
| FluentValidation      | Validación de datos en backend.                                  |
| Serilog               | Librería de logging estructurado.                                |
| Swagger               | Documentación interactiva de la API.                             |
| JWT                   | JSON Web Token: token de autenticación.                          |
| Access Token          | Token de acceso de corta duración.                               |
| Refresh Token         | Token para renovar el acceso sin reautenticar.                   |
| React                 | Librería frontend de interfaces.                                 |
| TypeScript            | Lenguaje tipado sobre JavaScript.                                |
| Vite                  | Herramienta de build del frontend.                               |
| TailwindCSS           | Framework de estilos utilitario.                                 |
| Zustand               | Librería de gestión de estado en React.                          |
| Zod                   | Validación de esquemas en frontend.                              |
| Docker                | Plataforma de contenedores.                                      |
| CI/CD                 | Integración y entrega continua.                                  |
| OWASP Top 10          | Lista de los riesgos de seguridad web más críticos.              |
| Soft Delete           | Borrado lógico: marca como eliminado sin borrar físicamente.     |
| Paginación            | División de resultados en páginas (PageNumber, PageSize).        |
| DTO                   | Data Transfer Object: objeto para transportar datos entre capas. |
| E2E (End To End)      | Prueba que recorre el sistema completo, de extremo a extremo: navegador → frontend → API → base de datos. Verifica el comportamiento tal como lo experimenta el usuario final, no piezas aisladas. |
| Playwright            | Framework de pruebas E2E que automatiza un navegador real. Herramienta de la capa E2E del proyecto (13 specs — `Testing.md` §6.3). |
| Spec                  | Archivo de pruebas de Playwright (`*.spec.ts`). Agrupa las pruebas de un flujo funcional (p. ej. `auth.spec.ts`). |
| Testcontainers        | Librería que levanta contenedores Docker efímeros desde el código de las pruebas. Usada en las pruebas de integración del backend. |
| Aislamiento de BD     | Principio por el cual cada ambiente (Producción, Desarrollo, Test) opera sobre una base de datos físicamente distinta. Ningún proceso de desarrollo o prueba escribe sobre Producción (ADR-012). |
| BD efímera            | Base de datos que se crea y se destruye con cada ejecución, sin conservar datos entre corridas. Garantiza pruebas deterministas. En este proyecto se implementa montando el directorio de datos en RAM (`tmpfs`). |
| tmpfs                 | Sistema de archivos montado en memoria RAM. Su contenido desaparece al detener el contenedor. Usado en `docker-compose.test.yml` para que la BD de test no pueda persistir datos. |
| Paridad de entornos   | Principio (12-Factor App) según el cual desarrollo, prueba y producción deben ser lo más parecidos posible. En este proyecto: misma imagen `postgres:15-alpine` en los tres. |
| Quality Gate          | Criterio de salida obligatorio que detiene el pipeline si no se cumple (cobertura ≥ 90%, suite E2E en verde, 0 vulnerabilidades críticas). |
| Flaky test            | Prueba que falla de forma intermitente sin que el código haya cambiado. Suele indicar un problema de aislamiento o de sincronización, no un defecto real del sistema. |

## 6. Control de Cambios y Aprobación

| Versión | Fecha      | Autor             | Descripción                                                         |
|---------|------------|-------------------|---------------------------------------------------------------------|
| 0.1.0   | 2026-06-03 | Equipo Enterprise | Versión semilla incluida en la Visión.                              |
| 0.2.0   | 2026-06-03 | Equipo Enterprise | Glosario ampliado: negocio, metodología, arquitectura y tecnología. |

**Aprobación requerida (Fase 1):**

| Rol (RACI)                 | Responsabilidad            | Estado    |
|----------------------------|----------------------------|-----------|
| Arquitecto Empresarial (A) | Aprueba el lenguaje ubicuo | Pendiente |
| Product Owner (R)          | Valida términos de negocio | Pendiente |

> **Regla SDD:** Documento transversal; debe mantenerse actualizado en cada fase.
