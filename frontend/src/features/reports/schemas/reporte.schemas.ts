import { z } from 'zod';

// Refleja CrearReporteCommandValidator.cs.
export const reporteSchema = z.object({
  motivo: z.enum(['ContenidoInapropiado', 'Fraude', 'Spam', 'InformacionFalsa', 'Otro'], {
    message: 'Selecciona un motivo valido.',
  }),
  descripcion: z.string().max(500, 'Maximo 500 caracteres.').optional(),
});

export type ReporteForm = z.infer<typeof reporteSchema>;
