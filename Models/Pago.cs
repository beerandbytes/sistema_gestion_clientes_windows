namespace GestorClientes.Models;

public class Pago
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public DateTime FechaPago { get; set; }
    public decimal Cantidad { get; set; }
}

