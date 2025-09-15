namespace QuickMarket.Api.Models
{
    public class VENTAS
    {
        public decimal ID_VENTA { get; set; }
        public DateTime FECHA { get; set; }
        public decimal? ID_CLIENTE { get; set; }
        public decimal TOTAL_BRUTO { get; set; }
        public decimal TOTAL_IMPUESTOS { get; set; }
        public decimal TOTAL_NETO { get; set; }
        public decimal? ID_EMPLEADO { get; set; }
        public DateTime CREADO_EN { get; set; }
        public DateTime? ACTUALIZADO_EN { get; set; }
    }
}
