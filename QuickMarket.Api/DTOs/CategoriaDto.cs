namespace QuickMarket.Api.Dtos
{
    // Lectura
    public record CategoriaDto(
        int IdCategoria,
        string Nombre,
        string? Descripcion,
        DateTime CreadoEn,
        DateTime? ActualizadoEn
    );

    // Creación
    public record CreateCategoriaDto(
        string Nombre,
        string? Descripcion
    );

    // Actualización
    public record UpdateCategoriaDto(
        string? Nombre,
        string? Descripcion
    );
}
