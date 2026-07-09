# UX.md
# Plataforma Inteligente de Intercambio de Objetos

> **Documento:** Diseño de Experiencia de Usuario (UX)
> **Fase SDLC:** 2 (Diseño) — documento complementario
> **Versión:** 1.0.0
> **Estado:** `PENDIENTE DE APROBACIÓN`
> **Fecha:** 2026-06-03
> **Autor:** Equipo Enterprise Senior (Especialista UX/UI)
> **Documentos padre:** Requisitos.md | CasosDeUso.md | HistoriasUsuario.md | Arquitectura.md
> **Convenciones:** Documentación en español. Diagramas en ASCII.

---

## Control de Versiones

| Versión | Fecha      | Autor                    | Cambios          |
|---------|------------|--------------------------|------------------|
| 1.0.0   | 2026-06-03 | Equipo Enterprise Senior | Versión inicial. |

---

## Tabla de Contenidos

1. Principios UX
2. Personas (Arquetipos de Usuario)
3. User Journeys (Recorridos Clave)
4. Arquitectura de Información
5. Flujos de Interacción Clave
6. Manejo de Estados (Carga, Vacío, Error)
7. Accesibilidad (WCAG)
8. Microcopy y Tono
9. Trazabilidad y Aprobación

---

## 1. Principios UX

Derivados de la sección UX del Prompt Master y de los RNF de usabilidad (RNF-040 a 042).

| Principio    | Aplicación                                                          |
|--------------|---------------------------------------------------------------------|
| Simplicidad  | Pocos pasos por tarea. Formularios cortos. Sin sobrecarga cognitiva.|
| Claridad     | Lenguaje directo. Acciones evidentes. Jerarquía visual clara.       |
| Rapidez      | Respuestas inmediatas (feedback), carga progresiva, búsqueda ágil.  |
| Confianza    | Identidad visible, reputación, estados claros de cada intercambio.  |
| Seguridad    | El usuario entiende qué comparte y con quién (no anónimo).          |
| Facilidad    | Mobile First. Navegación predecible. Recuperación de errores fácil. |

---

## 2. Personas (Arquetipos de Usuario)

### Persona 1 — El Ofertante

```
┌─────────────────────────────────────────────────────────┐
│  Rosa, 34 años — Ayacucho                               │
│  "Tengo cosas que ya no uso pero están en buen estado." │
├─────────────────────────────────────────────────────────┤
│  Objetivos:  Dar salida a objetos en desuso; conseguir  │
│              algo útil a cambio sin gastar.             │
│  Frustraciones: Vender por redes es inseguro y caótico. │
│  Necesita:   Publicar fácil, fotos claras, confianza en │
│              quién le escribe.                          │
│  Dispositivo: Mayormente móvil.                         │
└─────────────────────────────────────────────────────────┘
```

### Persona 2 — El Buscador

```
┌───────────────────────────────────────────────────────────┐
│  Carlos, 27 años — Ayacucho                               │
│  "Busco cosas específicas y cerca de mí."                 │
├───────────────────────────────────────────────────────────┤
│  Objetivos:  Encontrar objetos por categoría y cercanía;  │
│              intercambiar de forma segura.                │
│  Frustraciones: No saber con quién trata; ofertas lejanas.│
│  Necesita:   Buscar y filtrar rápido, ver reputación,     │
│              ubicación y estado del objeto.               │
│  Dispositivo: Móvil y escritorio.                         │
└───────────────────────────────────────────────────────────┘
```

### Persona 3 — El Moderador

```
┌──────────────────────────────────────────────────────────┐
│  Lucía, 40 años — Equipo de la plataforma                │
│  "Mantengo la comunidad sana y segura."                  │
├──────────────────────────────────────────────────────────┤
│  Objetivos:  Revisar reportes rápido; suspender abusos.  │
│  Necesita:   Cola de reportes priorizada, contexto claro,│
│              acciones de moderación a un clic.           │
│  Dispositivo: Escritorio.                                │
└──────────────────────────────────────────────────────────┘
```

### Persona 4 — El Administrador

```
┌──────────────────────────────────────────────────────────┐
│  Miguel, 45 años — Responsable de la plataforma          │
│  "Necesito ver el estado del sistema y gestionarlo."     │
├──────────────────────────────────────────────────────────┤
│  Objetivos:  Gestionar usuarios/categorías; ver métricas;│
│              garantizar trazabilidad.                    │
│  Necesita:   Dashboard claro, controles administrativos, │
│              auditoría consultable.                      │
│  Dispositivo: Escritorio.                                │
└──────────────────────────────────────────────────────────┘
```

---

## 3. User Journeys (Recorridos Clave)

### 3.1 Journey: De visitante a primera publicación (Rosa)

```
Descubre  →  Se registra  →  Confirma   →  Publica su  →  Recibe una
la web       (datos +        identidad     primer objeto   solicitud
             ubicación)      y ubicación   (fotos + cat.)   (notificación)

Emoción:  Curiosa → Algo de esfuerzo → Segura → Satisfecha → Ilusionada
Fricción: Formulario de registro: mantenerlo corto y claro.
Apoyo UX: Selectores geográficos en cascada; subida de fotos simple.
```

### 3.2 Journey: De búsqueda a intercambio completado (Carlos)

```
Busca   →  Filtra por  →  Ve detalle  →  Solicita    →  Acuerdan  →  Confirma  →  Califica
objeto     cercanía/cat.  + reputación   intercambio    (mensajes)   recepción    a Rosa
                          del ofertante  (ofrece algo)

Emoción:  Enfocado → Confiado → Decidido → Expectante → Coordinado → Tranquilo → Satisfecho
Fricción: Entender el estado del intercambio en cada paso.
Apoyo UX: Estados visibles (Pendiente/Aceptado/Completado); notificaciones claras.
```

---

## 4. Arquitectura de Información

Mapa de navegación principal (alineado con las páginas de Arquitectura.md).

```
                          ┌──────────────┐
                          │   Landing    │ (pública)
                          └──────┬───────┘
            ┌────────────────────┼────────────────────┐
            ▼                    ▼                     ▼
      ┌──────────┐        ┌─────────────┐       ┌──────────────┐
      │  Buscar  │        │  Login /    │       │ Detalle de   │
      │ objetos  │        │  Registro   │       │ objeto       │
      └──────────┘        └──────┬──────┘       └──────────────┘
                                 │ (autenticado)
        ┌────────────────────────┼────────────────────────┐
        ▼            ▼            ▼            ▼            ▼
   ┌─────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐
   │Dashboard│ │ Publicar │ │Intercam- │ │Favoritos │ │ Perfil   │
   │ usuario │ │ objeto   │ │ bios     │ │          │ │          │
   └─────────┘ └──────────┘ └────┬─────┘ └──────────┘ └──────────┘
                                 ▼
                          ┌──────────────┐
                          │ Mensajes /   │
                          │ Notificac.   │
                          └──────────────┘

   Zona ADMIN (rol Moderador/Administrador):
   ┌──────────┬──────────┬──────────┬──────────┬──────────┐
   │Dashboard │ Usuarios │ Objetos  │ Reportes │Auditoría │
   │  admin   │          │          │          │          │
   └──────────┴──────────┴──────────┴──────────┴──────────┘
```

---

## 5. Flujos de Interacción Clave

### 5.1 Publicar un objeto (UC-010)

```
1. Usuario pulsa "Publicar".
2. Completa título, descripción, categoría, condición, ubicación.
3. Sube 1-5 imágenes (arrastrar o seleccionar). Vista previa inmediata.
4. Revisa y confirma.
5. Feedback: "Objeto publicado". Redirige al detalle del objeto.

Reglas UX: validación en vivo; botón de envío deshabilitado hasta cumplir mínimos;
mensajes de error junto al campo, no genéricos.
```

### 5.2 Solicitar intercambio (UC-020)

```
1. En el detalle de un objeto ajeno, pulsa "Proponer intercambio".
2. Selecciona uno de SUS objetos disponibles para ofrecer.
3. Escribe un mensaje opcional.
4. Envía. Feedback: "Solicitud enviada". El ofertante recibe notificación.

Reglas UX: si no tiene objetos disponibles, se le invita a publicar primero;
no se puede solicitar el propio objeto (la acción no aparece).
```

### 5.3 Cerrar intercambio (UC-022)

```
1. Tras coordinar (mensajes), cada parte pulsa "Confirmar recepción".
2. Cuando ambos confirman → estado "Completado".
3. Se invita a calificar a la contraparte (1-5 + comentario).
4. Feedback: reputación actualizada.

Reglas UX: el estado del intercambio es siempre visible; calificar es opcional
pero se incentiva con un recordatorio no intrusivo.
```

---

## 6. Manejo de Estados (Carga, Vacío, Error)

Cada vista contempla sus estados, no solo el "ideal con datos".

| Estado      | Tratamiento UX                                                                     |
|-------------|------------------------------------------------------------------------------------|
| Cargando    | Skeleton/placeholder; nunca pantalla en blanco. Feedback en <300ms.                |
| Vacío       | Mensaje amable + acción sugerida. Ej: "Aún no tienes objetos. Publica el primero." |
| Error       | Mensaje claro y no técnico + opción de reintentar.                                 |
| Sin permiso | Explicar por qué y ofrecer login si aplica.                                        |
| Éxito       | Confirmación breve (toast) y siguiente paso claro.                                 |

---

## 7. Accesibilidad (WCAG)

Cumplimiento orientado a WCAG 2.1 nivel AA (RNF-041).
 
| Criterio                | Aplicación                                                    |
|-------------------------|---------------------------------------------------------------|
| Contraste               | Mínimo 4.5:1 en texto normal; 3:1 en texto grande.            |
| Navegación por teclado  | Todos los controles accesibles con Tab; foco visible.         |
| Lectores de pantalla    | Etiquetas ARIA, textos alternativos en imágenes.              |
| Formularios             | Labels asociados, mensajes de error descriptivos y vinculados.|
| Tamaño táctil           | Áreas de toque ≥ 44x44 px (Mobile First).                     |
| Independencia del color | El estado nunca se comunica solo por color (texto + icono).   |
| Texto escalable         | La interfaz soporta zoom hasta 200% sin pérdida de función.   |

---

## 8. Microcopy y Tono

| Contexto            | Tono / ejemplo                                                     |
|---------------------|--------------------------------------------------------------------|
| General             | Cercano, claro, en español peruano neutro. Sin tecnicismos.        |
| Confirmaciones      | "Objeto publicado." / "Solicitud enviada."                         |
| Errores             | "No pudimos guardar tu objeto. Revisa los campos marcados."        |
| Vacíos              | "Todavía no hay nada por aquí. ¡Empieza publicando un objeto!"     |
| Acciones críticas   | Confirmación explícita: "¿Seguro que deseas eliminar este objeto?" |
| Seguridad/Confianza | Mostrar reputación, ubicación y nombre real (no anónimo).          |

---

## 9. Trazabilidad y Aprobación

### 9.1 Trazabilidad

| Elemento UX               | Responde a              |
|---------------------------|-------------------------|
| Principios de simplicidad | RNF-042                 |
| Responsive Mobile First   | RNF-040                 |
| Accesibilidad WCAG        | RNF-041                 |
| Journeys de intercambio   | UC-010, UC-020, UC-022  |
| Estados y feedback        | HU-020, HU-040, HU-042  |
| Confianza / no anónimo    | RN-021, RN-003          |

### 9.2 Aprobación

| Rol                  | Nombre            | Aprobación  | Fecha |
|----------------------|-------------------|-------------|-------|
| Especialista UX/UI   | Equipo Enterprise | ☐ PENDIENTE | —     |
| Product Owner        | Equipo Enterprise | ☐ PENDIENTE | —     |
| Frontend Senior      | Equipo Enterprise | ☐ PENDIENTE | —     |

---

> **GATE DE CALIDAD:**
> Este documento define el comportamiento y los flujos. El diseño visual (colores,
> tipografía, componentes) se especifica en `UI.md`, coherente con esta experiencia.

---

*Documento generado bajo la metodología SDD — Plataforma Inteligente de Intercambio de Objetos — Ayacucho, Perú.*
