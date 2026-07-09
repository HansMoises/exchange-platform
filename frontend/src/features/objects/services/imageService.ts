import { apiClient } from '../../../services/apiClient';
import type { ApiResponse } from '../../../types/api.types';

// multipart/form-data -> URL publica de la imagen subida (POST /objects/images).
// Se usa desde ImageUploader para publicar/editar objetos (RN-011/012).
export const imageService = {
  subir: (archivo: File) => {
    const formData = new FormData();
    formData.append('archivo', archivo);
    return apiClient.post<ApiResponse<{ url: string }>>('/objects/images', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
};
