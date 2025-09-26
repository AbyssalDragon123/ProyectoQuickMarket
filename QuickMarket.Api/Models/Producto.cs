public class Producto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = null!;
    public string Descripcion { get; set; } = null!;

    public decimal PrecioUnitario { get; set; }
    public decimal Stock { get; set; }

    public int? IdCategoria { get; set; }

    // Virtual/generated columns (solo lectura desde EF)
    public decimal IvaUnitario { get; private set; }
    public decimal PrecioConIva { get; private set; }

    public DateTime CreadoEn { get; set; }
    public DateTime? ActualizadoEn { get; set; }

    // Navegación
    public Categoria? Categoria { get; set; }
}