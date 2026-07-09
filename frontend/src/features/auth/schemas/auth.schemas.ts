import { z } from 'zod';

// Refleja las reglas del backend (RegistrarUsuarioCommandValidator, Seguridad.md SS3);
// no las reemplaza (Frontend.md SS6).
const passwordSchema = z
  .string()
  .min(8, 'Minimo 8 caracteres.')
  .max(100, 'Maximo 100 caracteres.')
  .regex(/[A-Z]/, 'Debe tener al menos una mayuscula.')
  .regex(/[a-z]/, 'Debe tener al menos una minuscula.')
  .regex(/[0-9]/, 'Debe tener al menos un numero.')
  .regex(/[^A-Za-z0-9]/, 'Debe tener al menos un caracter especial.');

export const registroSchema = z
  .object({
    nombres: z.string().min(2, 'Minimo 2 caracteres.').max(100, 'Maximo 100 caracteres.'),
    apellidos: z.string().min(2, 'Minimo 2 caracteres.').max(100, 'Maximo 100 caracteres.'),
    email: z.string().email('Formato de correo invalido.').max(256, 'Maximo 256 caracteres.'),
    password: passwordSchema,
    confirmPassword: z.string().min(1, 'La confirmacion es requerida.'),
    telefono: z.string().regex(/^[0-9]{9}$/, 'El telefono debe tener 9 digitos.'),
    departamentoId: z.number().min(1, 'El departamento es requerido.'),
    provinciaId: z.number().min(1, 'La provincia es requerida.'),
    distritoId: z.number().min(1, 'El distrito es requerido.'),
  })
  .refine((datos) => datos.password === datos.confirmPassword, {
    message: 'Las contrasenas no coinciden.',
    path: ['confirmPassword'],
  });

export type RegistroForm = z.infer<typeof registroSchema>;

export const loginSchema = z.object({
  email: z.string().email('Formato de correo invalido.'),
  password: z.string().min(1, 'La contrasena es requerida.'),
});

export type LoginForm = z.infer<typeof loginSchema>;

export const forgotPasswordSchema = z.object({
  email: z.string().email('Formato de correo invalido.'),
});

export type ForgotPasswordForm = z.infer<typeof forgotPasswordSchema>;

export const resetPasswordSchema = z
  .object({
    password: passwordSchema,
    confirmPassword: z.string().min(1, 'La confirmacion es requerida.'),
  })
  .refine((datos) => datos.password === datos.confirmPassword, {
    message: 'Las contrasenas no coinciden.',
    path: ['confirmPassword'],
  });

export type ResetPasswordForm = z.infer<typeof resetPasswordSchema>;
