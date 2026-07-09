import { z } from 'zod';

// Refleja las reglas reales del backend (CrearObjetoCommandValidator.cs).
export const objetoSchema = z.object({
  titulo: z.string().min(5, 'Minimo 5 caracteres.').max(100, 'Maximo 100 caracteres.'),
  descripcion: z.string().min(20, 'Minimo 20 caracteres.').max(1000, 'Maximo 1000 caracteres.'),
  categoriaId: z.number().min(1, 'La categoria es requerida.'),
  condicionFisica: z.enum(['Nuevo', 'Bueno', 'Regular'], {
    message: 'Selecciona una condicion valida.',
  }),
  departamentoId: z.number().min(1, 'El departamento es requerido.'),
  provinciaId: z.number().min(1, 'La provincia es requerida.'),
  distritoId: z.number().min(1, 'El distrito es requerido.'),
});

export type ObjetoFormBase = z.infer<typeof objetoSchema>;

// Al crear, ademas se exigen 1-5 imagenes ya subidas (RN-011/012).
export const crearObjetoSchema = objetoSchema.extend({
  imagenesUrl: z.array(z.string()).min(1, 'Debes subir al menos 1 imagen.').max(5, 'Maximo 5 imagenes.'),
});

export type CrearObjetoForm = z.infer<typeof crearObjetoSchema>;
