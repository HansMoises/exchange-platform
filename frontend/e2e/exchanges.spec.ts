import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { expect, test } from '@playwright/test';
import { iniciarSesion, publicarObjetoBasico, registrarUsuario } from './helpers';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const IMAGEN_PRUEBA = path.resolve(__dirname, 'fixtures/imagen-prueba.png');
const CLAVE = 'ClaveSegura@123';

test('flujo completo de intercambio: proponer, aceptar y confirmar ambas partes', async ({ browser }) => {
  // Dos sesiones de navegador independientes: propietario y solicitante.
  const contextoPropietario = await browser.newContext();
  const contextoSolicitante = await browser.newContext();
  const paginaPropietario = await contextoPropietario.newPage();
  const paginaSolicitante = await contextoSolicitante.newPage();

  const correoPropietario = `e2e-propietario-${Date.now()}@example.com`;
  const correoSolicitante = `e2e-solicitante-${Date.now()}@example.com`;

  await registrarUsuario(paginaPropietario, correoPropietario);
  await iniciarSesion(paginaPropietario, correoPropietario, CLAVE);

  await registrarUsuario(paginaSolicitante, correoSolicitante);
  await iniciarSesion(paginaSolicitante, correoSolicitante, CLAVE);

  const tituloSolicitado = `Objeto solicitado E2E ${Date.now()}`;
  const tituloOfrecido = `Objeto ofrecido E2E ${Date.now()}`;

  await publicarObjetoBasico(paginaPropietario, tituloSolicitado, IMAGEN_PRUEBA);
  await publicarObjetoBasico(paginaSolicitante, tituloOfrecido, IMAGEN_PRUEBA);

  // El solicitante encuentra el objeto del propietario y propone intercambio.
  await paginaSolicitante.goto('/search');
  await paginaSolicitante.getByLabel('Buscar').fill(tituloSolicitado);
  await paginaSolicitante.getByRole('button', { name: 'Buscar' }).click();
  const tarjetaSolicitada = paginaSolicitante.locator('article', { hasText: tituloSolicitado });
  await tarjetaSolicitada.getByRole('link', { name: 'Ver detalle' }).click();

  await paginaSolicitante.getByRole('button', { name: 'Proponer intercambio' }).click();
  await paginaSolicitante.getByLabel('Tu objeto a ofrecer').selectOption({ label: tituloOfrecido });
  await paginaSolicitante.getByRole('button', { name: 'Enviar solicitud' }).click();

  await expect(paginaSolicitante).toHaveURL(/\/exchanges\/[\w-]+$/);
  const urlIntercambio = paginaSolicitante.url();

  // El propietario ve la solicitud pendiente y la acepta.
  await paginaPropietario.goto(urlIntercambio);
  await expect(paginaPropietario.getByText('Pendiente')).toBeVisible();
  await paginaPropietario.getByRole('button', { name: 'Aceptar' }).click();
  await expect(paginaPropietario.getByText('Aceptado')).toBeVisible();

  // Ambas partes confirman: el propietario primero.
  await paginaPropietario.getByRole('button', { name: 'Confirmar recepcion' }).click();
  await expect(paginaPropietario.getByText('Por confirmar')).toBeVisible();

  // El solicitante recarga para ver el estado actualizado y confirma tambien.
  await paginaSolicitante.reload();
  await paginaSolicitante.getByRole('button', { name: 'Confirmar recepcion' }).click();

  await expect(paginaSolicitante.getByText('Completado')).toBeVisible();

  await contextoPropietario.close();
  await contextoSolicitante.close();
});

test('el propietario puede rechazar una solicitud pendiente', async ({ browser }) => {
  const contextoPropietario = await browser.newContext();
  const contextoSolicitante = await browser.newContext();
  const paginaPropietario = await contextoPropietario.newPage();
  const paginaSolicitante = await contextoSolicitante.newPage();

  const correoPropietario = `e2e-rechaza-propietario-${Date.now()}@example.com`;
  const correoSolicitante = `e2e-rechaza-solicitante-${Date.now()}@example.com`;

  await registrarUsuario(paginaPropietario, correoPropietario);
  await iniciarSesion(paginaPropietario, correoPropietario, CLAVE);
  await registrarUsuario(paginaSolicitante, correoSolicitante);
  await iniciarSesion(paginaSolicitante, correoSolicitante, CLAVE);

  const tituloSolicitado = `Objeto a rechazar E2E ${Date.now()}`;
  const tituloOfrecido = `Objeto ofrecido rechazo E2E ${Date.now()}`;

  await publicarObjetoBasico(paginaPropietario, tituloSolicitado, IMAGEN_PRUEBA);
  await publicarObjetoBasico(paginaSolicitante, tituloOfrecido, IMAGEN_PRUEBA);

  await paginaSolicitante.goto('/search');
  await paginaSolicitante.getByLabel('Buscar').fill(tituloSolicitado);
  await paginaSolicitante.getByRole('button', { name: 'Buscar' }).click();
  const tarjetaSolicitada = paginaSolicitante.locator('article', { hasText: tituloSolicitado });
  await tarjetaSolicitada.getByRole('link', { name: 'Ver detalle' }).click();
  await paginaSolicitante.getByRole('button', { name: 'Proponer intercambio' }).click();
  await paginaSolicitante.getByLabel('Tu objeto a ofrecer').selectOption({ label: tituloOfrecido });
  await paginaSolicitante.getByRole('button', { name: 'Enviar solicitud' }).click();
  await expect(paginaSolicitante).toHaveURL(/\/exchanges\/[\w-]+$/);

  await paginaPropietario.goto(paginaSolicitante.url());
  await paginaPropietario.getByRole('button', { name: 'Rechazar' }).click();

  await expect(paginaPropietario.getByText('Rechazado')).toBeVisible();

  await contextoPropietario.close();
  await contextoSolicitante.close();
});
