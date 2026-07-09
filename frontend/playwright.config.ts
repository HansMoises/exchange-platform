import { defineConfig, devices } from '@playwright/test';

// Pruebas de Interfaz de Usuario (Testing.md SS3.2/SS4): navegador real
// contra el servidor de desarrollo de Vite.
export default defineConfig({
  testDir: './e2e',
  // El backend de desarrollo local (SQL Server + IIS Express) no tolera bien
  // multiples pruebas E2E golpeandolo en paralelo (registros/consultas geo
  // concurrentes causan timeouts intermitentes). Un solo worker es mas lento
  // pero determinista; en CI contra un backend dedicado esto podria revisarse.
  fullyParallel: false,
  workers: 1,
  reporter: 'html',
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'on-first-retry',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: true,
    timeout: 30_000,
  },
});
