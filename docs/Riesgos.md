# Riesgos.md

> **Documento:** Registro y Gestión de Riesgos
> **Fase SDLC:** Transversal (inicia en Fase 1, se mantiene en todo el ciclo)
> **Estado:** `BORRADOR — PENDIENTE DE APROBACIÓN`
> **Versión:** 0.3.0 (amplía la versión semilla de `VisionProyecto.md`)
> **Fecha:** 2026-06-03
> **Autor:** Equipo Enterprise (Arquitecto Empresarial / Product Owner)
> **Conecta con:** `VisionProyecto.md`, `Seguridad.md` (OWASP), BCP/DRP (Parte 6).

---

## 1. Introducción

Este registro identifica, evalúa y planifica la respuesta a los riesgos del proyecto. Es un documento **vivo**: se revisa en cada fase y se actualiza ante nuevos riesgos o cambios de exposición. La gestión sigue el ciclo identificar → analizar → planificar respuesta → monitorear.

## 2. Escalas de Evaluación

| Nivel | Probabilidad      | Impacto                       |
|-------|-------------------|-------------------------------|
| Bajo  | Improbable (<25%) | Efecto menor, absorbible      |
| Medio | Posible (25-60%)  | Retraso o sobrecosto moderado |
| Alto  | Probable (>60%)   | Compromete objetivos clave    |

## 3. Matriz de Exposición (Probabilidad × Impacto)

| P \ I | Bajo  | Medio | Alto    |
|-------|-------|-------|---------|
| Baja  | Bajo  | Bajo  | Medio   |
| Media | Bajo  | Medio | Alto    |
| Alta  | Medio | Alto  | Crítico |

> La **Exposición** resultante prioriza la atención: Crítico y Alto requieren plan de respuesta activo y monitoreo continuo.

## 4. Catálogo de Riesgos

| ID      | Riesgo                                                | Categoría | P     | I     | Exp.  | Estrategia |
|---------|-------------------------------------------------------|-----------|-------|-------|-------|------------|
| RGO-001 | Suplantación de identidad / usuarios falsos           | Seguridad | Media | Alto  | Alto  | Mitigar    |
| RGO-002 | Fraude en intercambios                                | Negocio   | Media | Alto  | Alto  | Mitigar    |
| RGO-003 | Baja adopción inicial en Ayacucho                     | Negocio   | Media | Alto  | Alto  | Mitigar    |
| RGO-004 | Crecimiento de datos degrada rendimiento              | Técnico   | Media | Medio | Medio | Mitigar    |
| RGO-005 | Contenido inapropiado u objetos prohibidos            | Operativo | Media | Medio | Medio | Mitigar    |
| RGO-006 | Desviación de la metodología SDD por presión          | Operativo | Media | Alto  | Alto  | Evitar     |
| RGO-007 | Vulnerabilidades de seguridad (OWASP)                 | Seguridad | Media | Alto  | Alto  | Mitigar    |
| RGO-008 | Pérdida de datos                                      | Técnico   | Baja  | Alto  | Medio | Mitigar    |
| RGO-009 | Dependencia de tecnologías nuevas (.NET 10, VS 2026)  | Técnico   | Media | Medio | Medio | Aceptar    |
| RGO-010 | Datos geográficos del Perú incompletos/inexactos      | Externo   | Media | Medio | Medio | Mitigar    |
| RGO-011 | Indisponibilidad de servicios externos (correo/mapas) | Externo   | Media | Medio | Medio | Transferir |
| RGO-012 | Rotación o falta de disponibilidad del equipo         | Operativo | Baja  | Alto  | Medio | Mitigar    |
| RGO-013 | Alcance creciente (scope creep)                       | Negocio   | Media | Alto  | Alto  | Evitar     |
| RGO-014 | Cobertura de pruebas por debajo del 90%               | Técnico   | Media | Medio | Medio | Mitigar    |
| RGO-015 | Mala experiencia de usuario reduce confianza          | Negocio   | Baja  | Alto  | Medio | Mitigar    |
| RGO-016 | **Migración accidental sobre la BD de producción** por fallback silencioso de EF Core al puerto 5432 | Técnico   | Media | **Crítico** | **Alto** | Mitigar |
| RGO-017 | Contaminación de la BD de producción con datos de prueba visibles en la sustentación | Técnico   | Alta  | Alto  | **Alto** | Evitar |

> Estrategias: **Evitar** (eliminar la causa), **Mitigar** (reducir P o I), **Transferir** (delegar a un tercero), **Aceptar** (asumir con contingencia).

## 5. Planes de Respuesta

| ID      | Plan de respuesta                                                                                  | Responsable                | Indicador de alerta                      |
|---------|----------------------------------------------------------------------------------------------------|----------------------------|------------------------------------------|
| RGO-001 | Verificación de datos, reputación visible, reportes y moderación; revisión de cuentas sospechosas. | Esp. Seguridad / Moderador | Pico de cuentas nuevas o reportes.       |
| RGO-002 | Identidad no anónima, historial y auditoría; bloqueo ante patrones de fraude.                      | Esp. Seguridad / Admin     | Reportes de fraude recurrentes.          |
| RGO-003 | UX simple, prueba de confianza, difusión comunitaria y piloto acotado.                             | Product Owner              | Bajo registro/uso en piloto.             |
| RGO-004 | Índices, paginación, consultas eficientes y caching futuro; pruebas de carga.                      | DBA / Backend              | Latencia de consultas creciente.         |
| RGO-005 | Categorías controladas, reportes y moderación con políticas claras.                                | Moderador / Admin          | Aumento de reportes de contenido.        |
| RGO-006 | Gates de aprobación obligatorios; calidad sobre velocidad; checklist por fase.                     | Arquitecto Empresarial     | Saltos de fase sin aprobación.           |
| RGO-007 | Diseño seguro, revisiones, pruebas OWASP, dependencias actualizadas.                               | Esp. Seguridad             | Hallazgos en análisis de seguridad.      |
| RGO-008 | Backups diario/semanal/mensual; pruebas de restauración; DRP.                                      | DevOps / DBA               | Fallo en respaldo o restauración.        |
| RGO-009 | Capacitación, prueba de versiones estables, plan de contingencia de versión.                       | Arquitecto / Backend       | Incompatibilidades o bugs de plataforma. |
| RGO-010 | Validar fuente oficial (INEI/RENIEC), carga verificada de datos maestros.                          | Arq. Datos / DBA           | Inconsistencias geográficas.             |
| RGO-011 | Diseño desacoplado, reintentos y degradación elegante; proveedor alterno.                          | Arquitecto / DevOps        | Errores de integración externos.         |
| RGO-012 | Documentación como fuente de verdad, pares, matriz RACI, traspaso ágil.                            | Product Owner              | Tareas bloqueadas por ausencia.          |
| RGO-013 | Alcance versionado por Roadmap; control de cambios; fuera de alcance explícito.                    | Product Owner / Arquitecto | Solicitudes fuera del MVP.               |
| RGO-014 | Cobertura medida en CI; bloqueo de merge bajo umbral; revisión de pruebas.                         | QA / DevOps                | Cobertura < 90% en CI.                   |
| RGO-015 | Diseño UX/UI cuidado, pruebas de usabilidad, accesibilidad WCAG.                                   | UX/UI / QA                 | Feedback negativo o abandono.            |
| RGO-016 | Fijar `ConnectionStrings__DefaultConnection` de forma **explícita** antes de todo comando EF Core; confirmación visual del puerto activo en `start-e2e.ps1`; nombre de BD distinto por ambiente (`exchange_platform` vs `exchange_test`) como segunda capa de defensa. | DevOps / Backend | Cualquier `dotnet ef database update` sin la variable definida. |
| RGO-017 | Aislamiento físico de BD en tres niveles (ADR-012): desarrollo y test corren sobre contenedores Docker; Supabase queda restringido a Producción. BD de test efímera (`tmpfs`). | Arquitecto / DevOps | Aparición de usuarios/objetos de prueba en el panel de administración de producción. |

## 6. Riesgos Críticos a Vigilar

Los de exposición **Alta** que definen la viabilidad del proyecto:

- **RGO-006 (desviación de SDD):** se evita con los gates de aprobación obligatorios de cada fase. Es el riesgo de proceso más relevante.
- **RGO-013 (scope creep):** se evita con el alcance versionado y la sección "Fuera de Alcance" de la Visión.
- **RGO-001, RGO-002, RGO-007 (identidad, fraude, seguridad):** núcleo de confianza; se mitigan con identidad no anónima, reputación, auditoría y OWASP.
- **RGO-003 (adopción):** condiciona el éxito del piloto; se mitiga con UX y difusión.

## 7. Seguimiento

| Actividad                             | Frecuencia              | Responsable            |
|---------------------------------------|-------------------------|------------------------|
| Revisión del registro de riesgos      | Por fase / quincenal    | Arquitecto Empresarial |
| Reevaluación de exposición            | Ante cambios relevantes | Product Owner          |
| Verificación de indicadores de alerta | Continua (CI/monitoreo) | DevOps / QA            |

## 8. Control de Cambios y Aprobación

| Versión | Fecha      | Autor             | Descripción                                                   |
|---------|------------|-------------------|---------------------------------------------------------------|
| 0.1.0   | 2026-06-03 | Equipo Enterprise | Versión semilla incluida en la Visión.                        |
| 0.2.0   | 2026-06-03 | Equipo Enterprise | Registro ampliado: escalas, exposición, planes y seguimiento. |
| 0.3.0   | 2026-07-13 | Equipo Enterprise | Se registran dos riesgos nuevos derivados de la implementación E2E y de ADR-012: **RGO-016** (migración accidental sobre producción por el fallback silencioso del `DesignTimeDbContextFactory` de EF Core al puerto 5432) y **RGO-017** (contaminación de la BD de producción con datos de prueba). Ambos con sus planes de mitigación. |

**Aprobación requerida (Fase 1):**

| Rol (RACI)                 | Responsabilidad                        | Estado    |
|----------------------------|----------------------------------------|-----------|
| Arquitecto Empresarial (A) | Aprueba el registro y estrategias      | Pendiente |
| Product Owner (R)          | Valida riesgos de negocio y exposición | Pendiente |
| Esp. Seguridad (C)         | Revisa riesgos de seguridad            | Pendiente |

> **Regla SDD:** Documento transversal; su mantenimiento es obligatorio en cada fase del ciclo de vida.
