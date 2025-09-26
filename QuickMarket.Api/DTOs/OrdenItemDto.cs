using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QuickMarket.Api.Dtos
{
    // Item que envía el frontend (sin impuestos)
    // ✅ Record posicional: los atributos de validación van en los PARÁMETROS.
    public sealed record OrdenItemDto(
        [property: JsonPropertyName("id_producto")]
        [Required, Range(1, int.MaxValue, ErrorMessage = "IdProducto debe ser >= 1")]
        int IdProducto,

        [property: JsonPropertyName("cantidad")]
        [Required, Range(typeof(decimal), "0.001", "79228162514264337593543950335",
            ErrorMessage = "Cantidad debe ser > 0")]
        decimal Cantidad,

        // Opcional: si viene null o 0, se toma el precio de PRODUCTOS
        [property: JsonPropertyName("precio_unitario")]
        [Range(typeof(decimal), "0", "79228162514264337593543950335",
            ErrorMessage = "PrecioUnitario no puede ser negativo")]
        decimal? PrecioUnitario
    );

    // Crear orden: el frontend NO manda totales (los hace la BD/servicio)
    public class CrearOrdenDto
    {
        [Required, Range(1, int.MaxValue)]
        public int IdCliente { get; set; }

        [Required, MinLength(1, ErrorMessage = "La orden no tiene items.")]
        public List<OrdenItemDto> Items { get; set; } = new();

        [MaxLength(500)]
        public string? Notas { get; set; }
    }

    // Venta resumida (salida)
    public record VentaResumenDto(
        int IdVenta,
        DateTime Fecha,
        int? IdCliente,
        decimal TotalBruto,
        decimal TotalImpuestos,
        decimal TotalNeto
    );

    // Item de venta para mostrar en factura/resumen (salida)
    public record VentaItemDto(
        int IdDetalle,
        int IdProducto,
        decimal Cantidad,
        decimal PrecioUnitario,
        decimal Importe,           // Cantidad * PrecioUnitario (display)
        string? NombreProducto
    );

    // Venta + detalle (salida)
    public record VentaConDetalleDto(
        VentaResumenDto Venta,
        List<VentaItemDto> Detalle
    );
}
