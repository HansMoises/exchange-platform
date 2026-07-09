import { useEffect, useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import Button from '../../../components/ui/Button';
import EmptyState from '../../../components/ui/EmptyState';
import Loading from '../../../components/ui/Loading';
import Select from '../../../components/ui/Select';
import { useToast } from '../../../hooks/useToast';
import { obtenerMensajeError } from '../../../utils/apiError';
import { objetoService } from '../../objects/services/objetoService';
import type { ObjetoDto } from '../../objects/types/objeto.types';
import { intercambioService } from '../services/intercambioService';

interface ProposeExchangeFormProps {
  objetoSolicitado: ObjetoDto;
  onExito: (intercambioId: string) => void;
}

// Proponer intercambio (UC-020, UX.md SS5.2): si no tiene objetos disponibles,
// se invita a publicar primero.
export default function ProposeExchangeForm({ objetoSolicitado, onExito }: ProposeExchangeFormProps) {
  const navigate = useNavigate();
  const { mostrar } = useToast();
  const [misObjetos, setMisObjetos] = useState<ObjetoDto[]>([]);
  const [cargando, setCargando] = useState(true);
  const [objetoOfrecidoId, setObjetoOfrecidoId] = useState('');
  const [mensaje, setMensaje] = useState('');
  const [enviando, setEnviando] = useState(false);

  useEffect(() => {
    objetoService
      .obtenerMisObjetosDisponibles()
      .then(({ data }) => setMisObjetos(data.data ?? []))
      .catch(() => setMisObjetos([]))
      .finally(() => setCargando(false));
  }, []);

  const enviar = async (evento: FormEvent) => {
    evento.preventDefault();
    if (!objetoOfrecidoId) {
      mostrar('Selecciona un objeto para ofrecer.', 'warning');
      return;
    }
    setEnviando(true);
    try {
      const { data } = await intercambioService.crear({
        objetoSolicitadoId: objetoSolicitado.id,
        objetoOfrecidoId,
        mensajeInicial: mensaje || undefined,
      });
      mostrar('Solicitud de intercambio enviada exitosamente.', 'success');
      if (data.data?.id) onExito(data.data.id);
    } catch (error) {
      mostrar(obtenerMensajeError(error, 'No pudimos enviar la solicitud.'), 'error');
    } finally {
      setEnviando(false);
    }
  };

  if (cargando) return <Loading lineas={2} />;

  if (misObjetos.length === 0) {
    return (
      <EmptyState
        mensaje="No tienes objetos disponibles para ofrecer."
        accion="Publicar un objeto"
        onAccion={() => navigate('/publish')}
      />
    );
  }

  return (
    <form onSubmit={enviar} className="flex flex-col gap-sm rounded-lg bg-bg p-md shadow-card">
      <Select
        label="Tu objeto a ofrecer"
        value={objetoOfrecidoId}
        onChange={(evento) => setObjetoOfrecidoId(evento.target.value)}
      >
        <option value="">Selecciona un objeto</option>
        {misObjetos.map((objeto) => (
          <option key={objeto.id} value={objeto.id}>
            {objeto.titulo}
          </option>
        ))}
      </Select>

      <label htmlFor="mensaje-intercambio" className="text-label font-medium text-text">
        Mensaje (opcional)
      </label>
      <textarea
        id="mensaje-intercambio"
        rows={3}
        maxLength={500}
        value={mensaje}
        onChange={(evento) => setMensaje(evento.target.value)}
        className="rounded-md border border-text-secondary/40 px-md py-sm text-body text-text focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2"
      />

      <Button type="submit" variant="primario" disabled={enviando}>
        {enviando ? 'Enviando...' : 'Enviar solicitud'}
      </Button>
    </form>
  );
}
