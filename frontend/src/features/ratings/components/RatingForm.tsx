import { useEffect, useState } from 'react';
import { Star } from 'lucide-react';
import Button from '../../../components/ui/Button';
import { useToast } from '../../../hooks/useToast';
import { useAuthStore } from '../../../stores/authStore';
import { obtenerMensajeError } from '../../../utils/apiError';
import { calificacionService } from '../services/calificacionService';

interface RatingFormProps {
  intercambioId: string;
  calificadoId: string;
}

const PUNTUACIONES = [1, 2, 3, 4, 5];

// Calificar tras intercambio completado (UC-022, UX.md SS5.3).
export default function RatingForm({ intercambioId, calificadoId }: RatingFormProps) {
  const usuario = useAuthStore((state) => state.usuario);
  const { mostrar } = useToast();
  const [yaCalifico, setYaCalifico] = useState<boolean | null>(null);
  const [puntuacion, setPuntuacion] = useState(0);
  const [comentario, setComentario] = useState('');
  const [enviando, setEnviando] = useState(false);
  const [enviado, setEnviado] = useState(false);

  useEffect(() => {
    if (!usuario) return;
    calificacionService
      .obtenerPorUsuario(calificadoId)
      .then(({ data }) => {
        const califico = (data.data ?? []).some(
          (calificacion) => calificacion.intercambioId === intercambioId && calificacion.calificadorId === usuario.id,
        );
        setYaCalifico(califico);
      })
      .catch(() => setYaCalifico(false));
  }, [calificadoId, intercambioId, usuario]);

  const enviar = async () => {
    if (puntuacion < 1) {
      mostrar('Selecciona una puntuacion.', 'warning');
      return;
    }
    setEnviando(true);
    try {
      await calificacionService.crear({ intercambioId, puntuacion, comentario: comentario || undefined });
      mostrar('Calificacion registrada exitosamente.', 'success');
      setEnviado(true);
    } catch (error) {
      mostrar(obtenerMensajeError(error, 'No pudimos registrar tu calificacion.'), 'error');
    } finally {
      setEnviando(false);
    }
  };

  if (yaCalifico === null) return null;

  if (yaCalifico || enviado) {
    return <p className="text-body-s text-text-secondary">Ya calificaste este intercambio.</p>;
  }

  return (
    <div className="flex flex-col gap-sm rounded-lg bg-bg p-md shadow-card">
      <h3 className="text-h3 text-text">Califica a la otra parte</h3>
      <div className="flex gap-xs" role="radiogroup" aria-label="Puntuacion">
        {PUNTUACIONES.map((valor) => (
          <button
            key={valor}
            type="button"
            role="radio"
            aria-checked={puntuacion === valor}
            aria-label={`${valor} estrellas`}
            onClick={() => setPuntuacion(valor)}
            className="flex min-h-11 min-w-11 items-center justify-center focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2"
          >
            <Star
              size={24}
              className={valor <= puntuacion ? 'fill-warning text-warning' : 'text-text-secondary'}
              aria-hidden="true"
            />
          </button>
        ))}
      </div>
      <label htmlFor="comentario-calificacion" className="text-label font-medium text-text">
        Comentario (opcional)
      </label>
      <textarea
        id="comentario-calificacion"
        rows={3}
        maxLength={500}
        value={comentario}
        onChange={(evento) => setComentario(evento.target.value)}
        className="rounded-md border border-text-secondary/40 px-md py-sm text-body text-text focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2"
      />
      <Button variant="primario" onClick={enviar} disabled={enviando}>
        {enviando ? 'Enviando...' : 'Enviar calificacion'}
      </Button>
    </div>
  );
}
