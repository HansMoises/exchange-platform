# UI.md
# Plataforma Inteligente de Intercambio de Objetos

> **Documento:** Sistema de Diseño Visual (UI)
> **Fase SDLC:** 2 (Diseño) — documento complementario
> **Versión:** 1.1.0
> **Estado:** `PENDIENTE DE APROBACIÓN`
> **Fecha:** 2026-07-07
> **Autor:** Equipo Enterprise Senior (Especialista UX/UI)
> **Documentos padre:** UX.md | Arquitectura.md | Requisitos.md
> **Convenciones:** Documentación en español. Diagramas en ASCII. Stack visual: TailwindCSS.

---

## Control de Versiones

| Versión | Fecha      | Autor                    | Cambios          |
|---------|------------|--------------------------|------------------|
| 1.0.0   | 2026-06-03 | Equipo Enterprise Senior | Versión inicial. |
| 1.1.0   | 2026-07-07 | Equipo Enterprise Senior | Evolución premium de la paleta e identidad verde/tierra andina (misma esencia, sin reemplazo): hover de primario más profundo, texto secundario más oscuro para cumplir AA en texto normal, radios ampliados (8/14/20px), sombras en capas (`card`/`soft`/`elevated`/`glow`), tipografía Inter cargada realmente por primera vez + Plus Jakarta Sans como fuente de display para títulos/hero, animaciones ligeras (`fade-in`/`slide-up`/`scale-in`, 150–300ms), badges en forma de píldora, microinteracciones (hover elevado, zoom de imagen, resplandor de marca en botones). Se documenta también la Landing page (antes placeholder), construida con datos reales (`/stats/public`, `/geo/categorias`, `/objects`), sin testimonios ficticios. |

---

## Tabla de Contenidos

1. Concepto e Inspiración
2. Paleta de Colores
3. Tipografía
4. Jerarquía de Texto
5. Design Tokens
6. Sistema de Espaciado
7. Catálogo de Componentes
8. Iconografía
9. Responsividad (Breakpoints)
10. Verificación de Contraste (WCAG)
11. Trazabilidad y Aprobación

---

## 1. Concepto e Inspiración

La identidad visual se inspira en la **naturaleza andina de Ayacucho**: los verdes de sus campos y cerros, los tonos tierra de su geografía y artesanía. Comunica los valores del proyecto: **comunidad, confianza, sostenibilidad, economía circular e intercambio responsable**.

| Concepto             | Expresión visual                                       |
|----------------------|--------------------------------------------------------|
| Comunidad            | Tonos cálidos y cercanos; fotos de objetos reales.     |
| Confianza            | Limpieza, orden, jerarquía clara; verde como base.     |
| Sostenibilidad       | Paleta natural (verdes, tierra); sin estridencias.     |
| Economía circular    | Iconografía de reutilización e intercambio.            |
| Intercambio responsable | Identidad visible, estados claros, diseño honesto.  |

Estilo general: **moderno, profesional, accesible, Mobile First**.

---

## 2. Paleta de Colores

### 2.1 Colores Primarios

| Token            | HEX       | Uso                                            |
|------------------|-----------|------------------------------------------------|
| Verde Bosque     | #2E5D34 | Color primario. Botones principales, marca.    |
| Verde Bosque 700 | #1F3F24 | Hover de primario, énfasis (v1.1: más profundo para dar sensacion premium). |
| Verde Bosque 100 | #DCE8DD | Fondos suaves, badges, estados informativos.   |
| Verde Bosque 50  | #EAF2EA | (v1.1) Tintes muy sutiles, fondos de iconos circulares. |

### 2.2 Colores Secundarios

| Token            | HEX       | Uso                                            |
|------------------|-----------|------------------------------------------------|
| Verde Oliva      | #6B7F3A | Color secundario. Acentos, enlaces, etiquetas. |
| Verde Oliva 600  | #556630 | Hover de secundario.                           |

### 2.3 Colores Complementarios (Tierra Andina)

| Token            | HEX       | Uso                                            |
|------------------|-----------|------------------------------------------------|
| Marrón Suave     | #A6764F | Detalles cálidos, iconos, ilustraciones.       |
| Tierra Andina    | #C8A27C | Fondos cálidos, secciones destacadas.          |

### 2.4 Neutros

| Token        | HEX       | Uso                                  |
|--------------|-----------|--------------------------------------|
| Blanco       | #FFFFFF | Fondo principal, tarjetas.           |
| Gris Claro   | #F6F5F1 | Fondo de secciones, separadores (v1.1: leve ajuste). |
| Beige        | #EDE7DD | Fondo cálido alternativo.            |
| Gris Medio   | #66665F | Texto secundario (v1.1: oscurecido de #8A8A82 para cumplir WCAG AA tambien en texto normal). |
| Gris Oscuro  | #33332F | Texto principal (v1.1: leve ajuste). |

### 2.5 Colores de Estado (Feedback)

| Token        | HEX       | Uso                                    |
|--------------|-----------|----------------------------------------|
| Éxito        | #2E7D4F | Confirmaciones, intercambio completado.|
| Advertencia  | #B8860B | Avisos, estados pendientes.            |
| Error        | #B23B3B | Errores, acciones destructivas.        |
| Información  | #2C6E91 | Mensajes informativos.                 |

---

## 3. Tipografía

| Rol            | Fuente                  | Justificación                              |
|----------------|-------------------------|--------------------------------------------|
| Principal      | Inter / system-ui       | Moderna, legible, excelente en pantalla.   |
| Alternativa    | Segoe UI, Roboto, sans  | Fallback del sistema.                      |

Pesos: Regular (400), Medium (500), SemiBold (600), Bold (700). Tipografía legible y accesible, sin serifas decorativas en cuerpo de texto.

---

## 4. Jerarquía de Texto

| Estilo   | Tamaño (px / rem) | Peso     | Uso                                |
|----------|-------------------|----------|------------------------------------|
| Hero     | 48 / 3.0          | ExtraBold| (v1.1) Titular de Landing/hero, fuente Plus Jakarta Sans. |
| H1       | 32 / 2.0          | Bold     | Título de página.                  |
| H2       | 26 / 1.625        | SemiBold | Secciones principales.             |
| H3       | 21 / 1.3125       | SemiBold | Subsecciones, títulos de tarjeta.  |
| Body     | 16 / 1.0          | Regular  | Texto general.                     |
| Body S   | 14 / 0.875        | Regular  | Texto secundario, descripciones.   |
| Caption  | 12 / 0.75         | Regular  | Notas, metadatos, timestamps.      |
| Label    | 14 / 0.875        | Medium   | Etiquetas de formulario, badges.   |

Interlineado base: 1.5 para cuerpo de texto (legibilidad y accesibilidad).

> **(v1.1)** Se agrega **Plus Jakarta Sans** como fuente de *display* para H1/H2/Hero (títulos grandes), manteniendo Inter para cuerpo de texto y UI, tal como recomienda esta misma sección. Ambas se cargan realmente vía Google Fonts (antes Inter nunca se cargaba y el sitio usaba la fuente del sistema como fallback silencioso).

---

## 5. Design Tokens

Variables para TailwindCSS / CSS, fuente única de verdad del estilo.

```css
:root {
  /* Primarios */
  --color-primary        : #2E5D34;
  --color-primary-hover  : #1F3F24;
  --color-primary-soft   : #DCE8DD;

  /* Secundarios */
  --color-secondary      : #6B7F3A;
  --color-secondary-hover: #556630;

  /* Tierra andina */
  --color-earth          : #A6764F;
  --color-earth-soft     : #C8A27C;

  /* Neutros */
  --color-bg             : #FFFFFF;
  --color-bg-alt         : #F6F5F1;
  --color-bg-warm        : #EDE7DD;
  --color-text           : #33332F;
  --color-text-secondary : #66665F;

  /* Estados */
  --color-success        : #2E7D4F;
  --color-warning        : #B8860B;
  --color-error          : #B23B3B;
  --color-info           : #2C6E91;

  /* Radios (v1.1: ampliados para una sensacion mas premium) */
  --radius-sm            : 8px;
  --radius-md            : 14px;
  --radius-lg            : 20px;
  --radius-xl            : 28px;

  /* Sombras (v1.1: en capas en vez de un solo blur) */
  --shadow-card          : 0 1px 2px rgba(51,51,47,0.04), 0 2px 8px rgba(51,51,47,0.06);
  --shadow-soft          : 0 1px 2px rgba(51,51,47,0.05);
  --shadow-elevated      : 0 16px 32px -12px rgba(51,51,47,0.18), 0 4px 10px -4px rgba(51,51,47,0.08);
  --shadow-glow          : 0 10px 28px -8px rgba(46,93,52,0.45);

  /* Tipografía */
  --font-base            : 'Inter', system-ui, sans-serif;
  --font-display         : 'Plus Jakarta Sans', 'Inter', system-ui, sans-serif;
}
```

---

## 6. Sistema de Espaciado

Escala basada en múltiplos de 4px (consistencia y ritmo visual).

| Token | Valor | Uso                          |
|-------|-------|------------------------------|
| xs    | 4px   | Separaciones mínimas.        |
| sm    | 8px   | Padding interno pequeño.     |
| md    | 16px  | Espaciado estándar.          |
| lg    | 24px  | Separación entre bloques.    |
| xl    | 32px  | Márgenes de sección.         |
| 2xl   | 48px  | Separaciones grandes.        |

---

## 7. Catálogo de Componentes

Componentes reutilizables (coinciden con la lista de `Arquitectura.md`).

### 7.1 Botones

| Variante   | Apariencia                          | Uso                          |
|------------|-------------------------------------|------------------------------|
| Primario   | Fondo verde bosque, texto blanco.   | Acción principal.            |
| Secundario | Borde verde, texto verde.           | Acción secundaria.           |
| Texto      | Sin fondo, texto verde oliva.       | Acciones terciarias.         |
| Peligro    | Fondo/borde rojo.                   | Eliminar, acciones destructivas.|

Estados de cada botón: normal, hover, activo, foco (anillo visible), deshabilitado.

```
┌──────────────────────┐   ┌──────────────────────┐
│   Publicar objeto    │   │   Cancelar           │
│   (primario verde)   │   │   (secundario borde) │
└──────────────────────┘   └──────────────────────┘
```

### 7.2 Tarjeta de Objeto (ObjectCard)

```
┌─────────────────────────────┐
│  ┌───────────────────────┐  │
│  │      [Imagen 16:9]    │  │  ← imagen portada
│  └───────────────────────┘  │
│  Bicicleta rodado 26        │  ← H3 título
│  Deportes · Ayacucho        │  ← caption categoría/ubicación
│  4.5  ·  Disponible         │  ← reputación + badge estado
│  ┌─────────────────────┐    │
│  │  Ver detalle        │    │  ← botón secundario
│  └─────────────────────┘    │
└─────────────────────────────┘
```

### 7.3 Badges de Estado

| Estado          | Color de fondo      | Texto    |
|-----------------|---------------------|----------|
| Disponible      | Verde Bosque 100    | Verde    |
| Reservado       | Beige / Advertencia | Marrón   |
| Intercambiado   | Gris claro          | Gris     |
| Pendiente       | Info suave          | Azul     |
| Completado      | Éxito suave         | Verde    |
| Rechazado       | Error suave         | Rojo     |

### 7.4 Otros Componentes

Inputs, Selects (con cascada geográfica), Modales, Tablas (admin), Paginación, SearchBox, Loading (skeleton), EmptyState, Toast, Avatar, Gallery (imágenes de objeto), Navbar, Sidebar (admin), Footer, Map (V2). Todos con sus estados (normal/foco/error/deshabilitado) y accesibles por teclado.

---

## 8. Iconografía

| Aspecto       | Decisión                                                  |
|---------------|-----------------------------------------------------------|
| Librería      | lucide-react (consistente con el stack frontend).         |
| Estilo        | Lineal, trazo medio, esquinas suaves.                     |
| Tamaños       | 16px (inline), 20px (botones), 24px (navegación).         |
| Color         | Hereda del texto o usa tokens de estado.                  |
| Accesibilidad | Iconos informativos con aria-label; decorativos ocultos.  |

Iconos clave: intercambio (flechas circulares), favorito (corazón), ubicación (pin), notificación (campana), reputación (estrella), reportar (bandera).

---

## 9. Responsividad (Breakpoints)

Mobile First (RNF-040). Estilos base para móvil, ampliados hacia arriba.

| Breakpoint | Ancho     | Dispositivo        | Layout                            |
|------------|-----------|--------------------|-----------------------------------|
| base       | < 640px   | Móvil              | 1 columna; navegación inferior.   |
| sm         | ≥ 640px   | Móvil grande       | 1-2 columnas.                     |
| md         | ≥ 768px   | Tablet             | 2-3 columnas; sidebar colapsable. |
| lg         | ≥ 1024px  | Escritorio         | 3-4 columnas; sidebar fija.       |
| xl         | ≥ 1280px  | Escritorio grande  | Contenedor centrado, máx. ancho.  |

---

## 10. Verificación de Contraste (WCAG)

Combinaciones principales verificadas para cumplir WCAG AA (≥ 4.5:1).

| Combinación                          | Ratio aprox. | Cumple AA         |
|--------------------------------------|--------------|-------------------|
| Texto Gris Oscuro (#33332F) / Blanco | ~11.5:1      | Sí                |
| Blanco / Verde Bosque (#2E5D34)      | ~7.0:1       | Sí                |
| Texto Gris Medio (#66665F) / Blanco  | ~4.8:1       | Sí (v1.1: ya cumple tambien en texto normal) |
| Verde Oliva (#6B7F3A) / Blanco       | ~4.0:1       | Solo texto grande |
| Blanco / Error (#B23B3B)             | ~5.2:1       | Sí                |

> Nota: el Verde Oliva sobre blanco se reserva para texto grande o elementos no esenciales. Para texto normal se usa Gris Oscuro, Gris Medio o Verde Bosque. El estado nunca se comunica solo por color (texto + icono).

---

## 11. Trazabilidad y Aprobación

### 11.1 Trazabilidad

| Elemento UI                     | Responde a                 |
|---------------------------------|----------------------------|
| Paleta verde/tierra andina      | UI (Prompt Master)         |
| Mobile First / breakpoints      | RNF-040                    |
| Contraste WCAG AA               | RNF-041, UX.md §7          |
| Componentes reutilizables       | Arquitectura.md (Frontend) |
| Badges de estado de intercambio | UX.md §4 (confianza)       |
| Tokens de diseño                | Mantenibilidad (RNF-052)   |

### 11.2 Aprobación

| Rol                  | Nombre            | Aprobación  | Fecha |
|----------------------|-------------------|-------------|-------|
| Especialista UX/UI   | Equipo Enterprise | ☐ PENDIENTE | —     |
| Product Owner        | Equipo Enterprise | ☐ PENDIENTE | —     |
| Frontend Senior      | Equipo Enterprise | ☐ PENDIENTE | —     |

---

> **GATE DE CALIDAD:**
> Este sistema de diseño es la fuente visual única. Los componentes de `Frontend.md`
> deben implementarse con estos tokens. El prototipo navegable se detalla en `Prototipo.md`.

---

*Documento generado bajo la metodología SDD — Plataforma Inteligente de Intercambio de Objetos — Ayacucho, Perú.*
