namespace QuickMarket.Api.Dtos
{
    // Lectura
    public record ProductoDto(
        int IdProducto,
        string Nombre,
        string Descripcion,
        decimal PrecioUnitario,
        decimal Stock,
        int? IdCategoria,
        decimal IvaUnitario,     // generado por DB
        decimal PrecioConIva,    // generado por DB
        DateTime CreadoEn,
        DateTime? ActualizadoEn
    );

    // Creación
    public record CreateProductoDto(
        string Nombre,
        string Descripcion,
        decimal PrecioUnitario,
        decimal Stock,
        int? IdCategoria
    );

    // Actualización
    public record UpdateProductoDto(
        string? Nombre,
        string? Descripcion,
        decimal? PrecioUnitario,
        decimal? Stock,
        int? IdCategoria
    );
}
