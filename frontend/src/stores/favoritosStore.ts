import { create } from 'zustand';
import { favoritoService } from '../features/favorites/services/favoritoService';

interface FavoritosState {
  ids: Set<string>;
  cargado: boolean;
  cargar: () => Promise<void>;
  alternar: (objetoId: string) => Promise<void>;
}

// Store global (no de una sola feature) porque FavoriteButton se usa desde
// ObjectCard/ObjectDetail (feature objects) ademas de la pagina Favoritos.
export const useFavoritosStore = create<FavoritosState>((set, get) => ({
  ids: new Set(),
  cargado: false,

  cargar: async () => {
    try {
      const { data } = await favoritoService.listar();
      const ids = new Set((data.data ?? []).map((objeto) => objeto.id));
      set({ ids, cargado: true });
    } catch {
      set({ cargado: true });
    }
  },

  alternar: async (objetoId) => {
    const { ids } = get();
    const esFavorito = ids.has(objetoId);
    const nuevasIds = new Set(ids);
    if (esFavorito) {
      nuevasIds.delete(objetoId);
    } else {
      nuevasIds.add(objetoId);
    }
    set({ ids: nuevasIds });

    try {
      if (esFavorito) {
        await favoritoService.quitar(objetoId);
      } else {
        await favoritoService.agregar(objetoId);
      }
    } catch {
      set({ ids });
    }
  },
}));
