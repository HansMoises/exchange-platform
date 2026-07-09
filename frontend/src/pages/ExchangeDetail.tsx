import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import Button from '../components/ui/Button';
import EmptyState from '../components/ui/EmptyState';
import Loading from '../components/ui/Loading';
import EstadoIntercambioBadge from '../features/exchanges/components/EstadoIntercambioBadge';
import { intercambioService } from '../features/exchanges/services/intercambioService';
import type { EstadoIntercambio, IntercambioDto } from '../features/exchanges/types/intercambio.types';
import RatingForm from '../features/ratings/components/RatingForm';
import { useDocumentTitle } from '../hooks/useDocumentTitle';
import { useToast } from '../hooks/useToast';
import { useAuthStore } from '../stores/authStore';
import { obtenerMensajeError } from '../utils/apiError';

const ESTADOS_ACTIVOS: EstadoIntercambio[] = ['Pendiente', 'Aceptado', 'PendienteConfirmacion'];

export default function ExchangeDetail() {
  useDocumentTitle('Intercambio');
  const { id } = useParams<{ id: string }>();
  const usuario = useAuthStore((state) => state.usuario);
  const { mostrar } = useToast();
  const [intercambio, setIntercambio] = useState<IntercambioDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(false);
  const [procesando, setProcesando] = useState(false);

  const cargar = () => {
    if (!id) return;
    setIsLoading(true);
    setError(false);
    intercambioService
      .obtener(id)
      .then(({ data }) => setIntercambio(data.data))
      .catch(() => setError(true))
      .finally(() => setIsLoading(false));
  };

  useEffect(cargar, [id]);

  if (isLoading) return <Loading lineas={6} className="p-lg" />;
  if (error || !intercambio || !usuario) return <EmptyState mensaje="No encontramos este intercambio." />;

  const esPropietario = usuario.id === intercambio.usuarioPropietarioId;
  const esSolicitante = usuario.id === intercambio.usuarioSolicitanteId;
  const yaConfirmo = esSolicitante ? intercambio.confirmacionSolicitante : intercambio.confirmacionPropietario;
  const otraParteId = esSolicitante ? intercambio.usuarioPropietarioId : intercambio.usuarioSolicitanteId;
  const otraParteNombre = esSolicitante ? intercambio.usuarioPropietarioNombres : intercambio.usuarioSolicitanteNombres;
  const puedeCancelar = ESTADOS_ACTIVOS.includes(intercambio.estado);

  const ejecutar = async (accion: () => Promise<unknown>, mensajeExito: string) => {
    setProcesando(true);
    try {
      await accion();
      mostrar(mensajeExito, 'success');
      cargar();
    } catch (error_) {
      mostrar(obtenerMensajeError(error_, 'No pudimos completar la accion.'), 'error');
    } finally {
      setProcesando(false);
    }
  };

  return (
    <div className="mx-auto flex max-w-2xl animate-fade-in flex-col gap-lg p-lg">
      <div className="flex items-center gap-sm">
        <h1 className="font-display text-h2 font-bold text-text">Intercambio</h1>
        <EstadoIntercambioBadge estado={intercambio.estado} />
      </div>

      <div className="flex flex-col gap-xs rounded-lg bg-bg p-md shadow-card">
        <p className="text-body text-text">
          <strong>{intercambio.objetoSolicitadoTitulo}</strong> por <strong>{intercambio.objetoOfrecidoTitulo}</strong>
        </p>
        <p className="text-body-s text-text-secondary">Con {otraParteNombre}</p>
        {intercambio.mensajeInicial && <p className="text-body-s text-text">"{intercambio.mensajeInicial}"</p>}
      </div>

      {intercambio.estado === 'Pendiente' && esPropietario && (
        <div className="flex gap-sm">
          <Button
            variant="primario"
            disabled={procesando}
            onClick={() =>
              ejecutar(() => intercambioService.aceptar(intercambio.id), 'Solicitud aceptada exitosamente.')
            }
          >
            Aceptar
          </Button>
          <Button
            variant="peligro"
            disabled={procesando}
            onClick={() => ejecutar(() => intercambioService.rechazar(intercambio.id), 'Solicitud rechazada.')}
          >
            Rechazar
          </Button>
        </div>
      )}

      {(intercambio.estado === 'Aceptado' || intercambio.estado === 'PendienteConfirmacion') && !yaConfirmo && (
        <Button
          variant="primario"
          disabled={procesando}
          onClick={() => ejecutar(() => intercambioService.confirmar(intercambio.id), 'Confirmacion registrada.')}
        >
          Confirmar recepcion
        </Button>
      )}

      {puedeCancelar && (
        <Button
          variant="texto"
          disabled={procesando}
          onClick={() => ejecutar(() => intercambioService.cancelar(intercambio.id), 'Intercambio cancelado.')}
        >
          Cancelar intercambio
        </Button>
      )}

      {intercambio.estado === 'Completado' && <RatingForm intercambioId={intercambio.id} calificadoId={otraParteId} />}
    </div>
  );
}
