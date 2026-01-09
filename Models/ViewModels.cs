using System.ComponentModel;
using System.Runtime.CompilerServices;
using GestorClientes.Models;

namespace GestorClientes.Models;

/// <summary>
/// Modelos de vista para mostrar datos en las interfaces de usuario
/// </summary>
public class ClienteDisplayItem : INotifyPropertyChanged
{
    private bool _isSelected;

    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Edad { get; set; } = string.Empty;
    public decimal? Peso { get; set; }
    public string Telefono { get; set; } = string.Empty;
    public string FechaAlta { get; set; } = string.Empty;
    public string FechaUltimoPago { get; set; } = string.Empty;
    public string FechaVencimiento { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string DiasRestantes { get; set; } = string.Empty;
    public string DiasRestantesColor { get; set; } = "Black";
    public Cliente? Cliente { get; set; }
    
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class PagoDisplayItem
{
    public int Id { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string FechaPago { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
}

public class RecordatorioItem
{
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string FechaVencimiento { get; set; } = string.Empty;
    public string DiasVencido { get; set; } = string.Empty;
    public string DiasRestantes { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}

public class PagoHistorialItem
{
    public int Id { get; set; }
    public string FechaPago { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
}

public class ClienteComboItem
{
    public Cliente? Cliente { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    
    public override string ToString()
    {
        if (Cliente == null)
            return NombreCompleto;
        
        // Si hay teléfono, incluirlo para diferenciar clientes con el mismo nombre
        if (!string.IsNullOrWhiteSpace(Cliente.Telefono))
        {
            return $"{NombreCompleto} - {Cliente.Telefono}";
        }
        
        // Si no hay teléfono, incluir el ID
        return $"{NombreCompleto} (ID: {Cliente.Id})";
    }
}



