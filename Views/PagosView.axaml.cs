using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Huskui.Avalonia.Controls;
using GestorClientes.Data;
using GestorClientes.Helpers;
using GestorClientes.Models;
using OfficeOpenXml;

namespace GestorClientes.Views;

public partial class PagosView : AppWindow
{
    private readonly PagoRepository _pagoRepository;
    private readonly ClienteRepository _clienteRepository;
    private List<Cliente> _todosLosClientes;
    private string _busquedaTexto = string.Empty;
    private ObservableCollection<PagoDisplayItem> _pagosDisplay = new();
    private ObservableCollection<ClienteComboItem> _clientesFiltrados = new();
    private List<PagoDisplayItem> _pagosFiltrados = new();

    // Los controles con x:Name se generan automáticamente por Avalonia

    public PagosView()
    {
        _pagoRepository = new PagoRepository();
        _clienteRepository = new ClienteRepository();
        _todosLosClientes = new List<Cliente>();
        
        try
        {
            InitializeComponent();
            
            if (CmbCliente != null)
            {
                CmbCliente.ItemsSource = _clientesFiltrados;
            }
            if (DataGridPagos != null)
            {
                DataGridPagos.ItemsSource = _pagosDisplay;
            }
            if (DtpFechaPago != null)
            {
                DtpFechaPago.SelectedDate = new DateTimeOffset(DateTime.Today);
            }
            
            // Inicializar controles de filtro
            InicializarControlesFiltro();
            
            LoadClientes();
            LoadPagos();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error al inicializar PagosView: {ex.Message}");
        }
    }

    private void LoadClientes()
    {
        try
        {
            _todosLosClientes = _clienteRepository.GetAll();
            Console.WriteLine($"[PagosView] Loaded {_todosLosClientes.Count} clients from database");
            ApplyFilter();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[PagosView] Error loading clients: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
        }
    }

    private void ApplyFilter()
    {
        List<Cliente> clientesFiltrados = _todosLosClientes;

        if (!string.IsNullOrWhiteSpace(_busquedaTexto))
        {
            var busquedaLower = _busquedaTexto.ToLower();
            clientesFiltrados = clientesFiltrados.Where(c =>
                c.Nombre.ToLower().Contains(busquedaLower) ||
                (!string.IsNullOrEmpty(c.Apellidos) && c.Apellidos.ToLower().Contains(busquedaLower)) ||
                (c.Telefono != null && c.Telefono.ToLower().Contains(busquedaLower))
            ).ToList();
        }

        _clientesFiltrados.Clear();
        foreach (var cliente in clientesFiltrados)
        {
            _clientesFiltrados.Add(new ClienteComboItem
            {
                Cliente = cliente,
                NombreCompleto = $"{cliente.Nombre} {cliente.Apellidos}".Trim()
            });
        }
    }

    private void InicializarControlesFiltro()
    {
        if (DtpFechaInicio != null)
        {
            DtpFechaInicio.SelectedDate = new DateTimeOffset(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
        }

        if (DtpFechaFin != null)
        {
            DtpFechaFin.SelectedDate = new DateTimeOffset(DateTime.Today);
        }
        
        var meses = new[] { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
                           "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
        
        if (CmbMes != null)
        {
            CmbMes.ItemsSource = meses;
            CmbMes.SelectedIndex = DateTime.Today.Month - 1;
        }
        
        if (TxtAnio != null)
        {
            TxtAnio.Text = DateTime.Today.Year.ToString();
        }
    }

    private void LoadPagos()
    {
        try
        {
            var pagos = _pagoRepository.GetAll();
            Console.WriteLine($"[PagosView] Loaded {pagos.Count} payments from database");

            _pagosDisplay.Clear();
            _pagosFiltrados.Clear();
            foreach (var pago in pagos.OrderByDescending(p => p.FechaPago))
            {
                var cliente = _clienteRepository.GetById(pago.ClienteId);
                var nombreCompleto = cliente != null ? $"{cliente.Nombre} {cliente.Apellidos}".Trim() : "N/A";
                var item = new PagoDisplayItem
                {
                    Id = pago.Id,
                    Cliente = nombreCompleto,
                    FechaPago = pago.FechaPago.ToString("yyyy-MM-dd"),
                    Cantidad = pago.Cantidad
                };
                _pagosDisplay.Add(item);
                _pagosFiltrados.Add(item);
            }
            
            ActualizarTotal();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[PagosView] Error loading payments: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
        }
    }
    
    private void ActualizarTotal()
    {
        var total = _pagosFiltrados.Sum(p => p.Cantidad);
        var contador = _pagosFiltrados.Count;
        
        if (LblTotal != null)
        {
            LblTotal.Text = $"Total: €{total:N2}";
        }
        
        if (LblContador != null)
        {
            LblContador.Text = $"Registros: {contador}";
        }
    }

    private void OnBusquedaChanged(object? sender, TextChangedEventArgs e)
    {
        if (TxtBusqueda != null)
        {
            _busquedaTexto = TxtBusqueda.Text ?? string.Empty;
        }
        ApplyFilter();
    }

    private void OnClienteChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Puede usarse para validaciones adicionales
    }

    private async void OnRegistrarPagoClick(object? sender, RoutedEventArgs e)
    {
        if (CmbCliente == null || TxtCantidad == null || DtpFechaPago == null)
        {
            return;
        }

        if (CmbCliente.SelectedItem is not ClienteComboItem item || item.Cliente == null)
        {
            await DialogHelper.ShowMessageAsync(this, "Seleccione un cliente.", "Validación");
            return;
        }

        if (!decimal.TryParse(TxtCantidad.Text, out decimal cantidad) || cantidad <= 0)
        {
            await DialogHelper.ShowMessageAsync(this,
                "Ingrese una cantidad válida mayor a cero.",
                "Validación");
            return;
        }

        try
        {
            var fechaPago = DtpFechaPago.SelectedDate?.DateTime ?? DateTime.Today;

            var cliente = item.Cliente;
            var pago = new Pago
            {
                ClienteId = cliente.Id,
                FechaPago = fechaPago.Date,
                Cantidad = cantidad
            };

            _pagoRepository.Insert(pago);

            cliente.FechaUltimoPago = fechaPago.Date;
            cliente.FechaVencimiento = fechaPago.Date.AddDays(30);
            cliente.Activo = true;
            cliente.Estado = "Activo";
            _clienteRepository.Update(cliente);

            await DialogHelper.ShowMessageAsync(this,
                "Pago registrado exitosamente. La membresía del cliente ha sido renovada.",
                "Éxito");

            TxtCantidad.Text = string.Empty;
            DtpFechaPago.SelectedDate = new DateTimeOffset(DateTime.Today);
            CmbCliente.SelectedItem = null;
            LoadPagos();
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(this, $"Error al registrar pago: {ex.Message}", "Error");
        }
    }

    private async void OnFiltrarPorFechaClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (DtpFechaInicio == null || DtpFechaFin == null)
            {
                return;
            }

            var fechaInicio = DtpFechaInicio.SelectedDate?.DateTime ?? DateTime.Today;
            var fechaFin = DtpFechaFin.SelectedDate?.DateTime ?? DateTime.Today;

            if (fechaInicio > fechaFin)
            {
                await DialogHelper.ShowMessageAsync(this,
                    "La fecha de inicio debe ser menor o igual a la fecha fin.",
                    "Validación");
                return;
            }

            var pagos = _pagoRepository.GetByFechaRange(fechaInicio, fechaFin);

            _pagosFiltrados.Clear();
            foreach (var pago in pagos.OrderByDescending(p => p.FechaPago))
            {
                var cliente = _clienteRepository.GetById(pago.ClienteId);
                var nombreCompleto = cliente != null ? $"{cliente.Nombre} {cliente.Apellidos}".Trim() : "N/A";
                _pagosFiltrados.Add(new PagoDisplayItem
                {
                    Id = pago.Id,
                    Cliente = nombreCompleto,
                    FechaPago = pago.FechaPago.ToString("yyyy-MM-dd"),
                    Cantidad = pago.Cantidad
                });
            }

            _pagosDisplay.Clear();
            foreach (var item in _pagosFiltrados)
            {
                _pagosDisplay.Add(item);
            }

            ActualizarTotal();
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(this, $"Error al filtrar pagos: {ex.Message}", "Error");
        }
    }

    private async void OnFiltrarPorMesClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (TxtAnio == null || CmbMes == null)
            {
                return;
            }

            if (!int.TryParse(TxtAnio.Text, out int anio))
            {
                await DialogHelper.ShowMessageAsync(this, "Ingrese un año válido.", "Validación");
                return;
            }

            var mes = CmbMes.SelectedIndex + 1;
            var fechaInicio = new DateTime(anio, mes, 1);
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            var pagos = _pagoRepository.GetByFechaRange(fechaInicio, fechaFin);

            _pagosFiltrados.Clear();
            foreach (var pago in pagos.OrderByDescending(p => p.FechaPago))
            {
                var cliente = _clienteRepository.GetById(pago.ClienteId);
                var nombreCompleto = cliente != null ? $"{cliente.Nombre} {cliente.Apellidos}".Trim() : "N/A";
                _pagosFiltrados.Add(new PagoDisplayItem
                {
                    Id = pago.Id,
                    Cliente = nombreCompleto,
                    FechaPago = pago.FechaPago.ToString("yyyy-MM-dd"),
                    Cantidad = pago.Cantidad
                });
            }

            _pagosDisplay.Clear();
            foreach (var item in _pagosFiltrados)
            {
                _pagosDisplay.Add(item);
            }

            ActualizarTotal();
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(this, $"Error al filtrar pagos: {ex.Message}", "Error");
        }
    }

    private async void OnExportarExcelPorFechaClick(object? sender, RoutedEventArgs e)
    {
        if (_pagosFiltrados.Count == 0)
        {
            await DialogHelper.ShowMessageAsync(this, 
                "No hay datos para exportar. Por favor, filtre primero los pagos.", 
                "Validación");
            return;
        }

        try
        {
            var fechaInicio = DtpFechaInicio?.SelectedDate?.DateTime ?? DateTime.Today;
            var fechaFin = DtpFechaFin?.SelectedDate?.DateTime ?? DateTime.Today;
            var nombreArchivo = $"Pagos_Por_Fecha_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}.xlsx";
            
            await ExportarPagosAExcel(nombreArchivo, _pagosFiltrados);
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(this, $"Error al exportar a Excel: {ex.Message}", "Error");
        }
    }

    private async void OnExportarExcelPorMesClick(object? sender, RoutedEventArgs e)
    {
        if (_pagosFiltrados.Count == 0)
        {
            await DialogHelper.ShowMessageAsync(this, 
                "No hay datos para exportar. Por favor, filtre primero los pagos.", 
                "Validación");
            return;
        }

        try
        {
            var mes = CmbMes?.SelectedIndex + 1 ?? DateTime.Today.Month;
            var anio = int.TryParse(TxtAnio?.Text, out int a) ? a : DateTime.Today.Year;
            var nombreArchivo = $"Pagos_Por_Mes_{anio}_{mes:D2}.xlsx";
            
            await ExportarPagosAExcel(nombreArchivo, _pagosFiltrados);
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(this, $"Error al exportar a Excel: {ex.Message}", "Error");
        }
    }

    private void OnTabControlSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Cuando se selecciona la pestaña "Todos", mostrar todos los pagos
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is TabItem tabItem && tabItem.Header?.ToString() == "Todos")
        {
            MostrarTodosLosPagos();
        }
    }

    private void MostrarTodosLosPagos()
    {
        try
        {
            var pagos = _pagoRepository.GetAll();

            _pagosFiltrados.Clear();
            _pagosDisplay.Clear();
            foreach (var pago in pagos.OrderByDescending(p => p.FechaPago))
            {
                var cliente = _clienteRepository.GetById(pago.ClienteId);
                var nombreCompleto = cliente != null ? $"{cliente.Nombre} {cliente.Apellidos}".Trim() : "N/A";
                var item = new PagoDisplayItem
                {
                    Id = pago.Id,
                    Cliente = nombreCompleto,
                    FechaPago = pago.FechaPago.ToString("yyyy-MM-dd"),
                    Cantidad = pago.Cantidad
                };
                _pagosFiltrados.Add(item);
                _pagosDisplay.Add(item);
            }

            ActualizarTotal();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[PagosView] Error al mostrar todos los pagos: {ex.Message}");
        }
    }

    private async void OnExportarExcelTodosClick(object? sender, RoutedEventArgs e)
    {
        if (_pagosDisplay.Count == 0)
        {
            await DialogHelper.ShowMessageAsync(this, 
                "No hay datos para exportar.", 
                "Validación");
            return;
        }

        try
        {
            var nombreArchivo = $"Pagos_Completo_{DateTime.Today:yyyyMMdd}.xlsx";
            
            await ExportarPagosAExcel(nombreArchivo, _pagosDisplay.ToList());
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(this, $"Error al exportar a Excel: {ex.Message}", "Error");
        }
    }

    private async Task ExportarPagosAExcel(string nombreArchivoSugerido, List<PagoDisplayItem> pagos)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Historial de Pagos");

        // Headers
        worksheet.Cells[1, 1].Value = "Cliente";
        worksheet.Cells[1, 2].Value = "Fecha de Pago";
        worksheet.Cells[1, 3].Value = "Cantidad";

        // Estilo de headers
        using (var range = worksheet.Cells[1, 1, 1, 3])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        // Datos
        int row = 2;
        decimal total = 0;
        foreach (var pago in pagos)
        {
            worksheet.Cells[row, 1].Value = pago.Cliente;
            worksheet.Cells[row, 2].Value = pago.FechaPago;
            worksheet.Cells[row, 3].Value = pago.Cantidad;
            worksheet.Cells[row, 3].Style.Numberformat.Format = "€#,##0.00";
            total += pago.Cantidad;
            row++;
        }

        // Total
        worksheet.Cells[row, 2].Value = "Total:";
        worksheet.Cells[row, 2].Style.Font.Bold = true;
        worksheet.Cells[row, 3].Value = total;
        worksheet.Cells[row, 3].Style.Numberformat.Format = "€#,##0.00";
        worksheet.Cells[row, 3].Style.Font.Bold = true;

        // Ajustar ancho de columnas
        worksheet.Column(1).Width = 30;
        worksheet.Column(2).Width = 15;
        worksheet.Column(3).Width = 15;

        // Guardar archivo
        var file = await SaveFileAsync(nombreArchivoSugerido);
        if (file != null)
        {
            await using var stream = await file.OpenWriteAsync();
            await package.SaveAsAsync(stream);
            await DialogHelper.ShowMessageAsync(this, 
                $"Archivo exportado exitosamente: {file.Name}", 
                "Éxito");
        }
    }

    private async Task<IStorageFile?> SaveFileAsync(string nombreArchivoSugerido)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider == null)
            return null;
        
        var storageProvider = topLevel.StorageProvider;

        var fileType = new FilePickerFileType("Excel")
        {
            Patterns = new[] { "*.xlsx" },
            AppleUniformTypeIdentifiers = new[] { "com.microsoft.excel.xlsx" },
            MimeTypes = new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" }
        };

        var options = new FilePickerSaveOptions
        {
            SuggestedFileName = nombreArchivoSugerido,
            FileTypeChoices = new[] { fileType },
            DefaultExtension = "xlsx"
        };

        return await storageProvider.SaveFilePickerAsync(options);
    }
}

