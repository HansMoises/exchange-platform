import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import axios from 'axios';
import type { LoginResponseDto, RefreshResponseDto, UsuarioDto } from '../types/usuario.types';
import type { ApiResponse } from '../types/api.types';

interface AuthState {
  usuario: UsuarioDto | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  login: (data: LoginResponseDto) => void;
  logout: () => void;
  intentarRefresh: () => Promise<boolean>;
  actualizarUsuario: (cambios: Partial<UsuarioDto>) => void;
}

// Instancia sin interceptores: evita el bucle infinito de refresh-en-401
// que se produciria si esta llamada pasara por apiClient.
const refreshClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  headers: { 'Content-Type': 'application/json' },
});

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      usuario: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,

      login: (data) =>
        set({
          usuario: data.usuario,
          accessToken: data.accessToken,
          refreshToken: data.refreshToken,
          isAuthenticated: true,
        }),

      logout: () =>
        set({
          usuario: null,
          accessToken: null,
          refreshToken: null,
          isAuthenticated: false,
        }),

      // Refleja en el usuario de la sesion cambios hechos en Perfil (nombre,
      // foto...) sin esperar a un nuevo login/refresh.
      actualizarUsuario: (cambios) => {
        const usuarioActual = get().usuario;
        if (!usuarioActual) return;
        set({ usuario: { ...usuarioActual, ...cambios } });
      },

      intentarRefresh: async () => {
        const refreshToken = get().refreshToken;
        if (!refreshToken) return false;

        try {
          const { data } = await refreshClient.post<ApiResponse<RefreshResponseDto>>('/auth/refresh', {
            refreshToken,
          });
          if (!data.data) return false;

          set({
            accessToken: data.data.accessToken,
            refreshToken: data.data.refreshToken,
          });
          return true;
        } catch {
          return false;
        }
      },
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({
        usuario: state.usuario,
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        isAuthenticated: state.isAuthenticated,
      }),
    },
  ),
);
