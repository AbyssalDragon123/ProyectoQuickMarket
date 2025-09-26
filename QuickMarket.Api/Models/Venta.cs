using QuickMarket.Api.Models;

public class Venta
{
    public int IdVenta { get; set; }
    public DateTime Fecha { get; set; }
    public int? IdCliente { get; set; }

    public decimal TotalBruto { get; set; }
    public decimal TotalImpuestos { get; set; }
    public decimal TotalNeto { get; set; }

    public DateTime CreadoEn { get; set; }
    public DateTime? ActualizadoEn { get; set; }

    // Navegación
    public Cliente? Cliente { get; set; }
    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}