// Models/CLIENTES.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickMarket.Api.Models
{
    public class CLIENTES
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal ID_CLIENTE { get; set; }

        // Vínculo 1:1 con USUARIOS (el dueño de este perfil de cliente)
        public decimal? ID_USUARIO { get; set; }

        public string NOMBRE { get; set; } = null!;
        public string? EMAIL { get; set; }
        public string? TELEFONO { get; set; }
        public string? DIRECCION { get; set; }
        public string? DEPARTAMENTO { get; set; }
        public string? MUNICIPIO { get; set; }
        public string? REFERENCIA { get; set; }

        public DateTime CREADO_EN { get; set; }
        public DateTime? ACTUALIZADO_EN { get; set; }
    }
}
