namespace QuickMarket.Api.Models
{
    public class LOGIN
    {
        public decimal ID_LOGIN { get; set; }
        public decimal ID_EMPLEADO { get; set; }

        public string USERNAME { get; set; } = null!;
        public string GMAIL { get; set; } = null!;          // <-- correo en LOGIN
        public string PASSWORD_HASH { get; set; } = null!;

        public string? RESET_TOKEN { get; set; }            // 5 dígitos
        public DateTime? RESET_TOKEN_EXP { get; set; }

        public string ESTADO { get; set; } = "activo";
        public DateTime CREADO_EN { get; set; }
        public DateTime? ACTUALIZADO_EN { get; set; }
    }
}
