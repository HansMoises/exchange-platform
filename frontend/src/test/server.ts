import { setupServer } from 'msw/node';

// Servidor MSW compartido entre pruebas; cada test agrega sus propios
// handlers con server.use(...) (Testing.md SS3.2).
export const server = setupServer();
