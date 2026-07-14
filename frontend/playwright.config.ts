import { defineConfig, devices } from '@playwright/test';

// Pruebas End-To-End de Interfaz de Usuario (Testing.md SS3.2/SS4):
// navegador real contra el servidor de desarrollo de Vite, con el backend
// apuntando a la base de datos de pruebas aislada (PostgreSQL en Docker,
// puerto 5433). Ver docker-compose.test.yml y backend/start-e2e.ps1.
export default defineConfig({
  testDir: './e2e',

  // Un solo worker: las pruebas comparten la base de datos de pruebas y varias
  // registran usuarios y publican objetos. La ejecucion secuencial garantiza
  // determinismo a costa de tiempo (aprox. 1 minuto la suite completa).
  fullyParallel: false,
  workers: 1,

  // Evidencia para el informe: reporte HTML navegable + JSON para procesar.
  reporter: [
    ['list'],
    ['html', { open: 'never', outputFolder: 'playwright-report' }],
    ['json', { outputFile: 'test-results/resultados.json' }],
  ],

  use: {
    baseURL: 'http://localhost:5173',
    // Captura de evidencias en cada ejecucion (no solo en fallos), para
    // documentar los flujos verificados en la memoria de tesis.
    trace: 'on',
    screenshot: 'on',
    video: 'on',
  },

  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],

  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: true,
    timeout: 30_000,
  },
});