import axios, { type InternalAxiosRequestConfig } from 'axios';
import { useAuthStore } from '../stores/authStore';

interface RetryableConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  headers: { 'Content-Type': 'application/json' },
});

// Request: adjunta el Access Token.
apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response: maneja 401 con refresh automatico (una sola vez por request).
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const config = error.config as RetryableConfig | undefined;
    const esRutaAuth = config?.url?.includes('/auth/');

    if (error.response?.status === 401 && config && !config._retry && !esRutaAuth) {
      config._retry = true;
      const ok = await useAuthStore.getState().intentarRefresh();
      if (ok) {
        return apiClient(config);
      }
      useAuthStore.getState().logout();
    }

    return Promise.reject(error);
  },
);
