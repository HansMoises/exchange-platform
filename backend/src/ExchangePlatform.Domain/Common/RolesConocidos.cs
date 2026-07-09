namespace ExchangePlatform.Domain.Common;

// Roles fijos sembrados en RolConfiguration.cs (Infrastructure/Persistence/Configurations).
// No hay endpoint GET /roles ni acceso a la tabla Roles desde IUnitOfWork,
// asi que este mapeo evita repetir el error de exponer RolId.ToString()
// (que devuelve "1"/"2"/"3" en vez del nombre) en cada DTO que expone el rol.
public static class RolesConocidos
{
    public const int Administrador = 1;
    public const int Moderador = 2;
    public const int Usuario = 3;

    public static string ObtenerNombre(int rolId) => rolId switch
    {
        Administrador => "Administrador",
        Moderador => "Moderador",
        Usuario => "Usuario",
        _ => "Usuario",
    };
}
