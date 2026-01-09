using Avalonia.Controls;
using Avalonia.Interactivity;
using Huskui.Avalonia.Controls;
using GestorClientes.Services;

namespace GestorClientes.Views;

public partial class ResumenView : AppWindow
{
    private readonly MetricasService _metricasService;

    // Los controles con x:Name se generan automáticamente por Avalonia

    public ResumenView()
    {
        _metricasService = new MetricasService();
        
        try
        {
            InitializeComponent();
            CargarMetricas();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error al inicializar ResumenView: {ex.Message}");
        }
    }

    private void CargarMetricas()
    {
        var totalClientes = _metricasService.GetTotalClientes();
        var clientesActivos = _metricasService.GetClientesActivos();
        var clientesVencidos = _metricasService.GetClientesVencidos();
        var ingresosMes = _metricasService.GetIngresosDelMes();

        if (LblTotalClientes != null)
        {
            LblTotalClientes.Text = totalClientes.ToString();
        }

        if (LblClientesActivos != null)
        {
            LblClientesActivos.Text = clientesActivos.ToString();
        }

        if (LblClientesVencidos != null)
        {
            LblClientesVencidos.Text = clientesVencidos.ToString();
        }

        if (LblIngresosMes != null)
        {
            LblIngresosMes.Text = $"€{ingresosMes:N2}";
        }
    }

    private void OnActualizarClick(object? sender, RoutedEventArgs e)
    {
        CargarMetricas();
    }
}

