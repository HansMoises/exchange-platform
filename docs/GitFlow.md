# GitFlow.md
# Plataforma Inteligente de Intercambio de Objetos

> **Documento:** Estrategia de Ramas y Flujo de Trabajo (GitFlow)
> **Fase SDLC:** 2 (Diseño) — documento de proceso / DevOps
> **Versión:** 1.0.0
> **Estado:** `PENDIENTE DE APROBACIÓN`
> **Fecha:** 2026-06-03
> **Autor:** Equipo Enterprise Senior (Especialista DevOps / Arquitecto)
> **Documentos padre:** Convenciones.md | Arquitectura.md
> **Convenciones:** Documentación en español. Diagramas en ASCII.

---

## Control de Versiones

| Versión | Fecha      | Autor                    | Cambios          |
|---------|------------|--------------------------|------------------|
| 1.0.0   | 2026-06-03 | Equipo Enterprise Senior | Versión inicial. |

---

## Tabla de Contenidos

1. Modelo de Ramas
2. Descripción de Ramas
3. Ciclo de Vida de una Funcionalidad
4. Proceso de Release
5. Proceso de Hotfix
6. Reglas de Pull Request
7. Protección de Ramas
8. Versionado Semántico
9. Integración con SDD y CI/CD
10. Aprobación

---

## 1. Modelo de Ramas

```
  main      ●─────────────●─────────────────●──────────● (producción, tags)
            │             │                 │          │
            │         release/1.0.0      hotfix/1.0.1  │
            │           │   │              │   │       │
  develop   ●───●───●───●───●──●───●───●───●───●───●───● (integración)
                │       │      │       │
       feature/auth     │  feature/objetos
                        │
                 feature/intercambios

  Flujo: feature/* ─► develop ─► release/* ─► main (+ tag)
         hotfix/* ─► main + develop
```

---

## 2. Descripción de Ramas

| Rama       | Origen   | Destino merge      | Propósito                                            | Vida       |
|------------|----------|--------------------|------------------------------------------------------|------------|
| main       | —        | —                  | Código en producción. Siempre estable y desplegable. | Permanente |
| develop    | main     | —                  | Integración de funcionalidades terminadas.           | Permanente |
| feature/*  | develop  | develop            | Desarrollo de una funcionalidad.                     | Temporal   |
| release/*  | develop  | main + develop     | Estabilización de una versión a publicar.            | Temporal   |
| hotfix/*   | main     | main + develop     | Corrección urgente en producción.                    | Temporal   |
| bugfix/*   | develop  | develop            | Corrección de defecto detectado en develop.          | Temporal   |

### Nombres de Rama (kebab-case)

| Tipo     | Patrón                       | Ejemplo                       |
|----------|------------------------------|-------------------------------|
| feature  | feature/{descripcion}        | feature/publicar-objeto       |
| bugfix   | bugfix/{descripcion}         | bugfix/validacion-imagenes    |
| release  | release/{version}            | release/1.0.0                 |
| hotfix   | hotfix/{version}             | hotfix/1.0.1                  |

---

## 3. Ciclo de Vida de una Funcionalidad

En coherencia con SDD: una funcionalidad **solo inicia** si su documentación está aprobada.

```
1. La documentación de la funcionalidad está aprobada (gate SDD).
2. Crear rama:        git checkout develop && git pull
                      git checkout -b feature/publicar-objeto
3. Desarrollar:       commits Conventional Commits (feat:, fix:, test:...).
4. Pruebas:           escribir y pasar pruebas (cobertura del ámbito).
5. Push:              git push -u origin feature/publicar-objeto
6. Pull Request:      hacia develop. CI ejecuta build + tests + cobertura.
7. Code Review:       aprobación (técnico, funcional, seguridad, arquitectura).
8. Merge:             a develop (squash o merge según política).
9. Limpieza:          eliminar la rama feature.
```

---

## 4. Proceso de Release

```
1. Desde develop estable:
     git checkout -b release/1.0.0
2. Estabilización:
     - Solo correcciones y ajustes finales (no nuevas features).
     - Actualizar versión (1.0.0), CHANGELOG y documentación.
3. Validación:
     - Pruebas completas, checklist "Antes de Desplegar" (ChecklistCalidad.md).
4. Merge a main:
     git checkout main && git merge --no-ff release/1.0.0
     git tag -a v1.0.0 -m "Release 1.0.0"
5. Merge de vuelta a develop (para no perder los ajustes del release).
6. Despliegue de main según Deployment.md.
7. Eliminar la rama release.
```

---

## 5. Proceso de Hotfix

Para corregir un fallo crítico en producción sin esperar al siguiente release.

```
1. Desde main:
     git checkout -b hotfix/1.0.1
2. Corregir el fallo + prueba que lo cubra.
3. Actualizar versión PATCH (1.0.0 -> 1.0.1).
4. Merge a main + tag:
     git checkout main && git merge --no-ff hotfix/1.0.1
     git tag -a v1.0.1 -m "Hotfix 1.0.1"
5. Merge a develop (para que la corrección no se pierda).
6. Desplegar main.
7. Eliminar la rama hotfix.
```

---

## 6. Reglas de Pull Request

| Regla                          | Detalle                                                           |
|--------------------------------|-------------------------------------------------------------------|
| Título                         | Sigue Conventional Commits. Ej: "feat(objetos): publicar objeto". |
| Descripción                    | Qué cambia, por qué, documentación/issue asociada.                |
| Tamaño                         | PRs pequeños y enfocados (más fáciles de revisar).                |
| CI verde                       | Build, tests y cobertura deben pasar antes de revisar.            |
| Revisores                      | Al menos 1 aprobación (técnico/arquitectura).                     |
| Checklist                      | Cumple DoD (Testing.md) y convenciones (Convenciones.md).         |
| Sin conflictos                 | Rama actualizada con develop antes del merge.                     |

Plantilla de PR (resumen):

```
[Qué cambia]
[Por qué — documento/regla asociada]
[Cómo se probó]
[Checklist]
- [ ] Cumple DoD
- [ ] Pruebas pasando y cobertura OK
- [ ] Documentación actualizada
- [ ] Cumple convenciones
```

---

## 7. Protección de Ramas

| Rama    | Reglas de protección                                                                     |
|---------|------------------------------------------------------------------------------------------|
| main    | Sin push directo. Solo vía PR desde release/hotfix. CI obligatorio. Requiere aprobación. |
| develop | Sin push directo. Solo vía PR. CI obligatorio. Requiere aprobación.                      |
| feature | Libre para el autor; CI corre en cada push.                                              |

Configurado en GitHub (Branch Protection Rules).

---

## 8. Versionado Semántico

Formato **MAJOR.MINOR.PATCH** (SemVer).

| Componente | Cuándo se incrementa                                  | Ejemplo        |
|------------|-------------------------------------------------------|----------------|
| MAJOR      | Cambios incompatibles (breaking changes).             | 1.0.0 → 2.0.0  |
| MINOR      | Nueva funcionalidad compatible.                       | 1.0.0 → 1.1.0  |
| PATCH      | Corrección de errores compatible.                     | 1.0.0 → 1.0.1  |

Cada release en `main` se etiqueta con su versión (`v1.0.0`).

---

## 9. Integración con SDD y CI/CD

| Evento Git                  | Disparador CI/CD (CICD.md)                       |
|-----------------------------|--------------------------------------------------|
| Push a feature/*            | Build + pruebas (feedback rápido al autor).      |
| PR hacia develop            | Build + pruebas + cobertura (quality gate).      |
| Merge a develop             | Build + pruebas + (opcional) deploy a Testing.   |
| Merge a main (release)      | Build + pruebas + deploy a Staging/Production.   |

**Coherencia con SDD:** ninguna rama feature se abre sin documentación aprobada; ningún merge a `main` ocurre sin pasar el gate de calidad. El flujo Git refleja el flujo SDLC (Análisis→…→Despliegue).

---

## 10. Aprobación

| Rol                    | Nombre            | Aprobación  | Fecha |
|------------------------|-------------------|-------------|-------|
| Especialista DevOps    | Equipo Enterprise | ☐ PENDIENTE | —     |
| Arquitecto de Software | Equipo Enterprise | ☐ PENDIENTE | —     |
| Backend Developer Senior| Equipo Enterprise| ☐ PENDIENTE | —     |

---

> **GATE DE CALIDAD:**
> Este flujo de ramas es obligatorio. `main` siempre debe estar desplegable.
> El detalle del pipeline que se ejecuta en cada evento está en `CICD.md`.

---

*Documento generado bajo la metodología SDD — Plataforma Inteligente de Intercambio de Objetos — Ayacucho, Perú.*
