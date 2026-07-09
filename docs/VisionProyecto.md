# VisionProyecto.md

> **Documento:** Visión del Proyecto
> **Paso SDD:** 1 de 8 (Visión) — **Fase SDLC:** 1 (Análisis)
> **Estado:** `BORRADOR — PENDIENTE DE APROBACIÓN`
> **Versión:** 0.5.0
> **Fecha:** 2026-07-08
> **Autor:** Equipo Enterprise (Arquitecto Empresarial Senior)
> **Fuente de verdad:** Este documento es el artefacto raíz de la trazabilidad. Ningún requisito, regla, caso de uso o línea de código puede existir sin rastrearse hasta aquí.
> **Nota de versión:** RST-001 actualizada — motor de BD migrado de SQL Server a PostgreSQL/Supabase (ver ADR-010 en Arquitectura.md).

---

## Tabla de Contenido

1. Antecedentes
2. Problemática
3. Justificación
4. Objetivos (General y Específicos)
5. Alcance
6. Fuera de Alcance
7. Stakeholders
8. Beneficios
9. Restricciones
10. Supuestos
11. Riesgos (versión inicial)
12. Criterios de Éxito
13. KPIs (Negocio y Técnicos)
14. Glosario mínimo
15. Control de Cambios y Aprobación

---

## 1. Antecedentes

En la ciudad de Ayacucho, y en general en el Perú, existe una cantidad significativa de objetos en desuso (ropa, libros, herramientas, electrodomésticos, muebles, juguetes, equipos electrónicos) que conservan valor funcional para otras personas. Hoy estos intercambios ocurren de manera **informal** a través de Facebook Marketplace, grupos de WhatsApp, redes sociales y contactos personales.

Estos mecanismos no fueron diseñados para el intercambio seguro y trazable de bienes entre ciudadanos: carecen de identidad verificada, de geolocalización estructurada, de reputación acumulable y de control administrativo. El presente proyecto nace para cubrir ese vacío con una plataforma web profesional, segura y escalable.

## 2. Problemática

El intercambio informal actual presenta las siguientes deficiencias estructurales:

- **Falta de organización:** la información de objetos está dispersa y sin categorización consistente.
- **Falta de trazabilidad:** no existe historial verificable de intercambios.
- **Falta de geolocalización:** no se conoce la cercanía real entre las partes.
- **Intercambios anónimos:** las partes no tienen identidad verificada.
- **Riesgo de fraude:** ausencia de mecanismos de confianza y verificación.
- **Baja confianza:** no hay reputación acumulada ni calificaciones.
- **Falta de reputación e historial:** cada intercambio parte de cero.
- **Falta de control administrativo:** no hay moderación, auditoría ni gestión de reportes.

**Enunciado del problema:** *No existe en Ayacucho una plataforma formal que permita intercambiar objetos en desuso de forma segura, identificada, geolocalizada y con reputación, lo que limita la economía circular y expone a los ciudadanos a fraude y desconfianza.*

## 3. Justificación

Una plataforma formal de intercambio aporta valor en tres dimensiones:

- **Ambiental:** fomenta la economía circular, reduce residuos y promueve la reutilización de recursos, alineándose con la sostenibilidad.
- **Social:** incrementa la confianza entre ciudadanos, fortalece la comunidad y genera impacto social positivo mediante el intercambio responsable.
- **Económica:** facilita el acceso a bienes funcionales sin desembolso monetario, optimizando recursos en los hogares.

La formalización (identidad, geolocalización, reputación, auditoría) es lo que diferencia esta plataforma de las soluciones informales actuales y constituye su propuesta de valor central.

## 4. Objetivos

### 4.1 Objetivo General

Desarrollar una plataforma web profesional para la gestión **segura, trazable y geolocalizada** de intercambios de objetos entre ciudadanos de Ayacucho, con capacidad de escalamiento a nivel nacional sin rediseños estructurales.

### 4.2 Objetivos Específicos

| ID     | Objetivo específico                                                                                  |
|--------|------------------------------------------------------------------------------------------------------|
| OE-001 | Permitir el registro de usuarios con identidad y ubicación verificables.                             |
| OE-002 | Proporcionar inicio de sesión, recuperación de contraseña y gestión de perfil.                       |
| OE-003 | Permitir la publicación de objetos con gestión de múltiples imágenes.                                |
| OE-004 | Ofrecer búsqueda avanzada con filtros geográficos y por categoría.                                   |
| OE-005 | Gestionar solicitudes de intercambio con aceptación/rechazo por ambas partes.                        |
| OE-006 | Mantener un historial completo y trazable de intercambios.                                           |
| OE-007 | Implementar un sistema de reputación con calificaciones y comentarios.                               |
| OE-008 | Permitir reportes de usuarios/objetos y su gestión administrativa.                                   |
| OE-009 | Ofrecer favoritos, notificaciones y chat entre usuarios.                                             |
| OE-010 | Proveer un panel administrativo con dashboards e indicadores.                                        |
| OE-011 | Garantizar geolocalización (departamento, provincia, distrito, lat/long) para búsqueda por cercanía. |
| OE-012 | Asegurar trazabilidad y auditoría completas de las acciones del sistema.                             |
| OE-013 | Diseñar la arquitectura para escalamiento nacional sin rediseños estructurales.                      |

> Cada OE-XXX será descompuesto en requisitos (`Requisitos.md`), reglas de negocio (`ReglasNegocio.md`) y casos de uso (`CasosDeUso.md`), y rastreado en `MatrizTrazabilidad.md`.

## 5. Alcance

- **Fase inicial (este proyecto / MVP V1):** despliegue para la ciudad de **Ayacucho**.
- **Fase futura:** escalamiento a la región Ayacucho y posteriormente a todo el **Perú**.
- La arquitectura (Clean Architecture, CQRS, datos geográficos jerárquicos Departamento→Provincia→Distrito) **deberá permitir el crecimiento nacional sin rediseños estructurales**.

**Funcionalidades dentro del alcance (visión global del producto):** registro/login/recuperación, perfil, publicación de objetos e imágenes, búsqueda y filtros geográficos, solicitudes de intercambio, historial, reputación y calificaciones, reportes, favoritos, chat, notificaciones y panel administrativo.

> El alcance **por versión** (qué entra en V1, V2, V3…) se detalla en `Roadmap.md`. Esta sección declara el alcance total del producto; el detalle versionado evita comprometer el cronograma del MVP.

## 6. Fuera de Alcance

Quedan explícitamente **fuera** del alcance del proyecto (al menos del MVP, salvo indicación del Roadmap):

- Transacciones monetarias, pasarelas de pago o cobros (la plataforma es de intercambio, no de venta).
- Logística de transporte o entrega de objetos (la coordinación física es responsabilidad de los usuarios).
- Aplicación móvil nativa Android/iOS (prevista para V5 del Roadmap).
- Funciones de Inteligencia Artificial (recomendaciones, detección de fraude, clasificación automática) — previstas para V3/V4.
- Internacionalización fuera del Perú (fase posterior a la nacional).
- Integraciones con sistemas externos de correo/mapas/notificaciones en producción real durante el MVP (se dejan **preparadas**, no necesariamente activas).

## 7. Stakeholders

| ID      | Stakeholder           | Rol / Interés                                              | Tipo                |
|---------|-----------------------|------------------------------------------------------------|---------------------|
| STK-001 | Ciudadano Usuario     | Publica e intercambia objetos; busca confianza y cercanía. | Externo / Primario  |
| STK-002 | Moderador             | Revisa reportes, modera publicaciones y usuarios.          | Interno / Operativo |
| STK-003 | Administrador         | Gestiona la plataforma, usuarios, categorías y auditoría.  | Interno / Operativo |
| STK-004 | Product Owner         | Define prioridades y valida valor de negocio.              | Interno / Decisión  |
| STK-005 | Equipo de Desarrollo  | Diseña, construye y mantiene el sistema.                   | Interno / Ejecución |
| STK-006 | Comunidad de Ayacucho | Beneficiaria del impacto social y ambiental.               | Externo / Indirecto |
| STK-007 | Entidades municipales | Potenciales aliados de economía circular.                  | Externo / Futuro    |

## 8. Beneficios

- **Para el ciudadano:** acceso a objetos útiles sin costo monetario, con seguridad e identidad verificada.
- **Para la comunidad:** mayor confianza, reducción de residuos y fortalecimiento del tejido social.
- **Para el ambiente:** menos desperdicio, más reutilización, economía circular activa.
- **Para la administración:** trazabilidad, control, métricas e indicadores para la toma de decisiones.
- **Estratégicos:** base tecnológica escalable a nivel nacional y evolucionable hacia IA y movilidad.

## 9. Restricciones

| ID      | Restricción                                                                                                                             |
|---------|-----------------------------------------------------------------------------------------------------------------------------------------|
| RST-001 | Tecnológicas backend: .NET 10, ASP.NET Core Web API, EF Core (Npgsql), PostgreSQL (Supabase), MediatR, AutoMapper, FluentValidation, Serilog, Swagger, JWT. |
| RST-002 | Tecnológicas frontend: React, TypeScript, Vite, TailwindCSS, Axios, React Router DOM, React Hook Form, Zod, Zustand, Node.js LTS.       |
| RST-003 | Arquitectura obligatoria: Clean Architecture + CQRS + Repository + Unit Of Work (backend); Feature Based (frontend).                    |
| RST-004 | Metodológica: desarrollo estricto bajo SDD; sin código sin documentación aprobada; flujo SDLC secuencial con gates.                     |
| RST-005 | DevOps: Docker, Docker Compose, Git, GitHub, GitHub Actions.                                                                            |
| RST-006 | Idioma: documentación en español; código en inglés.                                                                                     |
| RST-007 | Calidad: cobertura mínima de pruebas 90% (ideal 95%).                                                                                   |
| RST-008 | Despliegue inicial: local, pero preparado para nube (Azure/AWS/GCP).                                                                    |
| RST-009 | Seguridad: intercambios no anónimos; cumplimiento OWASP Top 10.                                                                         |

## 10. Supuestos

| ID      | Supuesto                                                                                        |
|---------|-------------------------------------------------------------------------------------------------|
| SUP-001 | Los usuarios disponen de acceso a internet y a un navegador moderno.                            |
| SUP-002 | Los usuarios proporcionarán datos reales de identidad y ubicación.                              |
| SUP-003 | La coordinación física del intercambio la realizan los usuarios fuera de la plataforma.         |
| SUP-004 | Existe disponibilidad de los datos geográficos del Perú (departamentos, provincias, distritos). |
| SUP-005 | El equipo cuenta con las herramientas y licencias necesarias (VS 2026, VS Code, etc.).          |
| SUP-006 | El despliegue inicial será en un entorno controlado (local/contenedores).                       |

## 11. Riesgos (versión inicial)

> Versión semilla; se ampliará y mantendrá en `Riesgos.md`. Probabilidad/Impacto en escala Baja/Media/Alta.

| ID      | Riesgo                                                 | Prob. | Impacto | Mitigación inicial                                         |
|---------|--------------------------------------------------------|-------|---------|------------------------------------------------------------|
| RGO-001 | Suplantación de identidad / usuarios falsos            | Media | Alto    | Verificación de datos, reputación, reportes y moderación.  |
| RGO-002 | Fraude en intercambios                                 | Media | Alto    | Identidad no anónima, reputación, historial, auditoría.    |
| RGO-003 | Baja adopción inicial en Ayacucho                      | Media | Alto    | UX simple, confianza visible, difusión comunitaria.        |
| RGO-004 | Crecimiento de datos degrada rendimiento               | Media | Medio   | Índices, paginación, caching futuro, diseño escalable.     |
| RGO-005 | Contenido inapropiado u objetos prohibidos             | Media | Medio   | Moderación, reportes, categorías controladas.              |
| RGO-006 | Desviación de la metodología SDD por presión de tiempo | Media | Alto    | Gates de aprobación obligatorios; calidad sobre velocidad. |
| RGO-007 | Vulnerabilidades de seguridad (OWASP)                  | Media | Alto    | Diseño seguro, revisiones, pruebas de seguridad.           |
| RGO-008 | Pérdida de datos                                       | Baja  | Alto    | Backups diario/semanal/mensual; DRP.                       |

## 12. Criterios de Éxito

| ID     | Criterio de Éxito                                                                                                          |
|--------|----------------------------------------------------------------------------------------------------------------------------|
| CE-001 | El MVP permite registro, login, publicación, búsqueda geolocalizada e intercambio completo de extremo a extremo.           |
| CE-002 | Todo intercambio es no anónimo, trazable y auditado.                                                                       |
| CE-003 | La plataforma alcanza la cobertura de pruebas mínima (≥90%).                                                               |
| CE-004 | El sistema cumple las mitigaciones OWASP Top 10 verificadas en pruebas de seguridad.                                       |
| CE-005 | La arquitectura permite incorporar nuevas regiones sin rediseño estructural (verificable por diseño de datos geográficos). |
| CE-006 | La documentación SDD está completa, aprobada y trazable antes de cada implementación.                                      |
| CE-007 | Usuarios reales completan intercambios en Ayacucho durante el piloto con índice de satisfacción favorable.                 |

## 13. KPIs

### 13.1 KPIs de Negocio

| ID      | KPI                            | Descripción                         |
|---------|--------------------------------|-------------------------------------|
| KPI-001 | Usuarios registrados           | Total y activos por periodo.        |
| KPI-002 | Objetos publicados             | Total y por categoría.              |
| KPI-003 | Intercambios realizados        | Completados vs. pendientes.         |
| KPI-004 | Tiempo promedio de intercambio | Desde solicitud hasta finalización. |
| KPI-005 | Tasa de éxito de intercambios  | % de solicitudes que finalizan.     |
| KPI-006 | Objetos reutilizados           | Impacto de economía circular.       |
| KPI-007 | Reputación promedio            | Calificación media de la comunidad. |

### 13.2 KPIs Técnicos

| ID      | KPI                      | Descripción                        |
|---------|--------------------------|------------------------------------|
| KPI-T01 | Tiempo de respuesta      | Latencia promedio de la API.       |
| KPI-T02 | Disponibilidad           | % uptime del sistema.              |
| KPI-T03 | Cobertura de pruebas     | % de código cubierto (≥90%).       |
| KPI-T04 | Errores por versión      | Defectos detectados por release.   |
| KPI-T05 | Rendimiento de consultas | Tiempo de consultas críticas a BD. |
| KPI-T06 | Consumo de recursos      | CPU/memoria por contenedor.        |

## 14. Glosario mínimo

> Versión semilla; se ampliará en `Glosario.md`.

- **Objeto:** bien físico en desuso publicado para intercambio.
- **Intercambio:** acuerdo entre dos usuarios para ceder/recibir objetos.
- **Reputación:** valor acumulado a partir de calificaciones de intercambios.
- **No anónimo:** todo usuario tiene identidad y ubicación verificables y visibles en el intercambio.
- **Economía circular:** modelo que prolonga la vida útil de los bienes mediante reutilización.
- **SDD (Spec Driven Development):** metodología donde la especificación documentada precede y gobierna al código.

## 15. Control de Cambios y Aprobación

| Versión | Fecha      | Autor             | Descripción                        |
|---------|------------|-------------------|------------------------------------|
| 0.1.0   | 2026-06-03 | Equipo Enterprise | Versión inicial para revisión.     |
| 0.2.0   | 2026-06-03 | Equipo Enterprise | Tablas con estilo visual.          |
| 0.3.0   | 2026-06-03 | Equipo Enterprise | HTML indentado en el código.       |
| 0.4.0   | 2026-06-03 | Equipo Enterprise | Tablas Markdown puras y alineadas. |

**Aprobación requerida (gate de salida Fase 1 / Paso 1 SDD):**

| Rol (RACI)                 | Responsabilidad                     | Estado    |
|----------------------------|-------------------------------------|-----------|
| Arquitecto Empresarial (A) | Aprueba la visión                   | Pendiente |
| Product Owner (R)          | Valida valor de negocio             | Pendiente |
| QA (C)                     | Revisa criterios de éxito y KPIs    | Pendiente |
| Stakeholders (C/I)         | Confirman problemática y beneficios | Pendiente |

> **Regla SDD:** No se avanza al PASO 2 (`Requisitos.md`) hasta que este documento sea aprobado formalmente.
