namespace QuickMarket.Api.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }                 // ID_USUARIO
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Rol { get; set; } = "cliente";        // 'cliente' | 'administrador'
        public string Estado { get; set; } = "activo";      // activo / inactivo (tu trigger/valor por defecto)

        public string? ResetToken { get; set; }
        public DateTime? ResetExpira { get; set; }

        public DateTime CreadoEn { get; set; }
        public DateTime? ActualizadoEn { get; set; }
    }
}
