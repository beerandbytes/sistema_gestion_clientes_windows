using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Huskui.Avalonia.Controls;
using GestorClientes.Data;
using GestorClientes.Helpers;
using GestorClientes.Models;
using OfficeOpenXml;
using System.IO;

namespace GestorClientes.Views;

public partial class ReportesView : AppWindow
{
    private readonly PagoRepository _pagoRepository;
    private readonly ClienteRepository _clienteRepository;
    private ObservableCollection<PagoDisplayItem> _pagosPorFecha = new();
    private ObservableCollection<PagoDisplayItem> _pagosPorMes = new();
    private ObservableCollection<PagoHistorialItem> _historial = new();
    private ObservableCollection<ClienteComboItem> _clientes = new();

    // Los controles con x:Name se generan automáticamente por Avalonia

    public ReportesView()
    {
        _pagoRepository = new PagoRepository();
        _clienteRepository = new ClienteRepository();
        
        try
        {
            InitializeComponent();
            
            if (DataGridPagosPorFecha != null)
            {
                DataGridPagosPorFecha.ItemsSource = _pagosPorFecha;
            }
            
            if (DataGridPagosPorMes != null)
            {
                DataGridPagosPorMes.ItemsSource = _pagosPorMes;
            }
            
            if (DataGridHistorial != null)
            {
                DataGridHistorial.ItemsSource = _historial;
            }
            
            if (CmbClienteHistorial != null)
            {
                CmbClienteHistorial.ItemsSource = _clientes;
            }
            
            CargarClientes();
            InicializarControles();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error al inicializar ReportesView: {ex.Message}");
        }
    }

    private void InicializarControles()
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

    private void CargarClientes()
    {
        var clientes = _clienteRepository.GetAll();
        _clientes.Clear();
        foreach (var cliente in clientes)
        {
            _clientes.Add(new ClienteComboItem
            {
                Cliente = cliente,
                NombreCompleto = $"{cliente.Nombre} {cliente.Apellidos}".Trim()
            });
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

            _pagosPorFecha.Clear();
            foreach (var pago in pagos)
            {
                var cliente = _clienteRepository.GetById(pago.ClienteId);
                var nombreCompleto = cliente != null ? $"{cliente.Nombre} {cliente.Apellidos}".Trim() : "N/A";
                _pagosPorFecha.Add(new PagoDisplayItem
                {
                    Id = pago.Id,
                    Cliente = nombreCompleto,
                    FechaPago = pago.FechaPago.ToString("yyyy-MM-dd"),
                    Cantidad = pago.Cantidad
                });
            }

            var total = pagos.Sum(p => p.Cantidad);
            if (LblTotalPorFecha != null)
            {
                LblTotalPorFecha.Text = $"Total: €{total:N2}";
            }
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

            _pagosPorMes.Clear();
            foreach (var pago in pagos)
            {
                var cliente = _clienteRepository.GetById(pago.ClienteId);
                var nombreCompleto = cliente != null ? $"{cliente.Nombre} {cliente.Apellidos}".Trim() : "N/A";
                _pagosPorMes.Add(new PagoDisplayItem
                {
                    Id = pago.Id,
                    Cliente = nombreCompleto,
                    FechaPago = pago.FechaPago.ToString("yyyy-MM-dd"),
                    Cantidad = pago.Cantidad
                });
            }

            var total = pagos.Sum(p => p.Cantidad);
            if (LblTotalPorMes != null)
            {
                LblTotalPorMes.Text = $"Total del mes: €{total:N2}";
            }
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(this, $"Error al filtrar pagos: {ex.Message}", "Error");
        }
    }

    private async void OnVerHistorialClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (CmbClienteHistorial == null)
            {
                return;
            }

            if (CmbClienteHistorial.SelectedItem is not ClienteComboItem item || item.Cliente == null)
            {
                await DialogHelper.ShowMessageAsync(this, "Seleccione un cliente.", "Validación");
                return;
            }

            var cliente = item.Cliente;
            var pagos = _pagoRepository.GetByClienteId(cliente.Id);

            _historial.Clear();
            foreach (var pago in pagos)
            {
                _historial.Add(new PagoHistorialItem
                {
                    Id = pago.Id,
                    FechaPago = pago.FechaPago.ToString("yyyy-MM-dd"),
                    Cantidad = pago.Cantidad
                });
            }

            var total = pagos.Sum(p => p.Cantidad);
            if (LblTotalHistorial != null)
            {
                LblTotalHistorial.Text = $"Total histórico de {cliente.Nombre}: €{total:N2}";
            }
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(this, $"Error al cargar historial: {ex.Message}", "Error");
        }
    }

    private async void OnDescargarExcelPorFechaClick(object? sender, RoutedEventArgs e)
    {
        if (_pagosPorFecha.Count == 0)
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
            
            await ExportarPagosPorFechaAExcel(nombreArchivo);
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(this, $"Error al exportar a Excel: {ex.Message}", "Error");
        }
    }

    private async void OnDescargarExcelPorMesClick(object? sender, RoutedEventArgs e)
    {
        if (_pagosPorMes.Count == 0)
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
            
            await ExportarPagosPorMesAExcel(nombreArchivo);
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(this, $"Error al exportar a Excel: {ex.Message}", "Error");
        }
    }

    private async void OnDescargarExcelHistorialClick(object? sender, RoutedEventArgs e)
    {
        if (_historial.Count == 0)
        {
            await DialogHelper.ShowMessageAsync(this, 
                "No hay datos para exportar. Por favor, seleccione un cliente y vea su historial primero.", 
                "Validación");
            return;
        }

        try
        {
            var clienteNombre = "Cliente";
            if (CmbClienteHistorial?.SelectedItem is ClienteComboItem item && item.Cliente != null)
            {
                clienteNombre = $"{item.Cliente.Nombre}_{item.Cliente.Apellidos}".Trim().Replace(" ", "_");
            }
            var nombreArchivo = $"Historial_Pagos_{clienteNombre}.xlsx";
            
            await ExportarHistorialAExcel(nombreArchivo);
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(this, $"Error al exportar a Excel: {ex.Message}", "Error");
        }
    }

    private async Task ExportarPagosPorFechaAExcel(string nombreArchivoSugerido)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Pagos por Fecha");

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
        foreach (var pago in _pagosPorFecha)
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

    private async Task ExportarPagosPorMesAExcel(string nombreArchivoSugerido)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Pagos por Mes");

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
        foreach (var pago in _pagosPorMes)
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

    private async Task ExportarHistorialAExcel(string nombreArchivoSugerido)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Historial de Pagos");

        // Headers
        worksheet.Cells[1, 1].Value = "Fecha de Pago";
        worksheet.Cells[1, 2].Value = "Cantidad";

        // Estilo de headers
        using (var range = worksheet.Cells[1, 1, 1, 2])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        // Datos
        int row = 2;
        decimal total = 0;
        foreach (var pago in _historial)
        {
            worksheet.Cells[row, 1].Value = pago.FechaPago;
            worksheet.Cells[row, 2].Value = pago.Cantidad;
            worksheet.Cells[row, 2].Style.Numberformat.Format = "€#,##0.00";
            total += pago.Cantidad;
            row++;
        }

        // Total
        worksheet.Cells[row, 1].Value = "Total:";
        worksheet.Cells[row, 1].Style.Font.Bold = true;
        worksheet.Cells[row, 2].Value = total;
        worksheet.Cells[row, 2].Style.Numberformat.Format = "€#,##0.00";
        worksheet.Cells[row, 2].Style.Font.Bold = true;

        // Ajustar ancho de columnas
        worksheet.Column(1).Width = 20;
        worksheet.Column(2).Width = 15;

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

