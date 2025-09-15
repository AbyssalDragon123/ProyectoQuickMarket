namespace QuickMarket.Api.Models
{
    public class DETALLE_VENTAS
    {
        public decimal ID_DETALLE { get; set; }
        public decimal ID_VENTA { get; set; }
        public decimal ID_PRODUCTO { get; set; }
        public decimal CANTIDAD { get; set; }
        public decimal PRECIO_UNITARIO { get; set; }   // sin IVA
        public decimal? SUBTOTAL { get; set; }         // virtual
        public DateTime CREADO_EN { get; set; }
        public DateTime? ACTUALIZADO_EN { get; set; }
    }
}
