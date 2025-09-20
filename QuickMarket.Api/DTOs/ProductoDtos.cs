// Dtos/ProductoDtos.cs
using System.ComponentModel.DataAnnotations;

namespace QuickMarket.Api.Dtos
{
    public record ProductoDto(
        decimal IdProducto,
        string Nombre,
        string Descripcion,
        decimal PrecioUnitario,
        decimal Stock,
        decimal? IdCategoria,
        decimal? IvaUnitario,
        decimal? PrecioConIva
    );

    public class ProductoCreateDto
    {
        [Required, StringLength(150, MinimumLength = 2)]
        public string Nombre { get; set; } = null!;

        [Required, StringLength(1000, MinimumLength = 2)]
        public string Descripcion { get; set; } = null!;

        [Range(0, double.MaxValue)]
        public decimal PrecioUnitario { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Stock { get; set; }

        public decimal? IdCategoria { get; set; }
    }

    public class ProductoUpdateDto
    {
        [Required, StringLength(150, MinimumLength = 2)]
        public string Nombre { get; set; } = null!;

        [Required, StringLength(1000, MinimumLength = 2)]
        public string Descripcion { get; set; } = null!;

        [Range(0, double.MaxValue)]
        public decimal PrecioUnitario { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Stock { get; set; }

        public decimal? IdCategoria { get; set; }
    }

    // Para operaciones puntuales de stock (sumar/restar)
    public class AjusteStockDto
    {
        // puede ser negativo para restar
        [Range(typeof(decimal), "-999999999", "999999999")]
        public decimal Delta { get; set; }

        [StringLength(250)]
        public string? Motivo { get; set; }
    }
}
