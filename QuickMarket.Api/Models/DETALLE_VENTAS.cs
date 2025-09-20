// Models/DETALLE_VENTAS.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickMarket.Api.Models
{
    public class DETALLE_VENTAS
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal ID_DETALLE { get; set; }

        public decimal ID_VENTA { get; set; }
        public decimal ID_PRODUCTO { get; set; }

        // Cantidad decimal (permite granel)
        public decimal CANTIDAD { get; set; }

        // Precio sin IVA
        public decimal PRECIO_UNITARIO { get; set; }

        // Calculado por la BD (triggers/paquete). EF lo trata como generado por DB.
        public decimal? SUBTOTAL { get; set; }

        public DateTime CREADO_EN { get; set; }
        public DateTime? ACTUALIZADO_EN { get; set; }
    }
}
