import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { expect, type Page } from '@playwright/test';

const __dirname = path.dirname(fileURLToPath(import.meta.url));

export async function registrarUsuario(page: Page, correo: string) {
  await page.goto('/register');
  await page.getByLabel('Nombres').fill('Test');
  await page.getByLabel('Apellidos').fill('E2E');
  await page.getByLabel('Correo electronico').fill(correo);
  await page.getByLabel('Contrasena', { exact: true }).fill('ClaveSegura@123');
  await page.getByLabel('Confirmar contrasena').fill('ClaveSegura@123');
  await page.getByLabel('Telefono').fill('987654321');
  await page.getByLabel('Departamento').selectOption({ label: 'Ayacucho' });
  await expect(page.getByLabel('Provincia')).toBeEnabled();
  await page.getByLabel('Provincia').selectOption({ label: 'Huamanga' });
  await expect(page.getByLabel('Distrito')).toBeEnabled();
  await page.getByLabel('Distrito').selectOption({ index: 1 });
  await page.getByRole('button', { name: 'Crear cuenta' }).click();
  await expect(page).toHaveURL(/\/login$/);
}

export async function iniciarSesion(page: Page, correo: string, password: string) {
  await page.goto('/login');
  await page.getByLabel('Correo electronico').fill(correo);
  await page.getByLabel('Contrasena').fill(password);
  await page.getByRole('button', { name: 'Iniciar sesion' }).click();
  await expect(page).toHaveURL(/\/dashboard$/);
}

async function publicarObjetoBasico(page: Page, titulo: string, imagenPath: string) {
  await page.goto('/publish');
  await page.getByLabel('Titulo').fill(titulo);
  await page.getByLabel('Descripcion', { exact: true }).fill('Descripcion de prueba con al menos veinte caracteres.');
  await page.getByLabel('Categoria').selectOption({ index: 1 });
  await page.getByLabel('Condicion fisica').selectOption('Bueno');
  await page.getByLabel('Departamento').selectOption({ label: 'Ayacucho' });
  await expect(page.getByLabel('Provincia')).toBeEnabled();
  await page.getByLabel('Provincia').selectOption({ label: 'Huamanga' });
  await expect(page.getByLabel('Distrito')).toBeEnabled();
  await page.getByLabel('Distrito').selectOption({ index: 1 });
  await page.locator('input[type="file"]').setInputFiles(imagenPath);
  await expect(page.getByRole('button', { name: 'Quitar imagen' })).toBeVisible();
  await page.getByRole('button', { name: 'Publicar objeto' }).click();
  await expect(page).toHaveURL(/\/objects\/[\w-]+$/);
}

export { publicarObjetoBasico };

// No hay proveedor SMTP configurado (LogEmailService, ver Backend): el
// backend registra el enlace de recuperacion en su log de archivo en vez de
// enviarlo de verdad. Esto permite una prueba de UI real (sin mocks) del
// flujo completo de "olvide mi contrasena", leyendo el enlace del mismo
// archivo que consultaria un desarrollador en desarrollo local.
export async function obtenerEnlaceRecuperacion(correo: string): Promise<string> {
  const fecha = new Date();
  const sufijo = `${fecha.getFullYear()}${String(fecha.getMonth() + 1).padStart(2, '0')}${String(
    fecha.getDate(),
  ).padStart(2, '0')}`;
  const rutaLog = path.resolve(__dirname, '../../Backend/src/ExchangePlatform.API/logs', `log-${sufijo}.txt`);

  const correoEscapado = correo.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  const patron = new RegExp(
    `\\[EMAIL SIMULADO\\] Para: ${correoEscapado} \\|[\\s\\S]*?enlace \\(valido por 1 hora\\): (\\S+)`,
    'g',
  );

  for (let intento = 0; intento < 10; intento++) {
    if (fs.existsSync(rutaLog)) {
      const contenido = fs.readFileSync(rutaLog, 'utf-8');
      let ultimoEnlace: string | undefined;
      let coincidencia: RegExpExecArray | null;
      while ((coincidencia = patron.exec(contenido)) !== null) {
        ultimoEnlace = coincidencia[1];
      }
      if (ultimoEnlace) return ultimoEnlace;
    }
    await new Promise((resolve) => setTimeout(resolve, 300));
  }

  throw new Error(`No se encontro el enlace de recuperacion para ${correo} en ${rutaLog}`);
}
