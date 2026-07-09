import { useEffect, useState } from 'react';
import Button from '../components/ui/Button';
import EmptyState from '../components/ui/EmptyState';
import Input from '../components/ui/Input';
import Loading from '../components/ui/Loading';
import { adminService } from '../features/admin/services/adminService';
import type { CategoriaAdminDto } from '../features/admin/types/admin.types';
import { useDocumentTitle } from '../hooks/useDocumentTitle';
import { useToast } from '../hooks/useToast';
import { obtenerMensajeError } from '../utils/apiError';

interface FormState {
  nombre: string;
  descripcion: string;
  icono: string;
}

const FORM_VACIO: FormState = { nombre: '', descripcion: '', icono: '' };

export default function AdminCategories() {
  useDocumentTitle('Gestion de categorias');
  const { mostrar } = useToast();
  const [categorias, setCategorias] = useState<CategoriaAdminDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(false);
  const [procesandoId, setProcesandoId] = useState<number | null>(null);
  const [editandoId, setEditandoId] = useState<number | null>(null);
  const [mostrarFormulario, setMostrarFormulario] = useState(false);
  const [form, setForm] = useState<FormState>(FORM_VACIO);
  const [guardando, setGuardando] = useState(false);

  const cargar = () => {
    setIsLoading(true);
    setError(false);
    adminService
      .obtenerCategorias()
      .then(({ data }) => setCategorias(data.data ?? []))
      .catch(() => setError(true))
      .finally(() => setIsLoading(false));
  };

  useEffect(cargar, []);

  const abrirCrear = () => {
    setEditandoId(null);
    setForm(FORM_VACIO);
    setMostrarFormulario(true);
  };

  const abrirEditar = (categoria: CategoriaAdminDto) => {
    setEditandoId(categoria.id);
    setForm({
      nombre: categoria.nombre,
      descripcion: categoria.descripcion ?? '',
      icono: categoria.icono ?? '',
    });
    setMostrarFormulario(true);
  };

  const cancelar = () => {
    setMostrarFormulario(false);
    setForm(FORM_VACIO);
    setEditandoId(null);
  };

  const guardar = async (evento: React.FormEvent) => {
    evento.preventDefault();
    setGuardando(true);
    const datos = {
      nombre: form.nombre.trim(),
      descripcion: form.descripcion.trim() || undefined,
      icono: form.icono.trim() || undefined,
    };
    try {
      if (editandoId != null) {
        await adminService.actualizarCategoria(editandoId, datos);
        mostrar('Categoria actualizada.', 'success');
      } else {
        await adminService.crearCategoria(datos);
        mostrar('Categoria creada.', 'success');
      }
      cancelar();
      cargar();
    } catch (error_) {
      mostrar(obtenerMensajeError(error_, 'No pudimos guardar la categoria.'), 'error');
    } finally {
      setGuardando(false);
    }
  };

  const alternarActivo = async (categoria: CategoriaAdminDto) => {
    setProcesandoId(categoria.id);
    try {
      if (categoria.isActive) {
        await adminService.desactivarCategoria(categoria.id);
        mostrar('Categoria desactivada.', 'success');
      } else {
        await adminService.activarCategoria(categoria.id);
        mostrar('Categoria activada.', 'success');
      }
      cargar();
    } catch (error_) {
      mostrar(obtenerMensajeError(error_, 'No pudimos actualizar la categoria.'), 'error');
    } finally {
      setProcesandoId(null);
    }
  };

  return (
    <div className="flex flex-col gap-lg">
      <div className="flex items-center justify-between">
        <h1 className="text-h2 text-primary">Gestion de categorias</h1>
        {!mostrarFormulario && (
          <Button variant="primario" onClick={abrirCrear}>
            Nueva categoria
          </Button>
        )}
      </div>

      {mostrarFormulario && (
        <form onSubmit={guardar} className="flex flex-col gap-md rounded-lg bg-bg p-lg shadow-card">
          <h2 className="text-h3 text-text">{editandoId != null ? 'Editar categoria' : 'Nueva categoria'}</h2>
          <Input
            label="Nombre"
            value={form.nombre}
            onChange={(evento) => setForm({ ...form, nombre: evento.target.value })}
            required
          />
          <Input
            label="Descripcion"
            value={form.descripcion}
            onChange={(evento) => setForm({ ...form, descripcion: evento.target.value })}
          />
          <Input
            label="Icono"
            value={form.icono}
            onChange={(evento) => setForm({ ...form, icono: evento.target.value })}
            helperText="Emoji o nombre de icono, p. ej. 📱"
          />
          <div className="flex gap-sm">
            <Button type="submit" variant="primario" disabled={guardando}>
              Guardar
            </Button>
            <Button variant="texto" onClick={cancelar} disabled={guardando}>
              Cancelar
            </Button>
          </div>
        </form>
      )}

      {isLoading && <Loading lineas={6} />}

      {!isLoading && error && (
        <EmptyState mensaje="No pudimos cargar las categorias." accion="Reintentar" onAccion={cargar} />
      )}

      {!isLoading && !error && categorias.length === 0 && <EmptyState mensaje="No hay categorias registradas." />}

      {!isLoading && !error && categorias.length > 0 && (
        <div className="overflow-x-auto rounded-lg bg-bg shadow-card">
          <table className="w-full text-left text-body-s">
            <thead>
              <tr className="border-b border-text-secondary/10 bg-bg-alt">
                <th scope="col" className="p-sm text-caption font-semibold uppercase tracking-wide text-text-secondary">
                  Icono
                </th>
                <th scope="col" className="p-sm text-caption font-semibold uppercase tracking-wide text-text-secondary">
                  Nombre
                </th>
                <th scope="col" className="p-sm text-caption font-semibold uppercase tracking-wide text-text-secondary">
                  Descripcion
                </th>
                <th scope="col" className="p-sm text-caption font-semibold uppercase tracking-wide text-text-secondary">
                  Estado
                </th>
                <th scope="col" className="p-sm text-caption font-semibold uppercase tracking-wide text-text-secondary">
                  Acciones
                </th>
              </tr>
            </thead>
            <tbody>
              {categorias.map((categoria) => (
                <tr
                  key={categoria.id}
                  className="border-b border-text-secondary/10 transition-colors duration-150 hover:bg-bg-alt/60"
                >
                  <td className="p-sm text-text">{categoria.icono}</td>
                  <td className="p-sm text-text">{categoria.nombre}</td>
                  <td className="p-sm text-text">{categoria.descripcion}</td>
                  <td className="p-sm text-text">{categoria.isActive ? 'Activa' : 'Inactiva'}</td>
                  <td className="p-sm">
                    <div className="flex gap-sm">
                      <Button variant="texto" onClick={() => abrirEditar(categoria)}>
                        Editar
                      </Button>
                      <Button
                        variant={categoria.isActive ? 'peligro' : 'secundario'}
                        disabled={procesandoId === categoria.id}
                        onClick={() => alternarActivo(categoria)}
                      >
                        {categoria.isActive ? 'Desactivar' : 'Activar'}
                      </Button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
