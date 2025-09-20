// Models/USUARIOS.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickMarket.Api.Models
{
    public class USUARIOS
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal ID_USUARIO { get; set; }

        public string USERNAME { get; set; } = null!;
        public string EMAIL { get; set; } = null!;
        public string PASSWORD_HASH { get; set; } = null!;

        public string ROL { get; set; } = "cliente"; // cliente / administrador
        public string ESTADO { get; set; } = "activo";

        public string? RESET_TOKEN { get; set; }
        public DateTime? RESET_EXPIRA { get; set; }

        public DateTime CREADO_EN { get; set; }
        public DateTime? ACTUALIZADO_EN { get; set; }
    }
}
