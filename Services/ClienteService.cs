using GestorClientes.Models;
using System.Text.RegularExpressions;

namespace GestorClientes.Services;

public class ClienteService
{
    public static string GetEstadoVisual(Cliente cliente)
    {
        // Si el cliente tiene un estado manual, usarlo
        if (!string.IsNullOrWhiteSpace(cliente.Estado))
        {
            return cliente.Estado switch
            {
                "Activo" => "🟢 Activo",
                "Pendiente" => "🟡 Pendiente de pago",
                "Vencido" => "🔴 Vencido",
                _ => cliente.Estado
            };
        }
        
        // Fallback: calcular automáticamente basado en fecha
        var hoy = DateTime.Today;
        return cliente.FechaVencimiento >= hoy ? "🟢 Activo" : "🔴 Vencido";
    }

    public static bool EsActivo(Cliente cliente)
    {
        var hoy = DateTime.Today;
        return cliente.FechaVencimiento >= hoy;
    }

    public static int GetDiasRestantes(Cliente cliente)
    {
        var hoy = DateTime.Today;
        var dias = (cliente.FechaVencimiento - hoy).Days;
        return dias;
    }

    public static string GetDiasRestantesVisual(Cliente cliente)
    {
        var dias = GetDiasRestantes(cliente);
        if (dias < 0)
            return "Vencido";
        if (dias == 0)
            return "Vence hoy";
        if (dias == 1)
            return "1 día";
        return $"{dias} días";
    }

    public static (bool esValido, string mensaje) ValidarTelefono(string telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono))
            return (true, string.Empty); // Teléfono es opcional

        // Remover espacios, guiones, paréntesis para validar
        var telefonoLimpio = Regex.Replace(telefono, @"[\s\-\(\)]", "");
        
        // Validar que solo contenga números después de limpiar
        if (!Regex.IsMatch(telefonoLimpio, @"^\d+$"))
            return (false, "El teléfono solo puede contener números, espacios, guiones y paréntesis.");

        // Validar longitud (mínimo 7, máximo 15 dígitos)
        if (telefonoLimpio.Length < 7)
            return (false, "El teléfono debe tener al menos 7 dígitos.");
        
        if (telefonoLimpio.Length > 15)
            return (false, "El teléfono no puede tener más de 15 dígitos.");

        return (true, string.Empty);
    }

    public static (bool esValido, string mensaje) ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return (false, "El nombre es requerido.");

        var nombreTrimmed = nombre.Trim();
        
        if (nombreTrimmed.Length < 2)
            return (false, "El nombre debe tener al menos 2 caracteres.");
        
        if (nombreTrimmed.Length > 100)
            return (false, "El nombre no puede tener más de 100 caracteres.");

        return (true, string.Empty);
    }
}

