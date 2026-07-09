export interface RegistrarUsuarioDto {
  nombres: string;
  apellidos: string;
  email: string;
  password: string;
  confirmPassword: string;
  telefono: string;
  departamentoId: number;
  provinciaId: number;
  distritoId: number;
}

export interface IniciarSesionDto {
  email: string;
  password: string;
}

export interface OlvidarPasswordDto {
  email: string;
}

export interface RestablecerPasswordDto {
  token: string;
  password: string;
  confirmPassword: string;
}
