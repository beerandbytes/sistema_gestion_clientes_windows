namespace GestorClientes.Models;

public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string ContraseñaHash { get; set; } = string.Empty;
}

