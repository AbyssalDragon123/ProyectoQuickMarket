// Dtos/ClienteDtos.cs
using System.ComponentModel.DataAnnotations;

namespace QuickMarket.Api.Dtos
{
    // Para devolver el perfil completo del cliente del usuario
    public record ClienteDto(
        decimal IdCliente,
        decimal? IdUsuario,
        string Nombre,
        string? Email,
        string? Telefono,
        string? Direccion,
        string? Departamento,
        string? Municipio,
        string? Referencia
    );

    // Para crear/actualizar (upsert) el perfil del usuario autenticado
    public class ClienteUpsertDto
    {
        [Required, StringLength(150, MinimumLength = 2)]
        public string Nombre { get; set; } = null!;

        [EmailAddress, MaxLength(150)]
        public string? Email { get; set; }

        [RegularExpression(@"^[0-9\s\-\+\(\)]{7,20}$", ErrorMessage = "Teléfono inválido.")]
        public string? Telefono { get; set; }

        [MaxLength(200)] public string? Direccion { get; set; }
        [MaxLength(80)] public string? Departamento { get; set; }
        [MaxLength(80)] public string? Municipio { get; set; }
        [MaxLength(200)] public string? Referencia { get; set; }
    }
}
