namespace QuickMarket.Api.Dtos
{
    // Lectura
    public record ClienteDto(
        int IdCliente,
        int? IdUsuario,
        string Nombre,
        string? Telefono,
        string? Direccion,
        string? Departamento,
        string? Municipio,
        string? Referencia,
        DateTime CreadoEn,
        DateTime? ActualizadoEn
    );

    // Creación
    public record CreateClienteDto(
        int? IdUsuario,
        string Nombre,
        string? Telefono,
        string? Direccion,
        string? Departamento,
        string? Municipio,
        string? Referencia
    );

    // Actualización
    public record UpdateClienteDto(
        int? IdUsuario,
        string? Nombre,
        string? Telefono,
        string? Direccion,
        string? Departamento,
        string? Municipio,
        string? Referencia
    );
}
