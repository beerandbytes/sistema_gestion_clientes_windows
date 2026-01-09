namespace GestorClientes.Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public int? Edad { get; set; }
    public decimal? Peso { get; set; }
    public string Telefono { get; set; } = string.Empty;
    public DateTime FechaAlta { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public DateTime? FechaUltimoPago { get; set; }
    public bool Activo { get; set; }
    public string Estado { get; set; } = string.Empty;
}

