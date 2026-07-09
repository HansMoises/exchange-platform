# Frontend.md
# Plataforma Inteligente de Intercambio de Objetos

> **Documento:** Guía de Implementación del Frontend
> **Fase SDLC:** 3 (Desarrollo / Implementación)
> **Versión:** 1.0.0
> **Estado:** `PENDIENTE DE APROBACIÓN`
> **Fecha:** 2026-06-03
> **Autor:** Equipo Enterprise Senior (Frontend Developer Senior / Arquitecto / UX-UI)
> **Documentos padre:** Arquitectura.md | UX.md | UI.md | API.md | Seguridad.md | Convenciones.md
> **Convenciones:** Documentación en español; código en español (con APIs de librerías en su idioma). Diagramas en ASCII. Stack: React + TypeScript.

---

## Control de Versiones

| Versión | Fecha      | Autor                    | Cambios          |
|---------|------------|--------------------------|------------------|
| 1.0.0   | 2026-06-03 | Equipo Enterprise Senior | Versión inicial. |

---

## Tabla de Contenidos

1. Alcance y Stack
2. Estructura del Proyecto
3. Anatomía de una Feature
4. Capa de Servicios (Axios)
5. Gestión de Estado (Zustand)
6. Formularios (React Hook Form + Zod)
7. Tipos y Contrato de API
8. Autenticación y Refresh Token
9. Rutas y Protección por Rol
10. Manejo de Estados de UI
11. Aplicación del Sistema Visual (UI.md)
12. Orden de Implementación
13. Aprobación

---

## 1. Alcance y Stack

Guía de implementación del SPA según el diseño aprobado. No redefine UX/UI; explica **cómo** construir en React.

| Componente  | Tecnología                     |
|-------------|--------------------------------|
| Librería UI | React                          |
| Lenguaje    | TypeScript (modo estricto)     |
| Build / Dev | Vite                           |
| Estilos     | TailwindCSS (tokens de UI.md)  |
| HTTP        | Axios (con interceptores)      |
| Ruteo       | React Router DOM               |
| Formularios | React Hook Form + Zod          |
| Estado      | Zustand (stores por módulo)    |
| Pruebas     | Vitest + React Testing Library |

---

## 2. Estructura del Proyecto

Feature Based (detalle en Arquitectura.md §4.1).

```
src/
├── assets/
├── components/ui/        ← componentes reutilizables (Button, Input, Card...)
├── components/layout/    ← Navbar, Sidebar, Footer
├── features/             ← módulos: auth, users, objects, exchanges, ...
├── layouts/              ← Public, Auth, Dashboard, Admin
├── pages/                ← páginas que componen features + layout
├── routes/               ← AppRouter, Public/Protected/RoleBased Route
├── services/             ← apiClient (Axios)
├── stores/               ← Zustand stores
├── hooks/                ← hooks globales
├── types/                ← tipos globales
├── utils/                ← formatters, validators, constants
└── styles/               ← globals.css (tokens)
```

---

## 3. Anatomía de una Feature

Cada feature es autocontenida. Ejemplo `features/objects/`:

```
features/objects/
├── components/
│   ├── ObjectCard.tsx        ← tarjeta de objeto (UI.md §7.2)
│   ├── ObjectForm.tsx        ← formulario publicar/editar
│   ├── ObjectList.tsx        ← lista con paginación
│   └── ImageUploader.tsx     ← subida 1-5 imágenes
├── hooks/
│   └── useObjects.ts         ← lógica de carga/paginación/filtros
├── services/
│   └── objetoService.ts      ← llamadas a /api/v1/objects
├── types/
│   └── objeto.types.ts       ← ObjetoDto, CrearObjetoDto, etc.
└── schemas/
    └── objeto.schemas.ts     ← validación Zod
```

Regla: lo común a varias features va en `components/ui` o `hooks`; lo específico se queda en la feature.

---

## 4. Capa de Servicios (Axios)

### 4.1 Cliente base con interceptores

```typescript
// services/apiClient.ts
import axios from 'axios';
import { useAuthStore } from '../stores/authStore';

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL, // http://localhost:5000/api/v1
  headers: { 'Content-Type': 'application/json' },
});

// Request: adjunta el Access Token
apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// Response: maneja 401 con refresh automático
apiClient.interceptors.response.use(
  (res) => res,
  async (error) => {
    if (error.response?.status === 401) {
      const ok = await useAuthStore.getState().intentarRefresh();
      if (ok) return apiClient(error.config);   // reintenta
      useAuthStore.getState().logout();          // refresh falló
    }
    return Promise.reject(error);
  }
);
```

### 4.2 Servicio de feature

```typescript
// features/objects/services/objetoService.ts
import { apiClient } from '../../../services/apiClient';
import type { ObjetoDto, CrearObjetoDto, PagedResult } from '../types/objeto.types';

export const objetoService = {
  listar: (params: Record<string, unknown>) =>
    apiClient.get<ApiResponse<PagedResult<ObjetoDto>>>('/objects', { params }),

  obtener: (id: string) =>
    apiClient.get<ApiResponse<ObjetoDto>>(`/objects/${id}`),

  crear: (dto: CrearObjetoDto) =>
    apiClient.post<ApiResponse<{ id: string }>>('/objects', dto),
};
```

---

## 5. Gestión de Estado (Zustand)

Un store por módulo. Ejemplo `authStore`:

```typescript
// stores/authStore.ts
import { create } from 'zustand';

interface AuthState {
  usuario: UsuarioDto | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  login: (data: LoginResponseDto) => void;
  logout: () => void;
  intentarRefresh: () => Promise<boolean>;
}

export const useAuthStore = create<AuthState>((set, get) => ({
  usuario: null,
  accessToken: null,
  refreshToken: null,
  isAuthenticated: false,

  login: (data) => set({
    usuario: data.user,
    accessToken: data.accessToken,
    refreshToken: data.refreshToken,
    isAuthenticated: true,
  }),

  logout: () => set({
    usuario: null, accessToken: null, refreshToken: null, isAuthenticated: false,
  }),

  intentarRefresh: async () => {
    // llama POST /auth/refresh con el refreshToken; actualiza tokens; retorna éxito
    return false; // implementación real en Fase 3
  },
}));
```

> Nota: en artefactos de navegador no se usa localStorage; en la app real, la persistencia de sesión se maneja según la política de seguridad (Seguridad.md).

---

## 6. Formularios (React Hook Form + Zod)

Validación en frontend que **refleja** las reglas del backend (no las reemplaza — RN, Seguridad.md).

```typescript
// features/auth/schemas/auth.schemas.ts
import { z } from 'zod';

export const registroSchema = z.object({
  nombres: z.string().min(2).max(100),
  apellidos: z.string().min(2).max(100),
  email: z.string().email().max(256),
  password: z.string()
    .min(8)
    .regex(/[A-Z]/, 'Debe tener una mayúscula')
    .regex(/[a-z]/, 'Debe tener una minúscula')
    .regex(/[0-9]/, 'Debe tener un número')
    .regex(/[^A-Za-z0-9]/, 'Debe tener un carácter especial'),
  confirmPassword: z.string(),
  telefono: z.string().regex(/^[0-9]{9}$/, '9 dígitos'),
  departamentoId: z.number(),
  provinciaId: z.number(),
  distritoId: z.number(),
}).refine((d) => d.password === d.confirmPassword, {
  message: 'Las contraseñas no coinciden',
  path: ['confirmPassword'],
});

export type RegistroForm = z.infer<typeof registroSchema>;
```

```tsx
// uso en componente
const { register, handleSubmit, formState: { errors } } =
  useForm<RegistroForm>({ resolver: zodResolver(registroSchema) });
```

---

## 7. Tipos y Contrato de API

El frontend tipa el contrato estándar de respuesta (API.md §2).

```typescript
// types/api.types.ts
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: string[] | null;
  timestamp: string;
}

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalRecords: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}
```

---

## 8. Autenticación y Refresh Token

```
1. Login → guarda accessToken + refreshToken en authStore.
2. Cada request adjunta Bearer accessToken (interceptor).
3. Si la API responde 401:
     a. El interceptor llama intentarRefresh() (POST /auth/refresh).
     b. Si OK → actualiza tokens y reintenta la request original.
     c. Si falla → logout() y redirige a /login.
4. Logout → POST /auth/logout (revoca refresh) + limpia store.
```

Coherente con la rotación de refresh tokens de `Seguridad.md`.

---

## 9. Rutas y Protección por Rol

```tsx
// routes/AppRouter.tsx (esquema)
<Routes>
  {/* Públicas */}
  <Route element={<PublicLayout />}>
    <Route path="/" element={<Landing />} />
    <Route path="/search" element={<Search />} />
    <Route path="/objects/:id" element={<ObjectDetail />} />
  </Route>

  {/* Auth */}
  <Route element={<AuthLayout />}>
    <Route path="/login" element={<PublicRoute><Login /></PublicRoute>} />
    <Route path="/register" element={<PublicRoute><Register /></PublicRoute>} />
  </Route>

  {/* Protegidas (usuario autenticado) */}
  <Route element={<ProtectedRoute><DashboardLayout /></ProtectedRoute>}>
    <Route path="/dashboard" element={<Dashboard />} />
    <Route path="/publish" element={<PublishObject />} />
    <Route path="/exchanges" element={<Exchanges />} />
  </Route>

  {/* Admin (rol Moderador/Administrador) */}
  <Route element={<RoleBasedRoute roles={['Administrador','Moderador']}><AdminLayout /></RoleBasedRoute>}>
    <Route path="/admin" element={<AdminDashboard />} />
    <Route path="/admin/users" element={<RoleBasedRoute roles={['Administrador']}><AdminUsers /></RoleBasedRoute>} />
  </Route>
</Routes>
```

Las rutas reflejan la matriz de permisos de `Seguridad.md`.

---

## 10. Manejo de Estados de UI

Toda vista de datos contempla los estados (UX.md §6):

```tsx
function ObjectList() {
  const { objetos, isLoading, error } = useObjects();

  if (isLoading) return <Loading />;            // skeleton
  if (error)     return <ErrorState onRetry={...} />;
  if (objetos.length === 0) return <EmptyState mensaje="Aún no hay objetos." accion="Publicar" />;

  return <>{objetos.map(o => <ObjectCard key={o.id} objeto={o} />)}</>;
}
```

---

## 11. Aplicación del Sistema Visual (UI.md)

Los tokens de `UI.md` se configuran en Tailwind y se usan vía clases.

```javascript
// tailwind.config.js (extracto)
export default {
  theme: {
    extend: {
      colors: {
        primary: '#2E5D34',
        'primary-hover': '#24492A',
        secondary: '#6B7F3A',
        earth: '#A6764F',
        success: '#2E7D4F',
        error: '#B23B3B',
      },
      borderRadius: { md: '10px', lg: '16px' },
    },
  },
};
```

```tsx
// uso
<button className="bg-primary hover:bg-primary-hover text-white rounded-md px-4 py-2">
  Publicar objeto
</button>
```

Reglas: responsive Mobile First; accesibilidad WCAG (foco visible, aria-labels, contraste); estado nunca solo por color.

---

## 12. Orden de Implementación

```
1. Setup: Vite + React + TS + Tailwind (tokens UI.md) + ESLint/Prettier.
2. Base: apiClient (Axios + interceptores), tipos ApiResponse/PagedResult, router.
3. Componentes UI base (Button, Input, Card, Loading, EmptyState, Toast...).
4. Feature AUTH: login, registro, recuperación + authStore + rutas públicas.
5. Layouts y rutas protegidas/rol.
6. Feature OBJECTS: lista, detalle, publicar, editar (+ búsqueda y filtros).
7. Feature EXCHANGES: solicitar, gestionar, confirmar, calificar.
8. Features NOTIFICATIONS, FAVORITES, MESSAGES, REPORTS.
9. Feature ADMIN: dashboard, gestión, auditoría.
10. Accesibilidad, responsive, pruebas de componentes, optimización (lazy/code splitting).
```

> Cada feature: rama feature/*, pruebas (Vitest/RTL), PR y DoD (GitFlow.md, ChecklistCalidad.md).

---

## 13. Aprobación

| Rol                        | Nombre            | Aprobación  | Fecha |
|----------------------------|-------------------|-------------|-------|
| Frontend Developer Senior  | Equipo Enterprise | ☐ PENDIENTE | —     |
| Arquitecto de Software     | Equipo Enterprise | ☐ PENDIENTE | —     |
| Especialista UX/UI         | Equipo Enterprise | ☐ PENDIENTE | —     |

---

> **GATE DE CALIDAD — FASE 3 (Frontend):**
> El SPA debe seguir esta guía, la experiencia de UX.md y el sistema visual de UI.md.
> Cada feature pasa por pruebas de componentes, accesibilidad y review antes de integrarse.

---

*Documento generado bajo la metodología SDD — Plataforma Inteligente de Intercambio de Objetos — Ayacucho, Perú.*
