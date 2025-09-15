using System.ComponentModel.DataAnnotations.Schema;

namespace QuickMarket.Api.Models
{
    public class PRODUCTOS
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal ID_PRODUCTO { get; set; }
        public string NOMBRE { get; set; } = null!;
        public string DESCRIPCION { get; set; } = null!;
        public decimal PRECIO_UNITARIO { get; set; }   // sin IVA
        public decimal STOCK { get; set; }
        public decimal? ID_CATEGORIA { get; set; }
        public decimal? IVA_UNITARIO { get; set; }     // virtual
        public decimal? PRECIO_CON_IVA { get; set; }   // virtual
        public DateTime CREADO_EN { get; set; }
        public DateTime? ACTUALIZADO_EN { get; set; }
    }
}
