import { useState, type FormEvent } from 'react';
import Button from '../../../components/ui/Button';
import Input from '../../../components/ui/Input';
import Select from '../../../components/ui/Select';
import { useCategorias, useDepartamentos, useDistritos, useProvincias } from '../../geo/hooks/useGeoSelects';
import type { ObjetosFiltroParams } from '../types/objeto.types';

interface SearchFiltersProps {
  filtros: ObjetosFiltroParams;
  onChange: (filtros: ObjetosFiltroParams) => void;
}

// Busqueda y filtros geograficos/categoria (API.md SS6, solo los que el
// backend real soporta hoy en ObjectsController.ObtenerObjetos).
export default function SearchFilters({ filtros, onChange }: SearchFiltersProps) {
  const [search, setSearch] = useState(filtros.search ?? '');

  const categorias = useCategorias();
  const departamentos = useDepartamentos();
  const provincias = useProvincias(filtros.departamentoId);
  const distritos = useDistritos(filtros.provinciaId);

  const buscar = (evento: FormEvent) => {
    evento.preventDefault();
    onChange({ ...filtros, search: search || undefined, pageNumber: 1 });
  };

  return (
    <div className="flex flex-col gap-md">
      <form onSubmit={buscar} className="flex items-end gap-sm">
        <div className="flex-1">
          <Input
            label="Buscar"
            placeholder="Titulo o descripcion..."
            value={search}
            onChange={(evento) => setSearch(evento.target.value)}
          />
        </div>
        <Button type="submit" variant="primario">
          Buscar
        </Button>
      </form>

      <div className="grid grid-cols-2 gap-sm sm:grid-cols-4">
        <Select
          label="Categoria"
          value={filtros.categoriaId ?? 0}
          onChange={(evento) =>
            onChange({ ...filtros, categoriaId: Number(evento.target.value) || undefined, pageNumber: 1 })
          }
        >
          <option value={0}>Todas</option>
          {categorias.map((categoria) => (
            <option key={categoria.id} value={categoria.id}>
              {categoria.nombre}
            </option>
          ))}
        </Select>

        <Select
          label="Departamento"
          value={filtros.departamentoId ?? 0}
          onChange={(evento) =>
            onChange({
              ...filtros,
              departamentoId: Number(evento.target.value) || undefined,
              provinciaId: undefined,
              distritoId: undefined,
              pageNumber: 1,
            })
          }
        >
          <option value={0}>Todos</option>
          {departamentos.map((departamento) => (
            <option key={departamento.id} value={departamento.id}>
              {departamento.nombre}
            </option>
          ))}
        </Select>

        <Select
          label="Provincia"
          disabled={!filtros.departamentoId}
          value={filtros.provinciaId ?? 0}
          onChange={(evento) =>
            onChange({
              ...filtros,
              provinciaId: Number(evento.target.value) || undefined,
              distritoId: undefined,
              pageNumber: 1,
            })
          }
        >
          <option value={0}>Todas</option>
          {provincias.map((provincia) => (
            <option key={provincia.id} value={provincia.id}>
              {provincia.nombre}
            </option>
          ))}
        </Select>

        <Select
          label="Distrito"
          disabled={!filtros.provinciaId}
          value={filtros.distritoId ?? 0}
          onChange={(evento) =>
            onChange({ ...filtros, distritoId: Number(evento.target.value) || undefined, pageNumber: 1 })
          }
        >
          <option value={0}>Todos</option>
          {distritos.map((distrito) => (
            <option key={distrito.id} value={distrito.id}>
              {distrito.nombre}
            </option>
          ))}
        </Select>
      </div>
    </div>
  );
}
