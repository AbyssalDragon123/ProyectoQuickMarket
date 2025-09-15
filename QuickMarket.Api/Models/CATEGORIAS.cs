namespace QuickMarket.Api.Models
{
    public class CATEGORIAS
    {
        public decimal ID_CATEGORIA { get; set; }
        public string NOMBRE { get; set; } = null!;
        public string? DESCRIPCION { get; set; }
        public DateTime CREADO_EN { get; set; }
        public DateTime? ACTUALIZADO_EN { get; set; }
    }
}
