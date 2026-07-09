# Prototipo.md
# Plataforma Inteligente de Intercambio de Objetos

> **Documento:** Prototipo / Wireframes
> **Fase SDLC:** 2 (Diseño) — documento complementario
> **Versión:** 1.0.0
> **Estado:** `PENDIENTE DE APROBACIÓN`
> **Fecha:** 2026-06-03
> **Autor:** Equipo Enterprise Senior (Especialista UX/UI)
> **Documentos padre:** UX.md | UI.md | Arquitectura.md | CasosDeUso.md
> **Convenciones:** Documentación en español. Wireframes en ASCII (baja fidelidad).

---

## Control de Versiones

| Versión | Fecha      | Autor                    | Cambios          |
|---------|------------|--------------------------|------------------|
| 1.0.0   | 2026-06-03 | Equipo Enterprise Senior | Versión inicial. |

---

## Tabla de Contenidos

1. Alcance del Prototipo
2. Mapa de Navegación
3. Landing
4. Login y Registro
5. Búsqueda y Resultados
6. Detalle de Objeto
7. Publicar Objeto
8. Intercambios y Detalle de Intercambio
9. Perfil y Dashboard de Usuario
10. Panel de Administración
11. Aprobación

---

## 1. Alcance del Prototipo

Wireframes de **baja fidelidad** de las pantallas clave del MVP. Muestran disposición y navegación, no el diseño final (color/tipografía se aplican con UI.md). Sirven de referencia para implementar el frontend (Frontend.md).

Convención de los wireframes:
- `[ ... ]` botón
- `( ... )` campo de entrada
- `▼` selector / desplegable
- `[img]` imagen / miniatura
- `★` reputación

---

## 2. Mapa de Navegación

```
Landing ──► Login/Registro ──► Dashboard
   │                              │
   ├─► Búsqueda ──► Resultados ──► Detalle de objeto ──► Solicitar intercambio
   │                                                          │
   └─► (público)                                              ▼
                                                       Intercambios ──► Detalle
                                                          │
                                            Perfil ◄──────┘
   Admin (rol) ──► Dashboard admin ──► Usuarios / Objetos / Reportes / Auditoría
```

---

## 3. Landing

```
┌─────────────────────────────────────────────────────────────┐
│ LOGO  Intercambia          Buscar   [Ingresar] [Registro]   │ ← Navbar
├─────────────────────────────────────────────────────────────┤
│                                                             │
│        Intercambia lo que ya no usas.                       │ ← Hero
│        Economía circular para Ayacucho.                     │
│                                                             │
│        ( Buscar objetos...                 ) [Buscar]       │ ← SearchBox
│                                                             │
├─────────────────────────────────────────────────────────────┤
│   ¿Cómo funciona?                                           │
│   [1] Publica   [2] Encuentra   [3] Intercambia             │
├─────────────────────────────────────────────────────────────┤
│   Objetos destacados                                        │
│   ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐               │
│   │[img]   │ │[img]   │ │[img]   │ │[img]   │  ← ObjectCard │
│   │Título  │ │Título  │ │Título  │ │Título  │               │
│   │4.5     │ │4.0     │ │5.0     │ │4.2     │               │
│   └────────┘ └────────┘ └────────┘ └────────┘               │
├─────────────────────────────────────────────────────────────┤
│ Footer: Acerca · Términos · Contacto                        │
└─────────────────────────────────────────────────────────────┘
```

---

## 4. Login y Registro

### 4.1 Login (UC-002)

```
┌─────────────────────────────────────┐
│            Iniciar sesión           │
├─────────────────────────────────────┤
│  Correo                             │
│  ( ejemplo@correo.com )             │
│                                     │
│  Contraseña                         │
│  ( •••••••• )                       │
│                                     │
│  [ Iniciar sesión ]                 │
│                                     │
│  ¿Olvidaste tu contraseña?          │
│  ¿No tienes cuenta? Regístrate      │
└─────────────────────────────────────┘
```

### 4.2 Registro (UC-001)

```
┌─────────────────────────────────────┐
│            Crear cuenta             │
├─────────────────────────────────────┤
│  Nombres        ( ............... ) │
│  Apellidos      ( ............... ) │
│  Correo         ( ............... ) │
│  Teléfono       ( ............... ) │
│  Contraseña     ( ............... ) │
│  Confirmar      ( ............... ) │
│                                     │
│  Ubicación                          │
│  Departamento   ▼ Ayacucho          │ ← cascada geográfica
│  Provincia      ▼ Huamanga          │
│  Distrito       ▼ Ayacucho          │
│                                     │
│  [ Registrarme ]                    │
└─────────────────────────────────────┘
```

---

## 5. Búsqueda y Resultados (UC-030)

```
┌──────────────────────────────────────────────────────────┐
│ Navbar                                                   │
├──────────────┬───────────────────────────────────────────┤
│ FILTROS      │  ( Buscar...                   ) [Buscar] │
│              │  Orden: ▼ Más recientes                   │
│ Categoría ▼  │  ┌────────┐ ┌────────┐ ┌────────┐         │
│ Depart.   ▼  │  │[img]   │ │[img]   │ │[img]   │         │
│ Provincia ▼  │  │Título  │ │Título  │ │Título  │         │
│ Distrito  ▼  │  │Cat·Ubi │ │Cat·Ubi │ │Cat·Ubi │         │
│ Estado    ▼  │  │4.5 Disp│ │4.0 Disp│ │5.0 Disp│         │
│              │  └────────┘ └────────┘ └────────┘         │
│ [Aplicar]    │  ┌────────┐ ┌────────┐ ┌────────┐         │
│              │  │ ...    │ │ ...    │ │ ...    │         │
│              │  └────────┘ └────────┘ └────────┘         │
│              │  ◄ 1 2 3 ... ►        ← Paginación        │
└──────────────┴───────────────────────────────────────────┘
```

Estado vacío: "No encontramos objetos con esos filtros. Prueba ampliar la búsqueda."

---

## 6. Detalle de Objeto (UC-030 → UC-020)

```
┌──────────────────────────────────────────────────────────┐
│ ◄ Volver                                                 │
├───────────────────────────┬──────────────────────────────┤
│  ┌─────────────────────┐  │  Bicicleta rodado 26         │
│  │      [img grande]   │  │  Categoría: Deportes         │
│  └─────────────────────┘  │  Estado: Disponible          │
│  [img][img][img][img]     │  Condición: Bueno            │
│   ← Gallery (miniaturas)  │  Ubicación: Ayacucho         │
│                           │                              │
│                           │  Descripción:                │
│                           │  Bicicleta en buen estado... │
│                           │                              │
│                           │  Propietario:                │
│                           │  [av] Rosa Q.   4.5          │
│                           │                              │
│                           │  [ Proponer intercambio ]    │ ← si no es propio
│                           │  [ ♥ Favorito ] [ Reportar ] │
└───────────────────────────┴──────────────────────────────┘
```

Modal "Proponer intercambio":

```
┌──────────────────────────────────────┐
│  Proponer intercambio                │
├──────────────────────────────────────┤
│  Ofreces a cambio:                   │
│  ▼ (selecciona uno de tus objetos)   │
│                                      │
│  Mensaje (opcional)                  │
│  ( ............................. )   │
│                                      │
│  [ Cancelar ]   [ Enviar solicitud ] │
└──────────────────────────────────────┘
```

---

## 7. Publicar Objeto (UC-010)

```
┌───────────────────────────────────────────────────────┐
│  Publicar objeto                                      │
├───────────────────────────────────────────────────────┤
│  Título        ( ............................ )       │
│  Descripción   ( ............................ )       │
│                ( ............................ )       │
│  Categoría     ▼ Deportes                             │
│  Condición     ▼ Bueno                                │
│                                                       │
│  Ubicación                                            │
│  Departamento ▼   Provincia ▼   Distrito ▼            │
│                                                       │
│  Imágenes (1 a 5)                                     │
│  ┌────┐ ┌────┐ ┌────┐ ┌────┐ ┌────┐                   │
│  │[+] │ │img │ │img │ │ +  │ │ +  │   ← ImageUploader │
│  └────┘ └────┘ └────┘ └────┘ └────┘                   │
│                                                       │
│  [ Cancelar ]                    [ Publicar objeto ]  │
└───────────────────────────────────────────────────────┘
```

---

## 8. Intercambios y Detalle de Intercambio

### 8.1 Lista de Intercambios

```
┌─────────────────────────────────────────────────────────┐
│  Mis intercambios     [Recibidos] [Enviados]            │
├─────────────────────────────────────────────────────────┤
│  ┌────────────────────────────────────────────────────┐ │
│  │ [img] Bicicleta ⇄ [img] Guitarra                  │  │
│  │ con Rosa Q. ★4.5     Estado: Pendiente   [Ver]    │  │
│  └────────────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────────────┐ │
│  │ [img] Libro ⇄ [img] Lámpara                       │ │
│  │ con Carlos M. ★4.0   Estado: Completado  [Ver]    │ │
│  └────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

### 8.2 Detalle de Intercambio (UC-021, UC-022)

```
┌──────────────────────────────────────────────────────────┐
│  Intercambio                          Estado: Aceptado   │ ← badge visible
├───────────────────────────┬──────────────────────────────┤
│  Tú ofreces:              │  Recibes:                    │
│  [img] Guitarra           │  [img] Bicicleta             │
│                           │  de Rosa Q. ★4.5             │
├───────────────────────────┴──────────────────────────────┤
│  Mensajes                                                │
│  Rosa: "Hola, coordinemos para mañana"                   │
│  Tú:   "Perfecto, a las 3pm"                             │
│  ( escribe un mensaje...                    ) [Enviar]   │
├──────────────────────────────────────────────────────────┤
│  [ Confirmar recepción ]        [ Cancelar intercambio ] │
│  (al completarse) → [ Calificar a Rosa ]                 │
└──────────────────────────────────────────────────────────┘
```

---

## 9. Perfil y Dashboard de Usuario

```
┌──────────────────────────────────────────────────────────┐
│  [avatar]  Rosa Quispe        ★ 4.5  ·  12 intercambios  │
│            Ayacucho            [ Editar perfil ]         │
├──────────────────────────────────────────────────────────┤
│  Resumen                                                 │
│  Objetos: 5   Activos: 2   Completados: 12               │
├──────────────────────────────────────────────────────────┤
│  Mis objetos publicados                                  │
│  ┌────────┐ ┌────────┐ ┌────────┐                        │
│  │[img]   │ │[img]   │ │[img]   │                        │
│  │Título  │ │Título  │ │Título  │                        │
│  └────────┘ └────────┘ └────────┘                        │
├──────────────────────────────────────────────────────────┤
│  Calificaciones recibidas                                │
│  ★★★★★ "Excelente intercambio" — Carlos M.             │
└──────────────────────────────────────────────────────────┘
```

---

## 10. Panel de Administración (UC-080, UC-081, UC-090)

```
┌──────────────┬───────────────────────────────────────────┐
│ SIDEBAR      │  Dashboard                                │
│              │  ┌──────────┐┌──────────┐┌──────────┐     │
│ ▸ Dashboard  │  │ Usuarios ││ Objetos  ││Intercamb.│     │
│ ▸ Usuarios   │  │  1,250   ││   870    ││   340    │     │
│ ▸ Objetos    │  └──────────┘└──────────┘└──────────┘     │
│ ▸ Reportes   │  ┌──────────┐┌──────────┐                 │
│ ▸ Auditoría  │  │Reportes  ││Activos30d│   ← KPIs        │
│              │  │    8     ││   410    │                 │
│              │  └──────────┘└──────────┘                 │
│              │                                           │
│              │  Reportes pendientes                      │
│              │  ┌──────────────────────────────────────┐ │
│              │  │ Objeto "X"  · Fraude   [Revisar]     │ │
│              │  │ Usuario "Y" · Spam     [Revisar]     │ │
│              │  └──────────────────────────────────────┘ │
└──────────────┴───────────────────────────────────────────┘
```

---

## 11. Aprobación

| Rol                        | Nombre            | Aprobación  | Fecha |
|----------------------------|-------------------|-------------|-------|
| Especialista UX/UI         | Equipo Enterprise | ☐ PENDIENTE | —     |
| Product Owner              | Equipo Enterprise | ☐ PENDIENTE | —     |
| Frontend Developer Senior  | Equipo Enterprise | ☐ PENDIENTE | —     |

---

> **GATE DE CALIDAD:**
> Estos wireframes son la referencia de disposición para implementar el frontend.
> El diseño visual final aplica los tokens de UI.md sobre esta estructura.

---

*Documento generado bajo la metodología SDD — Plataforma Inteligente de Intercambio de Objetos — Ayacucho, Perú.*
