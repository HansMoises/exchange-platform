// Refleja PerfilUsuarioDto real del backend (Users/DTOs/PerfilUsuarioDto.cs).
// Nota: aqui el rol llega como "rolId" (numero), no como nombre de texto
// (a diferencia de LoginResponseDto.usuario.rol, que si es un string).
export interface PerfilUsuarioDto {
  id: string;
  nombres: string;
  apellidos: string;
  email: string;
  telefono: string;
  fotoPerfil: string | null;
  rolId: number;
  departamentoId: number;
  provinciaId: number;
  distritoId: number;
  calificacionPromedio: number;
  totalIntercambios: number;
  miembroDesde: string;
}

export interface ActualizarPerfilDto {
  nombres: string;
  apellidos: string;
  telefono: string;
  departamentoId: number;
  provinciaId: number;
  distritoId: number;
}

export interface CambiarPasswordDto {
  passwordActual: string;
  passwordNuevo: string;
  confirmPassword: string;
}
