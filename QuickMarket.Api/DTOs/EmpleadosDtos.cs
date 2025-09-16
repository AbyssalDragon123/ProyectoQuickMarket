using System.ComponentModel.DataAnnotations;

namespace QuickMarket.Api.Dtos
{
    public record EmpleadoDto(
        decimal Id,
        string Nombre,
        string Carnet,
        DateTime CreadoEn,
        DateTime? ActualizadoEn
    );

    public class EmpleadoCreateDto
    {
        [Required, StringLength(80, MinimumLength = 2)]
        public string Nombre { get; set; } = null!;

        [Required, StringLength(30, MinimumLength = 3)]
        public string Carnet { get; set; } = null!;
    }

    public class EmpleadoUpdateDto
    {
        [Required, StringLength(80, MinimumLength = 2)]
        public string Nombre { get; set; } = null!;

        [Required, StringLength(30, MinimumLength = 3)]
        public string Carnet { get; set; } = null!;
    }
}
