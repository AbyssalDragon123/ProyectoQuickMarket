public class DetalleVenta
{
    public int IdDetalle { get; set; }
    public int IdVenta { get; set; }
    public int IdProducto { get; set; }

    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }

    // Virtual/generated (solo lectura desde EF)
    public decimal Subtotal { get; private set; }

    public DateTime CreadoEn { get; set; }
    public DateTime? ActualizadoEn { get; set; }

    // Navegación
    public Venta Venta { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
}