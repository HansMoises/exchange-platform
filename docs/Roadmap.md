# Roadmap.md
# Plataforma Inteligente de Intercambio de Objetos

> **Documento:** Hoja de Ruta del Producto
> **Fase SDLC:** Transversal (planificación) — documento vivo
> **Versión:** 1.0.0
> **Estado:** `PENDIENTE DE APROBACIÓN`
> **Fecha:** 2026-06-03
> **Autor:** Equipo Enterprise Senior (Product Owner / Arquitecto Empresarial)
> **Documentos padre:** VisionProyecto.md | Requisitos.md | HistoriasUsuario.md | Arquitectura.md
> **Convenciones:** Documentación en español. Diagramas en ASCII.

---

## Control de Versiones

| Versión | Fecha      | Autor                    | Cambios          |
|---------|------------|--------------------------|------------------|
| 1.0.0   | 2026-06-03 | Equipo Enterprise Senior | Versión inicial. |

---

## Tabla de Contenidos

1. Principios del Roadmap
2. Resumen de Versiones
3. V1 — MVP (Validar el negocio)
4. V2 — Comunidad
5. V3 — Analítica
6. V4 — Inteligencia (IA)
7. V5 — Movilidad
8. Mapeo de Funcionalidades a Versión
9. Dependencias entre Versiones
10. Aprobación

---

## 1. Principios del Roadmap

| Principio              | Aplicación                                                           |
|------------------------|----------------------------------------------------------------------|
| Valor incremental      | Primero lo que valida el negocio; luego lo que lo enriquece.         |
| MVP protegido          | El alcance de V1 no se amplía sin control (mitiga RGO-013).          |
| Sin fechas rígidas     | Se ordena por valor y dependencia, no por calendario fijo.           |
| Escalable sin rediseño | Cada versión respeta la arquitectura aprobada (RNF-020).             |
| Guiado por SDD         | Cada nueva funcionalidad reinicia el ciclo (documentar→…→desplegar). |

---

## 2. Resumen de Versiones

```
V1 ──────► V2 ──────► V3 ──────► V4 ──────► V5
MVP        Comunidad  Analítica  IA         Movilidad
│          │          │          │          │
Intercambio Chat,      Dashboard  Recomenda- App móvil
seguro     favoritos, avanzado,  ciones,    Android/iOS
geolocaliz. reputación KPIs       fraude
```

| Versión | Tema         | Objetivo principal                          | Alcance geográfico       |
|---------|--------------|---------------------------------------------|--------------------------|
| V1      | MVP          | Validar el modelo de intercambio seguro.    | Ayacucho                 |
| V2      | Comunidad    | Enriquecer interacción y confianza.         | Región Ayacucho          |
| V3      | Analítica    | Métricas y toma de decisiones.              | Todo el Perú             |
| V4      | Inteligencia | Recomendaciones y detección de fraude (IA). | Todo el Perú             |
| V5      | Movilidad    | Aplicación móvil nativa.                    | Nacional / Internacional |

---

## 3. V1 — MVP (Validar el negocio)

**Objetivo:** demostrar que ciudadanos de Ayacucho pueden intercambiar objetos de forma segura, identificada y geolocalizada, de extremo a extremo.

**Alcance / Funcionalidades:**

| Funcionalidad                          | Historias / Casos                         |
|----------------------------------------|-------------------------------------------|
| Registro, login, recuperación, perfil  | HU-001..003, HU-010 / UC-001..004, UC-012 |
| Publicación y gestión de objetos       | HU-020, HU-021 / UC-010, UC-011           |
| Búsqueda y filtros (incl. geográficos) | HU-030 / UC-030, UC-031                   |
| Solicitud y gestión de intercambio     | HU-040, HU-041 / UC-020, UC-021           |
| Confirmación y calificación            | HU-042 / UC-022                           |
| Geolocalización (UBIGEO completo)      | RF-100, RF-101                            |
| Reportes básicos y panel admin mínimo  | UC-040, UC-080                            |
| Auditoría y seguridad (JWT, OWASP)     | RNF-001..005, RN-062/063                  |

**Criterios de salida:** flujo de intercambio completo funcionando, no anónimo y trazable; cobertura ≥90%; OWASP verificado; piloto operativo en Ayacucho (CE-001, CE-002, CE-007).

---

## 4. V2 — Comunidad

**Objetivo:** profundizar la interacción y la confianza entre usuarios.

**Alcance / Funcionalidades:**

| Funcionalidad                         | Historias / Casos   |
|---------------------------------------|---------------------|
| Chat / mensajería entre partes        | HU-070 / UC-060     |
| Favoritos                             | HU-060 / UC-050     |
| Notificaciones enriquecidas           | HU-080 / UC-070     |
| Reputación avanzada                   | Evolución de RN-032 |
| Mejoras de moderación                 | UC-041              |
| Integración de mapas (Leaflet/Google) | Evolución de RF-033 |

**Criterios de salida:** mensajería operativa, favoritos y notificaciones en uso; mapas mostrando ubicación; expansión a la región Ayacucho.

---

## 5. V3 — Analítica

**Objetivo:** dotar a la administración de métricas para decidir y crecer.

**Alcance / Funcionalidades:**

| Funcionalidad                          | Referencia      |
|----------------------------------------|-----------------|
| Dashboard administrativo avanzado      | UC-081          |
| Reportes y exportaciones               | KPIs de negocio |
| Indicadores y KPIs en tiempo casi real | KPI-001..007    |
| Observabilidad (Grafana/Prometheus)    | Arquitectura.md |

**Criterios de salida:** dashboard con KPIs clave; observabilidad activa; preparación para escala nacional.

---

## 6. V4 — Inteligencia (IA)

**Objetivo:** incorporar capacidades inteligentes (no presentes en el MVP).

**Alcance / Funcionalidades:**

| Funcionalidad                          | Naturaleza            |
|----------------------------------------|-----------------------|
| Recomendación de objetos/intercambios  | IA / ML               |
| Sugerencias de intercambio (matching)  | IA / ML               |
| Detección de fraude                    | IA / ML               |
| Clasificación automática de objetos    | IA / ML               |
| Búsquedas inteligentes                 | IA / ML               |

**Criterios de salida:** modelos integrados tras interfaces (sin acoplar el core); mejora medible de la experiencia y la seguridad.

> La IA se diseña desacoplada: el dominio no depende de ella (preparación dejada desde la arquitectura).

---

## 7. V5 — Movilidad

**Objetivo:** llevar la plataforma a aplicaciones móviles nativas.

**Alcance / Funcionalidades:**

| Funcionalidad                 | Naturaleza               |
|-------------------------------|--------------------------|
| App móvil Android             | Cliente nativo / híbrido |
| App móvil iOS                 | Cliente nativo / híbrido |
| Notificaciones push           | Servicio externo         |
| Reutilización de la API REST  | Sin reescribir backend   |

**Criterios de salida:** apps publicadas consumiendo la misma API; experiencia móvil completa; alcance nacional/internacional.

---

## 8. Mapeo de Funcionalidades a Versión

| Funcionalidad              | V1 | V2 | V3 | V4 | V5 |
|----------------------------|----|----|----|----|----|
| Registro / Login / Perfil  | X  |    |    |    |    |
| Publicar objetos           | X  |    |    |    |    |
| Búsqueda y filtros geo     | X  |    |    |    |    |
| Intercambio + calificación | X  |    |    |    |    |
| Reportes / admin mínimo    | X  |    |    |    |    |
| Chat / mensajería          |    | X  |    |    |    |
| Favoritos                  |    | X  |    |    |    |
| Notificaciones avanzadas   |    | X  |    |    |    |
| Mapas                      |    | X  |    |    |    |
| Dashboard avanzado / KPIs  |    |    | X  |    |    |
| Observabilidad             |    |    | X  |    |    |
| Recomendaciones / fraude   |    |    |    | X  |    |
| App móvil                  |    |    |    |    | X  |

---

## 9. Dependencias entre Versiones

```
V1 (MVP)
  └─► es base de todo: API, dominio, datos, seguridad.

V2 (Comunidad)
  └─► requiere V1 (intercambios y notificaciones base).

V3 (Analítica)
  └─► requiere datos de uso generados en V1/V2.

V4 (IA)
  └─► requiere volumen de datos de V1-V3 para entrenar/medir.

V5 (Movilidad)
  └─► requiere API estable (V1+) y, deseablemente, V2 para experiencia completa.
```

Regla: cada versión **reinicia el ciclo SDD** (documentar→validar→diseñar→implementar→probar→desplegar) para su alcance.

---

## 10. Aprobación

| Rol                          | Nombre            | Aprobación  | Fecha |
|------------------------------|-------------------|-------------|-------|
| Product Owner                | Equipo Enterprise | ☐ PENDIENTE | —     |
| Arquitecto Empresarial Senior| Equipo Enterprise | ☐ PENDIENTE | —     |
| Arquitecto de Software       | Equipo Enterprise | ☐ PENDIENTE | —     |

---

> **NOTA:**
> Este roadmap es un documento vivo: se revisa al cierre de cada versión.
> El alcance de V1 (MVP) está protegido; nuevas ideas se asignan a versiones futuras,
> no se inyectan al MVP (control de scope creep — RGO-013).

---

*Documento generado bajo la metodología SDD — Plataforma Inteligente de Intercambio de Objetos — Ayacucho, Perú.*
