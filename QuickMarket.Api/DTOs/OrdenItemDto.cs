// Dtos/VentasDtos.cs
using System.ComponentModel.DataAnnotations;

namespace QuickMarket.Api.Dtos
{
    // Item que envía el frontend (sin impuestos)
    public record OrdenItemDto(
        [property: Range(1, double.MaxValue)] decimal IdProducto,
        [property: Range(typeof(decimal), "0.001", "999999999", ErrorMessage = "Cantidad debe ser > 0")]
        decimal Cantidad,
        [property: Range(0.0, double.MaxValue)] decimal PrecioUnitario
    );

    // Crear orden: el frontend NO manda totales (los hace la BD)
    public class CrearOrdenDto
    {
        [MinLength(1, ErrorMessage = "La orden no tiene items.")]
        public List<OrdenItemDto> Items { get; set; } = new();

        [MaxLength(500)] public string? Notas { get; set; }
    }

    // Venta resumida
    public record VentaResumenDto(
        decimal IdVenta,
        DateTime Fecha,
        decimal? IdCliente,
        decimal TotalBruto,
        decimal TotalImpuestos,
        decimal TotalNeto
    );

    // Item de venta para mostrar en factura/resumen
    public record VentaItemDto(
        decimal IdDetalle,
        decimal IdProducto,
        decimal Cantidad,
        decimal PrecioUnitario,
        decimal Importe // Cantidad * PrecioUnitario (display)
    );

    // Venta + detalle
    public record VentaConDetalleDto(
        VentaResumenDto Venta,
        List<VentaItemDto> Detalle
    );
}
