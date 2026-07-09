import { useState } from 'react';
import { Flag, Star } from 'lucide-react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import Button from '../components/ui/Button';
import EmptyState from '../components/ui/EmptyState';
import Loading from '../components/ui/Loading';
import ProposeExchangeForm from '../features/exchanges/components/ProposeExchangeForm';
import FavoriteButton from '../features/favorites/components/FavoriteButton';
import EstadoObjetoBadge from '../features/objects/components/EstadoObjetoBadge';
import { useObjeto } from '../features/objects/hooks/useObjeto';
import { objetoService } from '../features/objects/services/objetoService';
import ReportForm from '../features/reports/components/ReportForm';
import { useDocumentTitle } from '../hooks/useDocumentTitle';
import { useToast } from '../hooks/useToast';
import { useAuthStore } from '../stores/authStore';
import { obtenerMensajeError } from '../utils/apiError';

export default function ObjectDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { objeto, isLoading, error } = useObjeto(id);
  useDocumentTitle(objeto?.titulo ?? 'Detalle de objeto');
  const usuario = useAuthStore((state) => state.usuario);
  const { mostrar } = useToast();
  const [eliminando, setEliminando] = useState(false);
  const [mostrarFormularioIntercambio, setMostrarFormularioIntercambio] = useState(false);
  const [mostrarFormularioReporte, setMostrarFormularioReporte] = useState(false);

  if (isLoading) return <Loading lineas={6} className="p-lg" />;
  if (error || !objeto) return <EmptyState mensaje="No encontramos este objeto." />;

  const esPropietario = usuario?.id === objeto.usuarioId;
  const puedeProponerIntercambio = !esPropietario && usuario && objeto.estado === 'Disponible';
  const puedeReportar = !esPropietario && usuario;

  const eliminar = async () => {
    if (!window.confirm('Seguro que deseas eliminar este objeto?')) return;
    setEliminando(true);
    try {
      await objetoService.eliminar(objeto.id);
      mostrar('Objeto eliminado exitosamente.', 'success');
      navigate('/dashboard');
    } catch (error_) {
      mostrar(obtenerMensajeError(error_, 'No pudimos eliminar el objeto.'), 'error');
    } finally {
      setEliminando(false);
    }
  };

  return (
    <div className="mx-auto flex max-w-4xl animate-fade-in flex-col gap-lg p-lg">
      <div className="grid grid-cols-1 gap-md sm:grid-cols-2">
        {objeto.imagenes.length > 0 ? (
          objeto.imagenes.map((imagen) => (
            <img
              key={imagen.id}
              src={imagen.url}
              alt={objeto.titulo}
              className="aspect-video w-full rounded-lg object-cover shadow-card transition-transform duration-300 hover:scale-[1.02]"
            />
          ))
        ) : (
          <div className="flex aspect-video items-center justify-center rounded-lg bg-bg-alt text-text-secondary">
            Sin imagenes
          </div>
        )}
      </div>

      <div className="flex flex-col gap-sm rounded-lg bg-bg p-lg shadow-card">
        <div className="flex items-center gap-sm">
          <h1 className="font-display text-h1 font-bold text-text">{objeto.titulo}</h1>
          <EstadoObjetoBadge estado={objeto.estado} />
          {!esPropietario && usuario && <FavoriteButton objetoId={objeto.id} />}
        </div>
        <p className="text-body-s text-text-secondary">
          {objeto.categoriaNombre} · {objeto.condicionFisica}
        </p>
        <p className="text-body text-text">{objeto.descripcion}</p>
        <div className="flex items-center gap-xs text-body-s text-text">
          <Star size={16} className="text-warning" aria-hidden="true" />
          {objeto.usuarioCalificacion.toFixed(1)} · {objeto.usuarioNombres}
        </div>
      </div>

      {esPropietario && (
        <div className="flex gap-sm">
          <Link to={`/objects/${objeto.id}/edit`}>
            <Button variant="secundario">Editar</Button>
          </Link>
          <Button variant="peligro" onClick={eliminar} disabled={eliminando}>
            {eliminando ? 'Eliminando...' : 'Eliminar'}
          </Button>
        </div>
      )}

      {puedeProponerIntercambio &&
        (mostrarFormularioIntercambio ? (
          <ProposeExchangeForm
            objetoSolicitado={objeto}
            onExito={(intercambioId) => navigate(`/exchanges/${intercambioId}`)}
          />
        ) : (
          <Button variant="primario" onClick={() => setMostrarFormularioIntercambio(true)}>
            Proponer intercambio
          </Button>
        ))}

      {puedeReportar &&
        (mostrarFormularioReporte ? (
          <ReportForm entidadTipo="Objeto" entidadId={objeto.id} onExito={() => setMostrarFormularioReporte(false)} />
        ) : (
          <Button variant="texto" onClick={() => setMostrarFormularioReporte(true)}>
            <Flag size={16} aria-hidden="true" />
            Reportar objeto
          </Button>
        ))}
    </div>
  );
}
