using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Huskui.Avalonia.Controls;
using GestorClientes.Data;
using GestorClientes.Helpers;
using GestorClientes.Services;
using GestorClientes.Views;

namespace GestorClientes.Views;

public partial class MainWindow : AppWindow
{
    private readonly RecordatorioService _recordatorioService;
    private readonly MetricasService _metricasService;

    // Los controles con x:Name se generan automáticamente por Avalonia

    public MainWindow()
    {
        _recordatorioService = new RecordatorioService();
        _metricasService = new MetricasService();
        
        try
        {
            InitializeComponent();
            CargarIcono();
            CargarDashboard();
            VerificarRecordatorios();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error al inicializar MainWindow: {ex.Message}");
        }
    }

    private void CargarIcono()
    {
        try
        {
            var posiblesRutas = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "Resources", "logo.ico"),
                Path.Combine(AppContext.BaseDirectory, "logo.ico"),
                Path.Combine(Environment.CurrentDirectory, "Resources", "logo.ico"),
                Path.Combine(Environment.CurrentDirectory, "logo.ico")
            };

            foreach (var iconPath in posiblesRutas)
            {
                if (File.Exists(iconPath))
                {
                    try
                    {
                        this.Icon = new WindowIcon(iconPath);
                        Console.WriteLine($"Icono cargado desde: {iconPath}");
                        return;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al cargar icono desde {iconPath}: {ex.Message}");
                    }
                }
            }
            Console.WriteLine("No se encontró el archivo logo.ico en ninguna ubicación");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en CargarIcono: {ex.Message}");
        }
    }

    private void OnActivated(object? sender, EventArgs e)
    {
        CargarDashboard();
        VerificarRecordatorios();
    }

    private void CargarDashboard()
    {
        var totalClientes = _metricasService.GetTotalClientes();
        var clientesActivos = _metricasService.GetClientesActivos();
        var clientesVencidos = _metricasService.GetClientesVencidos();
        var ingresosMes = _metricasService.GetIngresosDelMes();
        var clientesProximos = _recordatorioService.GetClientesProximosAVencer(7).Count;

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

        if (LblClientesProximos != null)
        {
            LblClientesProximos.Text = clientesProximos.ToString();
        }
    }

    private void VerificarRecordatorios()
    {
        var clientesVencidos = _recordatorioService.GetClientesVencidos();
        var clientesProximos = _recordatorioService.GetClientesProximosAVencer(7);

        if (RecordatoriosInfoBar == null)
        {
            return;
        }

        if (clientesVencidos.Count > 0 || clientesProximos.Count > 0)
        {
            var mensaje = string.Empty;
            if (clientesVencidos.Count > 0)
            {
                mensaje += $"⚠️ Hay {clientesVencidos.Count} cliente(s) con membresía vencida. ";
            }
            if (clientesProximos.Count > 0)
            {
                mensaje += $"Hay {clientesProximos.Count} cliente(s) próximos a vencer (7 días).";
            }
            mensaje += " Revise la sección de Recordatorios para más detalles.";

            RecordatoriosInfoBar.Content = mensaje;
            RecordatoriosInfoBar.IsVisible = true;
        }
        else
        {
            RecordatoriosInfoBar.IsVisible = false;
        }
    }

    private void OnMetricaClick(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control control && control.Tag is string tipo)
        {
            MostrarDetallesMetrica(tipo);
        }
    }

    private async void MostrarDetallesMetrica(string tipo)
    {
        try
        {
            switch (tipo)
            {
                case "Todos":
                    var clientesFormTodos = new ClientesView();
                    await clientesFormTodos.ShowDialog(this);
                    CargarDashboard();
                    VerificarRecordatorios();
                    break;

                case "Activos":
                    var clientesFormActivos = new ClientesView();
                    clientesFormActivos.SetFiltro("Activos");
                    await clientesFormActivos.ShowDialog(this);
                    CargarDashboard();
                    VerificarRecordatorios();
                    break;

                case "Vencidos":
                    var clientesFormVencidos = new ClientesView();
                    clientesFormVencidos.SetFiltro("Vencidos");
                    await clientesFormVencidos.ShowDialog(this);
                    CargarDashboard();
                    VerificarRecordatorios();
                    break;

                case "Proximos":
                    MostrarClientesProximos();
                    break;

                case "Ingresos":
                    MostrarDetallesIngresos();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error al mostrar detalles de métrica '{tipo}': {ex.Message}");
            await ShowMessageAsync($"Error al mostrar detalles: {ex.Message}", "Error");
        }
    }

    private void MostrarClientesProximos()
    {
        var clientesProximos = _recordatorioService.GetClientesProximosAVencer(7);

        if (clientesProximos.Count == 0)
        {
            ShowMessage("No hay clientes próximos a vencer en los próximos 7 días.", "Clientes Próximos a Vencer");
            return;
        }

        var mensaje = "Clientes próximos a vencer (7 días):\n\n";
        foreach (var cliente in clientesProximos.OrderBy(c => c.FechaVencimiento))
        {
            var diasRestantes = (cliente.FechaVencimiento.Date - DateTime.Today).Days;
            var nombreCompleto = $"{cliente.Nombre} {cliente.Apellidos}".Trim();
            mensaje += $"{nombreCompleto} - Vence: {cliente.FechaVencimiento:dd/MM/yyyy} ({diasRestantes} días)\n";
        }

        ShowMessage(mensaje, "Clientes Próximos a Vencer");
    }

    private void MostrarDetallesIngresos()
    {
        var hoy = DateTime.Today;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
        var finMes = inicioMes.AddMonths(1).AddDays(-1);

        var pagoRepository = new PagoRepository();
        var clienteRepository = new ClienteRepository();
        var pagos = pagoRepository.GetByFechaRange(inicioMes, finMes);

        if (pagos.Count == 0)
        {
            ShowMessage($"No hay pagos registrados para el mes actual ({hoy:MMMM yyyy}).", "Ingresos del Mes");
            return;
        }

        var total = pagos.Sum(p => p.Cantidad);
        var mensaje = $"Ingresos del Mes: {hoy:MMMM yyyy}\n\n";
        mensaje += $"Total de pagos: {pagos.Count}\n";
        mensaje += $"Total ingresado: €{total:N2}\n\n";
        mensaje += "Últimos 10 pagos:\n";
        mensaje += "─────────────────────────────\n";

        var ultimosPagos = pagos.OrderByDescending(p => p.FechaPago).Take(10);
        foreach (var pago in ultimosPagos)
        {
            var cliente = clienteRepository.GetById(pago.ClienteId);
            var nombreCliente = cliente != null ? $"{cliente.Nombre} {cliente.Apellidos}".Trim() : "N/A";
            mensaje += $"{pago.FechaPago:dd/MM/yyyy} - {nombreCliente}: €{pago.Cantidad:N2}\n";
        }

        if (pagos.Count > 10)
        {
            mensaje += $"\n... y {pagos.Count - 10} pago(s) más";
        }

        ShowMessage(mensaje, "Detalle de Ingresos del Mes");
    }

    private Avalonia.Controls.Button? _activeNavItem = null;

    private void OnNavItemEnter(object? sender, PointerEventArgs e)
    {
        if (sender is Avalonia.Controls.Button button && button != _activeNavItem)
        {
            // Aplicar hover state: fondo Radix Blue 3 solo si no está activo
            button.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#B5DAFD"));
        }
    }

    private void OnNavItemLeave(object? sender, PointerEventArgs e)
    {
        if (sender is Avalonia.Controls.Button button && button != _activeNavItem)
        {
            // Restaurar estado normal: transparente solo si no está activo
            button.Background = Avalonia.Media.Brushes.Transparent;
        }
    }

    private void SetActiveNavItem(Avalonia.Controls.Control? activeItem)
    {
        // Resetear todos los items de navegación
        if (NavClientes != null) ResetNavItem(NavClientes);
        if (NavPagos != null) ResetNavItem(NavPagos);
        if (NavRecordatorios != null) ResetNavItem(NavRecordatorios);
        if (NavResumen != null) ResetNavItem(NavResumen);
        if (NavReportes != null) ResetNavItem(NavReportes);
        if (NavBackup != null) ResetNavItem(NavBackup);

        // Activar el item seleccionado
        if (activeItem is Avalonia.Controls.Button activeButton)
        {
            _activeNavItem = activeButton;
            activeButton.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#0C63E4"));
            
            // Cambiar color del texto a blanco
            var stackPanel = activeButton.Content as Avalonia.Controls.StackPanel;
            if (stackPanel != null)
            {
                foreach (var child in stackPanel.Children)
                {
                    if (child is Avalonia.Controls.TextBlock textBlock)
                    {
                        textBlock.Foreground = Avalonia.Media.Brushes.White;
                    }
                    else if (child is Avalonia.Controls.TextBlock iconTextBlock && 
                             (iconTextBlock.Text == "👥" || iconTextBlock.Text == "💰" || 
                              iconTextBlock.Text == "⚠️" || iconTextBlock.Text == "📊" || 
                              iconTextBlock.Text == "📄" || iconTextBlock.Text == "💾"))
                    {
                        iconTextBlock.Foreground = Avalonia.Media.Brushes.White;
                    }
                }
            }
        }
        else
        {
            _activeNavItem = null;
        }
    }

    private void ResetNavItem(Avalonia.Controls.Control item)
    {
        if (item is Avalonia.Controls.Button button)
        {
            button.Background = Avalonia.Media.Brushes.Transparent;
            
            var stackPanel = button.Content as Avalonia.Controls.StackPanel;
            if (stackPanel != null)
            {
                foreach (var child in stackPanel.Children)
                {
                    if (child is Avalonia.Controls.TextBlock textBlock)
                    {
                        textBlock.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#003A8C"));
                    }
                    else if (child.GetType().Name == "SymbolIcon")
                    {
                        // Usar reflexión para establecer Foreground en el icono
                        var foregroundProperty = child.GetType().GetProperty("Foreground");
                        if (foregroundProperty != null)
                        {
                            foregroundProperty.SetValue(child, new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#003A8C")));
                        }
                    }
                }
            }
        }
    }

    private async void OnClientesClick(object? sender, RoutedEventArgs e)
    {
        SetActiveNavItem(sender as Avalonia.Controls.Control);
        try
        {
            var clientesView = new ClientesView();
            await clientesView.ShowDialog(this);
            CargarDashboard();
            VerificarRecordatorios();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error al abrir ClientesView: {ex.Message}");
            await ShowMessageAsync($"Error al abrir la vista de clientes: {ex.Message}", "Error");
        }
    }

    private async void OnPagosClick(object? sender, RoutedEventArgs e)
    {
        SetActiveNavItem(sender as Avalonia.Controls.Control);
        try
        {
            var pagosView = new PagosView();
            await pagosView.ShowDialog(this);
            CargarDashboard();
            VerificarRecordatorios();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error al abrir PagosView: {ex.Message}");
            await ShowMessageAsync($"Error al abrir la vista de pagos: {ex.Message}", "Error");
        }
    }

    private async void OnRecordatoriosClick(object? sender, RoutedEventArgs e)
    {
        SetActiveNavItem(sender as Avalonia.Controls.Control);
        try
        {
            var recordatoriosView = new RecordatoriosView();
            await recordatoriosView.ShowDialog(this);
            CargarDashboard();
            VerificarRecordatorios();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error al abrir RecordatoriosView: {ex.Message}");
            await ShowMessageAsync($"Error al abrir la vista de recordatorios: {ex.Message}", "Error");
        }
    }

    private async void OnResumenClick(object? sender, RoutedEventArgs e)
    {
        SetActiveNavItem(sender as Avalonia.Controls.Control);
        try
        {
            var resumenView = new ResumenView();
            await resumenView.ShowDialog(this);
            CargarDashboard();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error al abrir ResumenView: {ex.Message}");
            await ShowMessageAsync($"Error al abrir la vista de resumen: {ex.Message}", "Error");
        }
    }

    private async void OnReportesClick(object? sender, RoutedEventArgs e)
    {
        SetActiveNavItem(sender as Avalonia.Controls.Control);
        try
        {
            var reportesView = new ReportesView();
            await reportesView.ShowDialog(this);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error al abrir ReportesView: {ex.Message}");
            await ShowMessageAsync($"Error al abrir la vista de reportes: {ex.Message}", "Error");
        }
    }

    private async void OnBackupClick(object? sender, RoutedEventArgs e)
    {
        SetActiveNavItem(sender as Avalonia.Controls.Control);
        try
        {
            var backupView = new BackupView();
            await backupView.ShowDialog(this);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error al abrir BackupView: {ex.Message}");
            await ShowMessageAsync($"Error al abrir la vista de backup: {ex.Message}", "Error");
        }
    }

    private async Task ShowMessageAsync(string message, string title)
    {
        try
        {
            await DialogHelper.ShowMessageAsync(this, message, title);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error al mostrar mensaje: {ex.Message}");
        }
    }

    // Versión fire-and-forget para llamadas desde métodos síncronos
    private void ShowMessage(string message, string title)
    {
        _ = ShowMessageAsync(message, title);
    }
}

