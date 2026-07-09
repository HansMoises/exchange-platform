import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { expect, test } from '@playwright/test';
import { iniciarSesion, publicarObjetoBasico, registrarUsuario } from './helpers';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const IMAGEN_PRUEBA = path.resolve(__dirname, 'fixtures/imagen-prueba.png');
const CLAVE = 'ClaveSegura@123';

test.describe('Publicar objeto', () => {
  test('publica un objeto con imagen real y navega al detalle', async ({ page }) => {
    const correo = `e2e-publicar-${Date.now()}@example.com`;
    await registrarUsuario(page, correo);
    await iniciarSesion(page, correo, CLAVE);

    const titulo = `Objeto E2E ${Date.now()}`;
    await publicarObjetoBasico(page, titulo, IMAGEN_PRUEBA);

    await expect(page.getByRole('heading', { name: titulo })).toBeVisible();
    await expect(page.getByText('Disponible')).toBeVisible();
  });

  test('el objeto publicado aparece luego en el listado propio', async ({ page }) => {
    const correo = `e2e-mis-objetos-${Date.now()}@example.com`;
    await registrarUsuario(page, correo);
    await iniciarSesion(page, correo, CLAVE);

    const titulo = `Mi objeto E2E ${Date.now()}`;
    await publicarObjetoBasico(page, titulo, IMAGEN_PRUEBA);

    await page.goto('/search');
    await page.getByLabel('Buscar').fill(titulo);
    await page.getByRole('button', { name: 'Buscar' }).click();

    await expect(page.getByRole('heading', { name: titulo })).toBeVisible();
  });
});
