namespace QuickMarket.Api.Models
{
    public class EMPLEADOS
    {
        public decimal ID_EMPLEADO { get; set; }
        public string NOMBRE { get; set; } = null!;
        public string CARNET { get; set; } = null!;
        public DateTime CREADO_EN { get; set; }
        public DateTime? ACTUALIZADO_EN { get; set; }
    }
}
