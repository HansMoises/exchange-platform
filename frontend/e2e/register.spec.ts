import { expect, test, type Page } from '@playwright/test';

async function llenarFormularioRegistro(page: Page, correo: string) {
  await page.getByLabel('Nombres').fill('Rosa');
  await page.getByLabel('Apellidos').fill('Quispe');
  await page.getByLabel('Correo electronico').fill(correo);
  await page.getByLabel('Contrasena', { exact: true }).fill('ClaveSegura@123');
  await page.getByLabel('Confirmar contrasena').fill('ClaveSegura@123');
  await page.getByLabel('Telefono').fill('987654321');

  await page.getByLabel('Departamento').selectOption({ label: 'Ayacucho' });
  await expect(page.getByLabel('Provincia')).toBeEnabled();
  await page.getByLabel('Provincia').selectOption({ label: 'Huamanga' });
  await expect(page.getByLabel('Distrito')).toBeEnabled();
  await page.getByLabel('Distrito').selectOption({ index: 1 });
}

test.describe('Registro', () => {
  test('crea una cuenta con los selects en cascada y navega a login', async ({ page }) => {
    await page.goto('/register');
    await expect(page.getByRole('heading', { name: 'Crear cuenta' })).toBeVisible();

    await llenarFormularioRegistro(page, `e2e-${Date.now()}@example.com`);
    await page.getByRole('button', { name: 'Crear cuenta' }).click();

    await expect(page).toHaveURL(/\/login$/);
  });

  test('rechaza un correo ya registrado', async ({ page }) => {
    const correo = `e2e-duplicado-${Date.now()}@example.com`;

    await page.goto('/register');
    await llenarFormularioRegistro(page, correo);
    await page.getByRole('button', { name: 'Crear cuenta' }).click();
    await expect(page).toHaveURL(/\/login$/);

    // Segundo intento con el mismo correo: debe fallar y quedarse en /register.
    await page.goto('/register');
    await llenarFormularioRegistro(page, correo);
    await page.getByRole('button', { name: 'Crear cuenta' }).click();

    await expect(page).toHaveURL(/\/register$/);
  });
});
