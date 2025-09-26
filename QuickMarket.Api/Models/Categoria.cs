using QuickMarket.Api.Models;

public class Categoria
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }

    public DateTime CreadoEn { get; set; }
    public DateTime? ActualizadoEn { get; set; }

    // Navegación
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}