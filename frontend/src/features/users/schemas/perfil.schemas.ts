import { z } from 'zod';

// Refleja ActualizarPerfilCommandValidator.cs.
export const perfilSchema = z.object({
  nombres: z.string().min(2, 'Minimo 2 caracteres.').max(100, 'Maximo 100 caracteres.'),
  apellidos: z.string().min(2, 'Minimo 2 caracteres.').max(100, 'Maximo 100 caracteres.'),
  telefono: z.string().regex(/^[0-9]{9}$/, 'El telefono debe tener 9 digitos.'),
  departamentoId: z.number().min(1, 'El departamento es requerido.'),
  provinciaId: z.number().min(1, 'La provincia es requerida.'),
  distritoId: z.number().min(1, 'El distrito es requerido.'),
});

export type PerfilForm = z.infer<typeof perfilSchema>;

// Refleja CambiarPasswordCommandValidator.cs.
export const cambiarPasswordSchema = z
  .object({
    passwordActual: z.string().min(1, 'La contrasena actual es requerida.'),
    passwordNuevo: z
      .string()
      .min(8, 'Minimo 8 caracteres.')
      .regex(/[A-Z]/, 'Debe tener al menos una mayuscula.')
      .regex(/[a-z]/, 'Debe tener al menos una minuscula.')
      .regex(/[0-9]/, 'Debe tener al menos un numero.')
      .regex(/[^A-Za-z0-9]/, 'Debe tener al menos un caracter especial.'),
    confirmPassword: z.string().min(1, 'La confirmacion es requerida.'),
  })
  .refine((datos) => datos.passwordNuevo === datos.confirmPassword, {
    message: 'Las contrasenas no coinciden.',
    path: ['confirmPassword'],
  });

export type CambiarPasswordForm = z.infer<typeof cambiarPasswordSchema>;
