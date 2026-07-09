import { Link } from 'react-router-dom';
import Button from '../../../components/ui/Button';
import type { IntercambioDto } from '../types/intercambio.types';
import EstadoIntercambioBadge from './EstadoIntercambioBadge';

interface ExchangeCardProps {
  intercambio: IntercambioDto;
  usuarioActualId: string;
}

export default function ExchangeCard({ intercambio, usuarioActualId }: ExchangeCardProps) {
  const esSolicitante = intercambio.usuarioSolicitanteId === usuarioActualId;
  const otraParteNombre = esSolicitante ? intercambio.usuarioPropietarioNombres : intercambio.usuarioSolicitanteNombres;

  return (
    <article className="flex flex-col gap-sm rounded-lg bg-bg p-md shadow-card">
      <div className="flex items-center justify-between">
        <span className="text-caption text-text-secondary">{esSolicitante ? 'Enviada' : 'Recibida'}</span>
        <EstadoIntercambioBadge estado={intercambio.estado} />
      </div>
      <p className="text-body text-text">
        <strong>{intercambio.objetoSolicitadoTitulo}</strong> por <strong>{intercambio.objetoOfrecidoTitulo}</strong>
      </p>
      <p className="text-body-s text-text-secondary">Con {otraParteNombre}</p>
      <Link to={`/exchanges/${intercambio.id}`}>
        <Button variant="secundario" className="w-full">
          Ver detalle
        </Button>
      </Link>
    </article>
  );
}
