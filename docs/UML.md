# UML.md
# Plataforma Inteligente de Intercambio de Objetos

> **Documento:** Modelado UML
> **Paso SDD:** 5 de 8 (UML) — **Fase SDLC:** 2 (Diseño)
> **Versión:** 1.2.0
> **Estado:** `PENDIENTE DE APROBACIÓN`
> **Fecha:** 2026-07-09
> **Autor:** Equipo Enterprise Senior (Arquitecto de Software / Especialista DDD)
> **Documentos padre:** VisionProyecto.md | Requisitos.md | ReglasNegocio.md | CasosDeUso.md | HistoriasUsuario.md | MatrizTrazabilidad.md
> **Convenciones:** Documentación y nomenclatura en español. Diagramas en ASCII art.

---

## Control de Versiones

| Versión | Fecha      | Autor                    | Cambios                                                                       |
|---------|------------|--------------------------|-------------------------------------------------------------------------------|
| 0.1.0   | 2026-06-03 | Equipo Enterprise        | Modelado inicial (Mermaid, modelo simple).                                    |
| 1.0.0   | 2026-06-03 | Equipo Enterprise Senior | Reescrito en español, diagramas ASCII y modelo de trueque (coherente con BD). |
| 1.1.0   | 2026-07-08 | Equipo Enterprise Senior | Diagramas de secuencia, componentes y despliegue actualizados: SQL Server → PostgreSQL/Supabase (ver ADR-010 en Arquitectura.md). |
| 1.2.0   | 2026-07-09 | Equipo Enterprise Senior | Diagrama de despliegue (sección 8) actualizado: VPS + Docker Compose → Vercel (frontend) + Render (backend), sin proxy Nginx. Ver ADR-011 en Arquitectura.md. |

---

## Tabla de Contenidos

1. Introducción
2. Diagrama de Casos de Uso
3. Diagrama de Clases (Dominio)
4. Diagrama de Secuencia (Flujo de Intercambio)
5. Diagrama de Actividades (Proceso de Intercambio)
6. Diagrama de Estados (Intercambio)
7. Diagrama de Componentes (Clean Architecture)
8. Diagrama de Despliegue (Docker)
9. Diagrama Entidad-Relación (preliminar)
10. Trazabilidad de los Diagramas
11. Aprobación

---

## 1. Introducción

Este documento modela el sistema con UML para traducir los casos de uso aprobados en estructura y comportamiento, antes de la implementación. Incluye los diagramas exigidos por el Prompt Master: Casos de Uso, Clases, Secuencia, Actividades, Estados, Componentes, Despliegue y Entidad-Relación.

El modelo de intercambio es de **trueque objeto-por-objeto**: el solicitante ofrece uno de sus objetos a cambio del objeto del propietario, y ambos confirman la entrega (coherente con `BD.md`, tabla Intercambios).

---

## 2. Diagrama de Casos de Uso

```
                    PLATAFORMA DE INTERCAMBIO DE OBJETOS
   ┌──────────────────────────────────────────────────────────────────┐
   │                                                                  │
   │   (UC-001) Registrar usuario                                     │
   │   (UC-002) Iniciar sesión                                        │
   │   (UC-003) Recuperar contraseña                                  │
   │   (UC-004) Cerrar sesión                                         │
   │   (UC-010) Publicar objeto                                       │
   │   (UC-011) Editar / eliminar objeto                              │
   │   (UC-012) Gestionar perfil                                      │
   │   (UC-020) Solicitar intercambio                                 │
   │   (UC-021) Aceptar / rechazar intercambio                        │
   │   (UC-022) Confirmar y calificar                                 │
   │   (UC-030) Buscar y filtrar objetos                              │
   │   (UC-040) Reportar objeto / usuario                             │
   │   (UC-041) Gestionar reportes                                    │
   │   (UC-080) Administrar plataforma                                │
   │   (UC-081) Ver dashboard e indicados                             │
   │   (UC-090) Consultar auditoría                                   │
   └──────────────────────────────────────────────────────────────────┘

   Actores y sus casos de uso:

   ┌──────────────┐
   │  Visitante   │──► UC-001, UC-002, UC-003, UC-030 (búsqueda pública)
   └──────────────┘

   ┌──────────────┐
   │   Usuario    │──► UC-004, UC-010, UC-011, UC-012,
   │              │    UC-020, UC-021, UC-022, UC-030,
   │              │    UC-040
   └──────────────┘

   ┌──────────────┐
   │  Moderador   │──► UC-041 (+ casos de Usuario)
   └──────────────┘

   ┌──────────────┐
   │Administrador │──► UC-080, UC-081, UC-090 (+ casos de Moderador)
   └──────────────┘

   ┌──────────────┐
   │   Sistema    │──► Genera notificaciones, registra auditoría,
   │ (secundario) │    aplica reglas de negocio
   └──────────────┘
```

---

## 3. Diagrama de Clases (Dominio)

Entidades del dominio en español. Todas las transaccionales heredan de `BaseEntity` (auditoría + soft delete).

```
┌───────────────────────────────┐
│          BaseEntity           │  (abstracta)
├───────────────────────────────┤
│ + Id : Guid                   │
│ + CreatedAt : DateTime        │
│ + CreatedBy : Guid            │
│ + UpdatedAt : DateTime?       │
│ + UpdatedBy : Guid?           │
│ + IsDeleted : bool            │
│ + DeletedAt : DateTime?       │
│ + DeletedBy : Guid?           │
└───────────────┬───────────────┘
                │ (herencia)
   ┌────────────┼───────────────┬───────────────┬──────────────┐
   ▼            ▼               ▼               ▼              ▼
┌────────┐ ┌─────────┐ ┌──────────────┐ ┌────────────┐ ┌──────────┐
│Usuario │ │ Objeto  │ │ Intercambio  │ │Calificacion│ │ Reporte  │
└────────┘ └─────────┘ └──────────────┘ └────────────┘ └──────────┘

┌───────────────────────────────┐        ┌───────────────────────────────┐
│            Usuario            │        │            Objeto             │
├───────────────────────────────┤        ├───────────────────────────────┤
│ + Nombres : string            │        │ + Titulo : string             │
│ + Apellidos : string          │        │ + Descripcion : string        │
│ + Email : string              │        │ + EstadoObjeto : EstadoObjeto │
│ + PasswordHash : string       │        │ + CondicionFisica : string    │
│ + Telefono : string           │        │ + Latitud : decimal?          │
│ + RolId : int                 │        │ + Longitud : decimal?         │
│ + DistritoId : int            │        ├───────────────────────────────┤
│ + CalificacionPromedio:decimal│        │ + Publicar()                  │
│ + TotalIntercambios : int     │        │ + Reservar()                  │
├───────────────────────────────┤        │ + MarcarIntercambiado()       │
│ + ActualizarReputacion()      │        │ + Suspender()                 │
│ + Bloquear()                  │        └───────────────────────────────┘
└───────────────────────────────┘

┌─────────────────────────────────────────────┐
│                 Intercambio                 │
├─────────────────────────────────────────────┤
│ + ObjetoSolicitadoId : Guid                 │
│ + ObjetoOfrecidoId : Guid                   │
│ + UsuarioSolicitanteId : Guid               │
│ + UsuarioPropietarioId : Guid               │
│ + EstadoIntercambio : EstadoIntercambio     │
│ + ConfirmacionSolicitante : bool            │
│ + ConfirmacionPropietario : bool            │
│ + FechaAceptacion : DateTime?               │
│ + FechaCompletado : DateTime?               │
├─────────────────────────────────────────────┤
│ + Aceptar()                                 │
│ + Rechazar()                                │
│ + ConfirmarRecepcion(usuarioId)             │
│ + Cancelar()                                │
└─────────────────────────────────────────────┘

┌────────────────────┐   ┌────────────────────┐   ┌────────────────────┐
│    Calificacion    │   │      Mensaje       │   │    Notificacion    │
├────────────────────┤   ├────────────────────┤   ├────────────────────┤
│ + IntercambioId    │   │ + IntercambioId    │   │ + UsuarioId        │
│ + CalificadorId    │   │ + RemitenteId      │   │ + Tipo             │
│ + CalificadoId     │   │ + Contenido        │   │ + Titulo           │
│ + Puntuacion : int │   │ + EnviadoEn        │   │ + Mensaje          │
│ + Comentario       │   │ + IsLeido : bool   │   │ + IsLeida : bool   │
└────────────────────┘   └────────────────────┘   └────────────────────┘

┌────────────────────┐   ┌────────────────────┐   ┌────────────────────┐
│      Favorito      │   │      Reporte       │   │  Geo (maestras)    │
├────────────────────┤   ├────────────────────┤   ├────────────────────┤
│ + UsuarioId        │   │ + ReportanteId     │   │ Departamento       │
│ + ObjetoId         │   │ + EntidadTipo      │   │   └─ Provincia     │
│ + AgregadoEn       │   │ + Motivo           │   │        └─ Distrito │
└────────────────────┘   │ + EstadoReporte    │   └────────────────────┘
                         └────────────────────┘

  RELACIONES PRINCIPALES (cardinalidad):
   Usuario     1 ── N  Objeto            (publica)
   Usuario     N ── 1  Rol               (tiene)
   Usuario     N ── 1  Distrito          (ubicado en)
   Objeto      1 ── N  ImagenObjeto      (posee)
   Objeto      N ── 1  Categoria         (clasificado)
   Intercambio N ─ 1   Objeto            (solicitado)      y N ─ 1 Objeto (ofrecido)
   Intercambio N ─ 1   Usuario           (solicitante)     y N ─ 1 Usuario (propietario)
   Intercambio 1 ── N  Calificacion      (genera, hasta 2)
   Intercambio 1 ── N  Mensaje           (contiene)
   Usuario     1 ── N  Favorito                              N ── 1  Objeto
   Usuario     1 ── N  Notificacion / Reporte / RefreshToken / AuditLog
```

---

## 4. Diagrama de Secuencia (Flujo de Intercambio)

Corresponde a UC-020 → UC-021 → UC-022 (trueque con doble confirmación). Reglas RN-020 a RN-032.

```
Solicitante   Propietario      API          Application      Domain         PostgreSQL    Notif.
    │              │            │                │              │               │               │
    │ POST /exchanges (UC-020)  │                │              │               │               │
    ├─────────────────────────► │                │              │               │               │
    │              │            │ CrearIntercambioCommand       │               │               │
    │              │            ├──────────────► │              │               │               │
    │              │            │                │ valida RN-022 (no propio)    │               │
    │              │            │                ├────────────► │               │               │
    │              │            │                │ ◄────────────┤ OK            │               │
    │              │            │                │ persiste Intercambio(Pendiente)              │
    │              │            │                ├──────────────────────────────►               │
    │              │            │                │ notifica Propietario (SolicitudRecibida)     │
    │              │            │                ├───────────────────────────────────────────►
    │ ◄─────────────────────────┤ 201 Created    │              │               │               │
    │              │            │                │              │               │               │
    │              │ PATCH /exchanges/{id}/accept (UC-021)      │               │               │
    │              ├──────────► │                │              │               │               │
    │              │            │ AceptarIntercambioCommand     │               │               │
    │              │            ├──────────────► │ valida RN-023 (solo propietario)             │
    │              │            │                │ estado=Aceptado, fechaAceptacion             │
    │              │            │                ├──────────────────────────────►               │
    │              │            │                │ notifica Solicitante (SolicitudAceptada)     │
    │              │            │                ├───────────────────────────────────────────►
    │              │ ◄──────────┤ 200 OK         │              │               │               │
    │              │            │                │              │               │               │
    │  (ambos confirman recepción - UC-022)      │              │               │               │
    │ PATCH /exchanges/{id}/confirm              │              │               │               │
    ├─────────────────────────► │ ConfirmarIntercambioCommand   │               │               │
    │              │            ├──────────────► │ marca confirmacion_solicitante=1             │
    │              ├──────────► │ (propietario confirma)         │               │              │
    │              │            │                │ si ambas confirmaciones=1 → estado=Completado
    │              │            │                ├──────────────────────────────►               │
    │              │            │                │ objetos → Intercambiado                      │
    │              │            │                │              │               │               │
    │ POST /ratings (UC-022)    │                │              │               │               │
    ├─────────────────────────► │ CalificarCommand              │               │               │
    │              │            ├──────────────► │ valida RN-030 (única), RN-031 (rango)        │
    │              │            │                │ guarda Calificacion + recalcula reputación   │
    │              │            │                ├──────────────────────────────►               │
    │ ◄─────────────────────────┤ 201 Created    │              │               │               │
```

---

## 5. Diagrama de Actividades (Proceso de Intercambio)

```
        ┌──────────┐
        │  Inicio  │
        └────┬─────┘
             ▼
   ┌─────────────────────┐
   │  Publicar objeto    │
   └─────────┬───────────┘
             ▼
   ┌─────────────────────┐
   │ Otro usuario busca  │
   │ y encuentra objeto  │
   └─────────┬───────────┘
             ▼
   ┌─────────────────────┐
   │Solicitar intercambio│
   │(ofrece objeto a     │
   │  cambio)            │
   └─────────┬───────────┘
             ▼
        ┌────────────────────┐
        │ ¿Propietario       │
        │   acepta?          │
        └───┬────────────┬───┘
         No │            │ Sí
            ▼            ▼
   ┌──────────────┐  ┌─────────────────────┐
   │  Rechazado   │  │ Coordinar entrega   │
   └──────┬───────┘  │ (vía mensajería)    │
          │          └─────────┬───────────┘
          │                    ▼
          │          ┌─────────────────────┐
          │          │ Ambos confirman     │
          │          │ recepción           │
          │          └─────────┬───────────┘
          │                    ▼
          │          ┌─────────────────────┐
          │          │ Intercambio         │
          │          │ Completado          │
          │          └─────────┬───────────┘
          │                    ▼
          │          ┌─────────────────────┐
          │          │ Calificar a la      │
          │          │ contraparte         │
          │          └─────────┬───────────┘
          │                    ▼
          │          ┌─────────────────────┐
          │          │Actualizar reputación│
          │          └─────────┬───────────┘
          ▼                    ▼
        ┌──────────────────────────┐
        │           Fin            │
        └──────────────────────────┘
```

---

## 6. Diagrama de Estados (Intercambio)

Ciclo de vida del estado de un intercambio (coherente con CK_Intercambios_Estado en BD.md).

```
                  crear solicitud (UC-020)
                          │
                          ▼
                   ┌─────────────┐
          rechazar │  Pendiente  │ aceptar (UC-021)
        ┌──────────┤             ├──────────┐
        ▼          └─────────────┘          ▼
  ┌───────────┐                       ┌─────────────┐
  │ Rechazado │                       │  Aceptado   │
  │ (final)   │                       └──────┬──────┘
  └───────────┘                              │ una parte confirma
        ▲                                    ▼
        │                          ┌──────────────────────┐
        │ cancelar (UC-004/INT)    │PendienteConfirmacion │
        │                          └──────────┬───────────┘
        │                                     │ ambas confirman
  ┌───────────┐                               ▼
  │ Cancelado │◄──────────┐            ┌──────────────┐
  │ (final)   │           └────────────┤  Completado  │
  └───────────┘  (desde cualquier      │   (final)    │
                  estado no final)     └──────────────┘

  Estados: Pendiente · Aceptado · PendienteConfirmacion ·
           Completado · Rechazado · Cancelado
```

---

## 7. Diagrama de Componentes (Clean Architecture)

```
┌──────────────────────────────────────────────────────────────┐
│  FRONTEND (React + TS - Feature Based)                       │
│  features: auth · users · objects · exchanges ·              │
│            notifications · admin                             │
└───────────────────────────┬──────────────────────────────────┘
                            │ HTTPS / REST /api/v1 + JWT
                            ▼
┌──────────────────────────────────────────────────────────────┐
│  BACKEND (.NET 10 - Clean Architecture)                      │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐    │
│  │ Capa API: Controllers, Middlewares, Filters, JWT     │    │
│  └───────────────────────┬──────────────────────────────┘    │
│                          │ MediatR                           │
│  ┌───────────────────────▼──────────────────────────────┐    │
│  │ Capa Application: Commands, Queries, Handlers,       │    │
│  │ Validators, Behaviors, DTOs, Mappings                │    │
│  └───────────────────────┬──────────────────────────────┘    │
│                          │ interfaces (DIP)                  │
│  ┌───────────────────────▼──────────────────────────────┐    │
│  │ Capa Domain: Entidades, ValueObjects, Reglas,        │    │
│  │ Enums, Domain Events, Interfaces                     │    │
│  └───────────────────────┬──────────────────────────────┘    │
│                          │ implementaciones                  │
│  ┌───────────────────────▼──────────────────────────────┐    │
│  │ Capa Infrastructure: EF Core, Repositories, UoW,     │    │
│  │ JwtService, Serilog, Servicios externos              │    │
│  └───────────────────────┬──────────────────────────────┘    │
└──────────────────────────┼───────────────────────────────────┘
                           │ EF Core (Npgsql) / TLS 5432 (pooler)
                           ▼
                  ┌─────────────────────────┐
                  │  PostgreSQL (Supabase)  │
                  │  servicio gestionado    │
                  └─────────────────────────┘

  Regla de dependencias: todas apuntan hacia el Dominio.
```

---

## 8. Diagrama de Despliegue (Vercel + Render)

```
┌─────────────────────────────┐
│  Navegador del usuario      │
│  (SPA React)                │
└──────────────┬──────────────┘
               │ HTTPS
               ▼
┌─────────────────────────────┐
│  Vercel                     │
│  Frontend (build Vite,      │
│  CDN, SSL automático)       │
└──────────────┬──────────────┘
               │ HTTPS — REST API (JSON)
               ▼
┌─────────────────────────────┐
│  Render                     │
│  Backend (contenedor Docker,│
│  ASP.NET Core .NET 10,      │
│  SSL automático)            │
└──────────────┬──────────────┘
               │ TLS :5432 (pooler)
               ▼
┌─────────────────────────────┐
│   Supabase (externo)        │
│   PostgreSQL gestionado     │
└─────────────────────────────┘
```

> Sin proxy Nginx ni Docker Compose en producción: Vercel y Render terminan TLS y enrutan cada uno de forma independiente (ADR-011, Arquitectura.md §8). Docker Compose se conserva solo para desarrollo local (Docker.md).

---

## 9. Diagrama Entidad-Relación (preliminar)

Vista conceptual. El modelo físico completo (tipos, PK/FK, índices, constraints) está en `BD.md`.

```
 Departamentos ──1:N── Provincias ──1:N── Distritos
                                              │ 1:N
                          ┌───────────────────┼───────────────────┐
                          ▼                                       ▼
                      Usuarios ──1:N── Objetos ──1:N── ImagenesObjeto
                          │   \             │ N:1
                          │    \            └── Categorias
                          │     \
                          │      └──1:N── Favoritos ──N:1── Objetos
                          │
                  ┌───────┴─────────┐
                  ▼                 ▼
            Intercambios       (Roles N:1)
              │  │  │
       ┌──────┘  │  └────────┐
       ▼         ▼           ▼
 Calificaciones Mensajes  (estados)

  Usuarios ──1:N── Notificaciones
  Usuarios ──1:N── Reportes
  Usuarios ──1:N── RefreshTokens
  Usuarios ──1:N── AuditLogs

  Intercambios referencia:
    - objeto_solicitado_id  → Objetos
    - objeto_ofrecido_id    → Objetos
    - usuario_solicitante_id → Usuarios
    - usuario_propietario_id → Usuarios
```

---

## 10. Trazabilidad de los Diagramas

| Diagrama          | Deriva de                       | Alimenta                          |
|-------------------|---------------------------------|-----------------------------------|
| Casos de Uso      | CasosDeUso.md (UC-XXX)          | Arquitectura.md                   |
| Clases            | ReglasNegocio.md, Glosario.md   | BD.md, Backend.md                 |
| Secuencia         | UC-020/021/022, RN-020..032     | Backend.md, API.md                |
| Actividades       | Flujo de intercambio            | Frontend.md                       |
| Estados           | CK_Intercambios_Estado (BD.md)  | Backend.md, API.md                |
| Componentes       | Arquitectura obligatoria        | Arquitectura.md, Backend.md       |
| Despliegue        | DevOps / Docker                 | Docker.md, Deployment.md          |
| Entidad-Relación  | Clases de dominio               | BD.md                             |

---

## 11. Aprobación

| Rol                          | Nombre            | Aprobación  | Fecha |
|------------------------------|-------------------|-------------|-------|
| Arquitecto de Software (A/R) | Equipo Enterprise | ☐ PENDIENTE | —     |
| Especialista DDD (C)         | Equipo Enterprise | ☐ PENDIENTE | —     |
| Arquitecto de Datos (C)      | Equipo Enterprise | ☐ PENDIENTE | —     |

---

> **GATE DE CALIDAD — FASE 2, PASO 5:**
> Este documento debe ser revisado y formalmente aprobado. Los modelos aquí definidos
> son la base para Arquitectura.md, BD.md y la implementación.

---

*Documento generado bajo la metodología SDD — Plataforma Inteligente de Intercambio de Objetos — Ayacucho, Perú.*
