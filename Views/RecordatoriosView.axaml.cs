using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Huskui.Avalonia.Controls;
using GestorClientes.Models;
using GestorClientes.Services;

namespace GestorClientes.Views;

public partial class RecordatoriosView : AppWindow
{
    private readonly RecordatorioService _recordatorioService;
    private ObservableCollection<RecordatorioItem> _vencidosDisplay = new();
    private ObservableCollection<RecordatorioItem> _proximosDisplay = new();

    // Los controles con x:Name se generan automáticamente por Avalonia

    public RecordatoriosView()
    {
        _recordatorioService = new RecordatorioService();
        
        try
        {
            InitializeComponent();
            
            if (DataGridVencidos != null)
            {
                DataGridVencidos.ItemsSource = _vencidosDisplay;
            }
            
            if (DataGridProximos != null)
            {
                DataGridProximos.ItemsSource = _proximosDisplay;
            }
            
            CargarRecordatorios();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error al inicializar RecordatoriosView: {ex.Message}");
        }
    }

    private void CargarRecordatorios()
    {
        var clientesVencidos = _recordatorioService.GetClientesVencidos();
        var clientesProximos = _recordatorioService.GetClientesProximosAVencer(7);

        _vencidosDisplay.Clear();
        foreach (var cliente in clientesVencidos)
        {
            _vencidosDisplay.Add(new RecordatorioItem
            {
                Nombre = $"{cliente.Nombre} {cliente.Apellidos}".Trim(),
                Telefono = cliente.Telefono ?? "N/A",
                FechaVencimiento = cliente.FechaVencimiento.ToString("yyyy-MM-dd"),
                DiasVencido = Math.Abs(_recordatorioService.GetDiasHastaVencimiento(cliente)).ToString(),
                DiasRestantes = _recordatorioService.GetDiasHastaVencimiento(cliente).ToString(),
                Estado = "Vencido"
            });
        }

        _proximosDisplay.Clear();
        foreach (var cliente in clientesProximos)
        {
            _proximosDisplay.Add(new RecordatorioItem
            {
                Nombre = $"{cliente.Nombre} {cliente.Apellidos}".Trim(),
                Telefono = cliente.Telefono ?? "N/A",
                FechaVencimiento = cliente.FechaVencimiento.ToString("yyyy-MM-dd"),
                DiasRestantes = _recordatorioService.GetDiasHastaVencimiento(cliente).ToString(),
                Estado = "Próximo a Vencer"
            });
        }

        if (LblVencidos != null)
        {
            LblVencidos.Text = $"Clientes Vencidos: {clientesVencidos.Count}";
        }
        
        if (LblProximos != null)
        {
            LblProximos.Text = $"Clientes Próximos a Vencer (7 días): {clientesProximos.Count}";
        }
    }

    private void OnActualizarClick(object? sender, RoutedEventArgs e)
    {
        CargarRecordatorios();
    }
}

