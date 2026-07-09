export type RolUsuario = 'Administrador' | 'Moderador' | 'Usuario';

export interface UsuarioDto {
  id: string;
  nombres: string;
  apellidos: string;
  email: string;
  rol: RolUsuario;
  fotoPerfil: string | null;
  calificacionPromedio: number;
  totalIntercambios: number;
}

// Nota: el backend real (LoginResponseDto.cs) serializa la propiedad "Usuario"
// como "usuario", a diferencia del ejemplo "user" documentado en API.md SS4.
export interface LoginResponseDto {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  usuario: UsuarioDto;
}

export interface RefreshResponseDto {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
}
