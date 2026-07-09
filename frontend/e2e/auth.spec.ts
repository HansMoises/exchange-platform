import { expect, test } from '@playwright/test';
import { iniciarSesion, obtenerEnlaceRecuperacion, registrarUsuario } from './helpers';

test.describe('Login', () => {
  test('muestra errores de validacion con el formulario vacio', async ({ page }) => {
    await page.goto('/login');

    await expect(page.getByRole('heading', { name: 'Iniciar sesion' })).toBeVisible();

    await page.getByRole('button', { name: 'Iniciar sesion' }).click();

    await expect(page.getByText('Formato de correo invalido.')).toBeVisible();
    await expect(page.getByText('La contrasena es requerida.')).toBeVisible();
  });

  test('navega a registro desde login', async ({ page }) => {
    await page.goto('/login');
    await page.getByRole('link', { name: 'Registrate' }).click();

    await expect(page).toHaveURL(/\/register$/);
    await expect(page.getByRole('heading', { name: 'Crear cuenta' })).toBeVisible();
  });
});

test.describe('Recuperacion de contrasena', () => {
  test('flujo completo: olvide mi contrasena -> restablecer -> login con la nueva', async ({ page }) => {
    const correo = `e2e-recuperar-${Date.now()}@example.com`;
    await registrarUsuario(page, correo);

    await page.goto('/forgot-password');
    await page.getByLabel('Correo electronico').fill(correo);
    await page.getByRole('button', { name: 'Enviar instrucciones' }).click();

    await expect(page.getByText('Si el correo esta registrado, recibiras instrucciones en breve.')).toBeVisible();

    const enlace = await obtenerEnlaceRecuperacion(correo);
    await page.goto(enlace);

    await expect(page.getByRole('heading', { name: 'Restablecer contrasena' })).toBeVisible();
    await page.getByLabel('Nueva contrasena', { exact: true }).fill('ClaveNueva@456');
    await page.getByLabel('Confirmar nueva contrasena').fill('ClaveNueva@456');
    await page.getByRole('button', { name: 'Restablecer contrasena' }).click();

    await expect(page).toHaveURL(/\/login$/);

    await iniciarSesion(page, correo, 'ClaveNueva@456');
  });

  test('muestra un mensaje de enlace invalido sin token', async ({ page }) => {
    await page.goto('/reset-password');
    await expect(page.getByText('El enlace de recuperacion no es valido.')).toBeVisible();
  });
});
