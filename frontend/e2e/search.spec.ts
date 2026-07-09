import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { expect, test } from '@playwright/test';
import { iniciarSesion, publicarObjetoBasico, registrarUsuario } from './helpers';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const IMAGEN_PRUEBA = path.resolve(__dirname, 'fixtures/imagen-prueba.png');
const CLAVE = 'ClaveSegura@123';

test.describe('Busqueda de objetos', () => {
  test('lista objetos publicados sin necesitar sesion', async ({ page }) => {
    await page.goto('/search');

    await expect(page.getByRole('heading', { name: 'Buscar objetos' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Ver detalle' }).first()).toBeVisible();
  });

  test('filtra resultados por texto de busqueda', async ({ page }) => {
    // Autónomo y con títulos únicos por corrida: publica los dos objetos que
    // luego filtra (en vez de asumir datos demo preexistentes) y usa un sufijo
    // único para que el filtro devuelva exactamente una coincidencia, sin
    // chocar con objetos de corridas anteriores (strict-mode de Playwright).
    const sufijo = Date.now();
    const correo = `e2e-buscar-${sufijo}@example.com`;
    const tituloTablet = `Tablet Samsung ${sufijo}`;
    const tituloLaptop = `Laptop HP ${sufijo}`;
    await registrarUsuario(page, correo);
    await iniciarSesion(page, correo, CLAVE);
    await publicarObjetoBasico(page, tituloTablet, IMAGEN_PRUEBA);
    await publicarObjetoBasico(page, tituloLaptop, IMAGEN_PRUEBA);

    await page.goto('/search');
    await page.getByLabel('Buscar').fill(tituloTablet);
    await page.getByRole('button', { name: 'Buscar' }).click();

    await expect(page.getByRole('heading', { name: tituloTablet })).toBeVisible();
    await expect(page.getByRole('heading', { name: tituloLaptop })).not.toBeVisible();
  });

  test('navega al detalle de un objeto desde la tarjeta', async ({ page }) => {
    await page.goto('/search');

    const tituloTarjeta = await page.getByRole('heading', { level: 3 }).first().textContent();
    await page.getByRole('link', { name: 'Ver detalle' }).first().click();

    await expect(page).toHaveURL(/\/objects\/[\w-]+$/);
    await expect(page.getByRole('heading', { level: 1, name: tituloTarjeta ?? '' })).toBeVisible();
  });
});
