using System.ComponentModel.DataAnnotations;

namespace QuickMarket.Api.Dtos
{
    public record ClienteDto(
        decimal Id,
        decimal? IdEmpleado,
        string Nombre,
        string? Telefono
    );

    public class ClienteCreateDto
    {
        public decimal? IdEmpleado { get; set; }

        [Required, StringLength(150, MinimumLength = 2)]
        public string Nombre { get; set; } = null!;

        [RegularExpression(@"^[0-9\s\-\+\(\)]{7,20}$", ErrorMessage = "Teléfono inválido.")]
        public string? Telefono { get; set; }
    }

    public class ClienteUpdateDto
    {
        public decimal? IdEmpleado { get; set; }

        [Required, StringLength(150, MinimumLength = 2)]
        public string Nombre { get; set; } = null!;

        [RegularExpression(@"^[0-9\s\-\+\(\)]{7,20}$", ErrorMessage = "Teléfono inválido.")]
        public string? Telefono { get; set; }
    }
}
