import Button from './Button';

interface PaginationProps {
  pageNumber: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
  onChange: (pagina: number) => void;
}

export default function Pagination({ pageNumber, totalPages, hasPrevious, hasNext, onChange }: PaginationProps) {
  if (totalPages <= 1) return null;

  return (
    <nav className="flex items-center justify-center gap-md" aria-label="Paginacion">
      <Button variant="secundario" disabled={!hasPrevious} onClick={() => onChange(pageNumber - 1)}>
        Anterior
      </Button>
      <span className="rounded-full bg-bg-alt px-md py-xs text-body-s font-medium text-text">
        Pagina {pageNumber} de {totalPages}
      </span>
      <Button variant="secundario" disabled={!hasNext} onClick={() => onChange(pageNumber + 1)}>
        Siguiente
      </Button>
    </nav>
  );
}
