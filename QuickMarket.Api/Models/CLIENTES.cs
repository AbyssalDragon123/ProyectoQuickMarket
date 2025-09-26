using QuickMarket.Api.Models;

public class Cliente
{
    public int IdCliente { get; set; }
    public int? IdUsuario { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Departamento { get; set; }
    public string? Municipio { get; set; }
    public string? Referencia { get; set; }

    public DateTime CreadoEn { get; set; }
    public DateTime? ActualizadoEn { get; set; }

    // Navegación
    public Usuario? Usuario { get; set; }
}