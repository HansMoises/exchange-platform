import { useEffect, useState } from 'react';
import { geoService } from '../services/geoService';
import type { CategoriaDto, DepartamentoDto, DistritoDto, ProvinciaDto } from '../types/geo.types';

// Hooks independientes y componibles para los selects en cascada
// departamento -> provincia -> distrito (UI.md SS7.4). Se combinan con el
// valor seleccionado del formulario (react-hook-form watch), sin duplicar
// el estado de seleccion.

export function useDepartamentos() {
  const [departamentos, setDepartamentos] = useState<DepartamentoDto[]>([]);

  useEffect(() => {
    geoService
      .obtenerDepartamentos()
      .then(({ data }) => setDepartamentos(data.data ?? []))
      .catch(() => setDepartamentos([]));
  }, []);

  return departamentos;
}

export function useProvincias(departamentoId: number | undefined) {
  const [provincias, setProvincias] = useState<ProvinciaDto[]>([]);

  useEffect(() => {
    if (!departamentoId) {
      setProvincias([]);
      return;
    }
    geoService
      .obtenerProvincias(departamentoId)
      .then(({ data }) => setProvincias(data.data ?? []))
      .catch(() => setProvincias([]));
  }, [departamentoId]);

  return provincias;
}

export function useDistritos(provinciaId: number | undefined) {
  const [distritos, setDistritos] = useState<DistritoDto[]>([]);

  useEffect(() => {
    if (!provinciaId) {
      setDistritos([]);
      return;
    }
    geoService
      .obtenerDistritos(provinciaId)
      .then(({ data }) => setDistritos(data.data ?? []))
      .catch(() => setDistritos([]));
  }, [provinciaId]);

  return distritos;
}

export function useCategorias() {
  const [categorias, setCategorias] = useState<CategoriaDto[]>([]);

  useEffect(() => {
    geoService
      .obtenerCategorias()
      .then(({ data }) => setCategorias(data.data ?? []))
      .catch(() => setCategorias([]));
  }, []);

  return categorias;
}
