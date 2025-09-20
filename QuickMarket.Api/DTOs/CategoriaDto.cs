// Dtos/CategoriaDtos.cs
using System.ComponentModel.DataAnnotations;

namespace QuickMarket.Api.Dtos
{
    public record CategoriaDto(
        decimal IdCategoria,
        string Nombre,
        string? Descripcion,
        DateTime CreadoEn,
        DateTime? ActualizadoEn
    );

    public class CategoriaCreateDto
    {
        [Required, StringLength(120, MinimumLength = 2)]
        public string Nombre { get; set; } = null!;

        [StringLength(500)]
        public string? Descripcion { get; set; }
    }

    public class CategoriaUpdateDto
    {
        [Required, StringLength(120, MinimumLength = 2)]
        public string Nombre { get; set; } = null!;

        [StringLength(500)]
        public string? Descripcion { get; set; }
    }
}
