# DatosGeograficos.md
# Plataforma Inteligente de Intercambio de Objetos

> **Documento:** Datos Geográficos (UBIGEO)
> **Fase SDLC:** 2 (Diseño) — documento complementario
> **Versión:** 1.0.0
> **Estado:** `PENDIENTE DE APROBACIÓN`
> **Fecha:** 2026-06-03
> **Autor:** Equipo Enterprise Senior (Arquitecto de Datos / DBA)
> **Documentos padre:** BD.md | Arquitectura.md | ReglasNegocio.md | Requisitos.md
> **Convenciones:** Documentación en español. Diagramas en ASCII.

---

## Control de Versiones

| Versión | Fecha      | Autor                    | Cambios          |
|---------|------------|--------------------------|------------------|
| 1.0.0   | 2026-06-03 | Equipo Enterprise Senior | Versión inicial. |

---

## Tabla de Contenidos

1. Propósito
2. Estándar UBIGEO
3. Modelo Jerárquico
4. Fuente Oficial de Datos
5. Estrategia de Carga (Seed)
6. Cobertura por Fases
7. Uso en el Sistema
8. Validaciones de Coherencia
9. Datos de Referencia (Ayacucho)
10. Trazabilidad y Aprobación

---

## 1. Propósito

Los datos geográficos permiten:
- Registrar la **ubicación** verificable de usuarios y objetos (no anónimo — RN-003).
- **Filtrar** y buscar objetos por departamento, provincia y distrito (RF-031).
- Preparar la búsqueda por **cercanía** y mapas (RF-033, V2).
- Habilitar el **escalamiento nacional sin rediseño**: nuevas regiones se incorporan como datos, no como código (RNF-021, ADR-007).

Se gestionan como **datos maestros**: estables, jerárquicos, de solo referencia.

---

## 2. Estándar UBIGEO

El proyecto adopta el **UBIGEO** (Ubicación Geográfica), código oficial del INEI/RENIEC que identifica de forma única cada circunscripción del Perú.

### 2.1 Codificación

El UBIGEO es un código numérico jerárquico de 6 dígitos:

```
   D D   P P   I I
   │ │   │ │   │ │
   │ │   │ │   └─┴── Distrito  (2 dígitos)
   │ │   └─┴──────── Provincia (2 dígitos)
   └─┴────────────── Departamento (2 dígitos)

   Ejemplo: 050101
     05    → Departamento de Ayacucho
     0501  → Provincia de Huamanga
     050101 → Distrito de Ayacucho
```

| Nivel        | Dígitos | Longitud código | Ejemplo |
|--------------|---------|-----------------|---------|
| Departamento | 1-2     | 2               | 05      |
| Provincia    | 3-4     | 4               | 0501    |
| Distrito     | 5-6     | 6               | 050101  |

---

## 3. Modelo Jerárquico

```
┌─────────────────────────────────────────────────┐
│  PERÚ                                           │
│                                                 │
│  Departamento (25)         ubigeo CHAR(2)       │
│       │ 1:N                                     │
│       ▼                                         │
│  Provincia (196)           ubigeo CHAR(4)       │
│       │ 1:N                                     │
│       ▼                                         │
│  Distrito (1,874)          ubigeo CHAR(6)       │
│       │ 1:N                  + latitud/longitud │
│       ▼                                         │
│  Usuarios / Objetos (ubicados en un distrito)   │
└─────────────────────────────────────────────────┘
```

Relaciones (implementadas en `BD.md`):
- Una **Provincia** pertenece a un **Departamento** (FK + coherencia de ubigeo).
- Un **Distrito** pertenece a una **Provincia** (FK + coherencia de ubigeo).
- **Usuario** y **Objeto** referencian departamento_id, provincia_id y distrito_id.

---

## 4. Fuente Oficial de Datos

| Aspecto          | Detalle                                                        |
|------------------|----------------------------------------------------------------|
| Fuente primaria  | INEI — Instituto Nacional de Estadística e Informática.        |
| Fuente alterna   | RENIEC; datasets públicos de UBIGEO (gobierno / repositorios). |
| Formato de carga | CSV/JSON importado a las tablas maestras vía seed.             |
| Coordenadas      | Latitud/longitud por distrito (centroide), cuando disponible.  |
| Actualización    | Baja frecuencia; revisión ante cambios oficiales de UBIGEO.    |

> **Riesgo asociado:** RGO-010 (datos geográficos incompletos/inexactos). Mitigación: validar contra fuente oficial antes de la carga.

---

## 5. Estrategia de Carga (Seed)

```
Orden de carga (respetando dependencias FK):
  1. Departamentos  (25 registros)
  2. Provincias     (196 registros)
  3. Distritos      (1,874 registros)

Método:
  - EF Core Data Seeding o script SQL de migración inicial.
  - Idempotente: si el dato existe (por ubigeo único), no se duplica.
  - Se ejecuta una sola vez al inicializar la base de datos.
```

Ejemplo de estructura del archivo de origen (CSV):

```
ubigeo,nombre,nivel,ubigeo_padre
05,Ayacucho,departamento,
0501,Huamanga,provincia,05
050101,Ayacucho,distrito,0501
050102,Acocro,distrito,0501
```

---

## 6. Cobertura por Fases

Aunque el MVP opera en Ayacucho, **se carga el UBIGEO completo del Perú desde V1**. Así, escalar a otra región es solo habilitarla a nivel de negocio, sin migraciones de datos ni cambios de esquema.

| Fase | Alcance operativo  | Datos cargados                               |
|------|--------------------|----------------------------------------------|
| 1    | Ayacucho (MVP)     | UBIGEO completo Perú                         |
| 2    | Región Ayacucho    | Sin cambios (ya está)                        |
| 3    | Todo el Perú       | Sin cambios (ya está)                        |
| 4    | Internacional      | Extensión del modelo (país como nuevo nivel) |

> La decisión de cargar todo el Perú desde el inicio materializa el ADR-007 y el criterio de éxito CE-005 (escalar sin rediseño).

---

## 7. Uso en el Sistema

| Funcionalidad             | Uso de datos geográficos                                       |
|---------------------------|----------------------------------------------------------------|
| Registro de usuario       | Selección en cascada Departamento→Provincia→Distrito (UC-001). |
| Publicación de objeto     | Ubicación del objeto (UC-010).                                 |
| Búsqueda y filtros        | Filtrar por departamento/provincia/distrito (RF-031).          |
| Búsqueda por cercanía     | Usar latitud/longitud del distrito (RF-033, V2).               |
| Selectores en formularios | Endpoints públicos /geo/* (API.md §13).                        |

Flujo de selección en cascada (UX):

```
[Selecciona Departamento] ──► carga Provincias del departamento
        │
        ▼
[Selecciona Provincia]    ──► carga Distritos de la provincia
        │
        ▼
[Selecciona Distrito]     ──► ubicación completa registrada
```

---

## 8. Validaciones de Coherencia

Implementan RN-050 y RN-051.

| Validación                                               | Dónde                         | 
|----------------------------------------------------------|-------------------------------|
| La provincia debe pertenecer al departamento elegido.    | Backend + BD (FK)             |
| El distrito debe pertenecer a la provincia elegida.      | Backend + BD (FK)             |
| Los tres niveles son obligatorios para usuario y objeto. | Backend + BD                  |
| El ubigeo del hijo debe iniciar con el del padre.        | Validación de datos al cargar |
| Solo se aceptan IDs existentes en las tablas maestras.   | FK + validación               |

---

## 9. Datos de Referencia (Ayacucho)

Departamento de Ayacucho (ubigeo 05) y sus 11 provincias:

| Ubigeo | Provincia            |
|--------|----------------------|
| 0501   | Huamanga             |
| 0502   | Cangallo             |
| 0503   | Huanca Sancos        |
| 0504   | Huanta               |
| 0505   | La Mar               |
| 0506   | Lucanas              |
| 0507   | Parinacochas         |
| 0508   | Páucar del Sara Sara |
| 0509   | Sucre                |
| 0510   | Víctor Fajardo       |
| 0511   | Vilcas Huamán        |

Ejemplo de distritos de la provincia de Huamanga (0501):

| Ubigeo  | Distrito       |
|---------|----------------|
| 050101  | Ayacucho       |
| 050102  | Acocro         |
| 050103  | Acos Vinchos   |
| 050104  | Carmen Alto    |
| 050105  | Chiara         |
| ...     | (y los demás)  |

> Los valores exactos y completos se cargan desde la fuente oficial (sección 4). Esta tabla es referencial.

---

## 10. Trazabilidad y Aprobación

### 10.1 Trazabilidad

| Elemento                       | Responde a               |
|--------------------------------|--------------------------|
| Modelo jerárquico UBIGEO       | RN-050, RN-051           |
| Carga completa del Perú en V1  | RNF-021, ADR-007, CE-005 |
| Ubicación obligatoria          | RN-003 (no anónimo)      |
| Endpoints /geo/*               | API.md §13               |
| Tablas maestras                | BD.md §5                 |
| Coordenadas para cercanía      | RF-033                   |

### 10.2 Aprobación

| Rol                        | Nombre            | Aprobación  | Fecha |
|----------------------------|-------------------|-------------|-------|
| Arquitecto de Datos Senior | Equipo Enterprise | ☐ PENDIENTE | —     |
| DBA Senior                 | Equipo Enterprise | ☐ PENDIENTE | —     |
| Backend Developer Senior   | Equipo Enterprise | ☐ PENDIENTE | —     |

---

> **GATE DE CALIDAD:**
> La carga del UBIGEO oficial es prerrequisito del seed inicial de la base de datos.
> Sin datos geográficos válidos, el registro de usuarios y la publicación de objetos no funcionan.

---

*Documento generado bajo la metodología SDD — Plataforma Inteligente de Intercambio de Objetos — Ayacucho, Perú.*
