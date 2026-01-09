using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Huskui.Avalonia.Controls;
using GestorClientes.Data;
using GestorClientes.Helpers;
using GestorClientes.Models;
using GestorClientes.Services;
using GestorClientes.Views;
using System.IO;

namespace GestorClientes.Views;

public partial class ClientesView : AppWindow
{
    private readonly ClienteRepository _clienteRepository;
    private readonly PagoRepository _pagoRepository;
    private List<Cliente> _todosLosClientes;
    private string _busquedaTexto = string.Empty;
    private ObservableCollection<ClienteDisplayItem> _clientesDisplay = new();
    
    // Propiedad pública para binding (si es necesario)
    public ObservableCollection<ClienteDisplayItem> ClientesDisplay => _clientesDisplay;
    
    private void SuscribirEventosClienteDisplay()
    {
        // Suscribirse a cambios en la colección para manejar eventos de items individuales
        _clientesDisplay.CollectionChanged += (sender, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (ClienteDisplayItem item in e.NewItems)
                {
                    item.PropertyChanged += OnClienteDisplayItemPropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (ClienteDisplayItem item in e.OldItems)
                {
                    item.PropertyChanged -= OnClienteDisplayItemPropertyChanged;
                }
            }
        };
        
        // Suscribirse a los items existentes
        foreach (var item in _clientesDisplay)
        {
            item.PropertyChanged += OnClienteDisplayItemPropertyChanged;
        }
    }
    
    private void OnClienteDisplayItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ClienteDisplayItem.IsSelected))
        {
            // Actualizar contador inmediatamente cuando cambia IsSelected
            Dispatcher.UIThread.Post(() =>
            {
                ActualizarContadorSeleccionados();
                // Forzar actualización visual del DataGrid
                if (DataGridClientes != null)
                {
                    DataGridClientes.InvalidateVisual();
                }
            }, DispatcherPriority.Normal);
        }
    }
    
    private static string GetLogPath()
    {
        try
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var currentDir = new DirectoryInfo(baseDir);
            var maxLevels = 10;
            var level = 0;
            
            while (currentDir != null && level < maxLevels)
            {
                var cursorDir = Path.Combine(currentDir.FullName, ".cursor");
                if (Directory.Exists(cursorDir))
                {
                    var logPath = Path.Combine(cursorDir, "debug.log");
                    Directory.CreateDirectory(cursorDir);
                    return logPath;
                }
                currentDir = currentDir.Parent;
                level++;
            }
            
            var fallbackPath = Path.Combine(baseDir, "debug.log");
            return fallbackPath;
        }
        catch
        {
            return Path.Combine(Path.GetTempPath(), "debug.log");
        }
    }

    public ClientesView()
    {
        // #region agent log
        var logPath = GetLogPath();
        try { File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"D\",\"location\":\"ClientesView.axaml.cs:26\",\"message\":\"Constructor entry\",\"data\":{{\"logPath\":\"{logPath.Replace("\\", "\\\\")}\"}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
        // #endregion
        _clienteRepository = new ClienteRepository();
        _pagoRepository = new PagoRepository();
        _todosLosClientes = new List<Cliente>();
        
        try
        {
            InitializeComponent();
            // #region agent log
            try { File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"D\",\"location\":\"ClientesView.axaml.cs:34\",\"message\":\"After InitializeComponent\",\"data\":{{\"dataGridNull\":{DataGridClientes == null}}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion
            Console.WriteLine("[ClientesView] InitializeComponent completed");
            
            // Asignar ItemsSource directamente al DataGrid con x:Name
            if (DataGridClientes != null)
            {
                DataGridClientes.ItemsSource = _clientesDisplay;
                // #region agent log
                try { File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"A\",\"location\":\"ClientesView.axaml.cs:40\",\"message\":\"DataGrid ItemsSource assigned\",\"data\":{{\"itemsSourceAssigned\":{DataGridClientes.ItemsSource != null},\"collectionCount\":{_clientesDisplay.Count}}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
                // #endregion
                Console.WriteLine("[ClientesView] DataGrid ItemsSource assigned");
                Console.WriteLine($"[ClientesView] Initial collection count: {_clientesDisplay.Count}");
            }
            else
            {
                // #region agent log
                try { File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"B\",\"location\":\"ClientesView.axaml.cs:47\",\"message\":\"DataGrid is NULL\",\"data\":{{}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
                // #endregion
                Console.Error.WriteLine("[ClientesView] ERROR: DataGridClientes is null!");
            }
            
            // Suscribirse a eventos de los items para actualizar contador inmediatamente
            SuscribirEventosClienteDisplay();
            
            // Cargar clientes inmediatamente después de inicializar
            LoadClientes();
        }
        catch (Exception ex)
        {
            // #region agent log
            try { File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"D\",\"location\":\"ClientesView.axaml.cs:56\",\"message\":\"Constructor error\",\"data\":{{\"error\":\"{ex.Message.Replace("\"", "\\\"")}\"}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion
            Console.Error.WriteLine($"[ClientesView] Error al inicializar ClientesView: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
        }
    }
    
    // Los controles con x:Name se generan automáticamente por Avalonia
    private DataGrid? GetDataGridClientes() => this.FindControl<DataGrid>("DataGridClientes");

    private void OnWindowLoaded(object? sender, RoutedEventArgs e)
    {
        // #region agent log
        var logPath = GetLogPath();
        try { File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"B\",\"location\":\"ClientesView.axaml.cs:62\",\"message\":\"WindowLoaded entry\",\"data\":{{\"dataGridNull\":{DataGridClientes == null},\"dataGridVisible\":{DataGridClientes?.IsVisible ?? false},\"dataGridIsLoaded\":{DataGridClientes?.IsLoaded ?? false},\"collectionCount\":{_clientesDisplay.Count}}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
        // #endregion
        // Asegurar que el DataGrid esté correctamente configurado después de que la ventana se cargue
        if (DataGridClientes != null)
        {
            // Asegurar que el ItemsSource esté asignado
            if (DataGridClientes.ItemsSource == null || DataGridClientes.ItemsSource != _clientesDisplay)
            {
                DataGridClientes.ItemsSource = _clientesDisplay;
            }
            
            // #region agent log
            try { File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"B\",\"location\":\"ClientesView.axaml.cs:71\",\"message\":\"WindowLoaded exit\",\"data\":{{\"itemsSourceAssigned\":{DataGridClientes.ItemsSource != null},\"collectionCount\":{_clientesDisplay.Count},\"dataGridHeight\":{DataGridClientes.Height},\"dataGridWidth\":{DataGridClientes.Width}}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion
            Console.WriteLine($"[ClientesView] Window loaded, DataGrid ItemsSource count: {_clientesDisplay.Count}");
        }
    }

    private async void OnDataGridLoaded(object? sender, RoutedEventArgs e)
    {
        // Cuando el DataGrid se carga, asegurar que tenga los datos
        if (DataGridClientes != null)
        {
            if (DataGridClientes.ItemsSource == null || DataGridClientes.ItemsSource != _clientesDisplay)
            {
                DataGridClientes.ItemsSource = _clientesDisplay;
            }
            
            // Forzar actualización visual
            DataGridClientes.InvalidateVisual();
            DataGridClientes.UpdateLayout();
            
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "clientes_debug.log");
            File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] DataGrid loaded - ItemsSource count: {_clientesDisplay.Count}, DataGrid visible: {DataGridClientes.IsVisible}\n");
            Console.WriteLine($"[ClientesView] DataGrid loaded, ItemsSource count: {_clientesDisplay.Count}");
            
            // Diagnóstico: verificar dimensiones después de envolver en Border
            if (_clientesDisplay.Count > 0)
            {
                var itemsSourceCount = DataGridClientes.ItemsSource is System.Collections.ICollection collection ? collection.Count : -1;
                // Esperar un momento para que el layout se calcule
                Dispatcher.UIThread.Post(() =>
                {
                    var logPath2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "clientes_debug.log");
                    File.AppendAllText(logPath2, 
                        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] DIAGNÓSTICO DataGrid (después de Border):\n" +
                        $"  Clientes en colección: {_clientesDisplay.Count}\n" +
                        $"  ItemsSource asignado: {DataGridClientes.ItemsSource != null}\n" +
                        $"  ItemsSource count: {itemsSourceCount}\n" +
                        $"  DataGrid visible: {DataGridClientes.IsVisible}\n" +
                        $"  DataGrid IsLoaded: {DataGridClientes.IsLoaded}\n" +
                        $"  DataGrid Height: {DataGridClientes.Height}\n" +
                        $"  DataGrid Width: {DataGridClientes.Width}\n" +
                        $"  DataGrid Bounds: {DataGridClientes.Bounds}\n");
                }, DispatcherPriority.Loaded);
            }
        }
    }

    public void SetFiltro(string tipo)
    {
        if (CmbFiltro != null)
        {
            CmbFiltro.SelectedIndex = tipo switch
            {
                "Activos" => 1,
                "Vencidos" => 2,
                _ => 0
            };
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            e.Handled = true;
            LoadClientes();
        }
        else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.N)
        {
            e.Handled = true;
            AbrirModalAgregar();
        }
        else if (e.Key == Key.Escape && TxtBusqueda != null && TxtBusqueda.IsFocused)
        {
            e.Handled = true;
            LimpiarBusqueda();
        }
    }

    private void LoadClientes()
    {
        // #region agent log
        var logPath = GetLogPath();
        try { File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"A\",\"location\":\"ClientesView.axaml.cs:150\",\"message\":\"LoadClientes entry\",\"data\":{{\"dataGridNull\":{DataGridClientes == null},\"dataGridVisible\":{DataGridClientes?.IsVisible ?? false},\"collectionCount\":{_clientesDisplay.Count}}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
        // #endregion
        try
        {
            _todosLosClientes = _clienteRepository.GetAll();
            // #region agent log
            try { File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"A\",\"location\":\"ClientesView.axaml.cs:154\",\"message\":\"After GetAll\",\"data\":{{\"clientesCount\":{_todosLosClientes.Count}}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion
            Console.WriteLine($"[ClientesView] Loaded {_todosLosClientes.Count} clients from database");

            var hoy = DateTime.Today;
            var estadosParaActualizar = new List<(int id, bool activo)>();

            foreach (var cliente in _todosLosClientes)
            {
                var nuevoEstadoActivo = ClienteService.EsActivo(cliente);
                if (cliente.Activo != nuevoEstadoActivo)
                {
                    estadosParaActualizar.Add((cliente.Id, nuevoEstadoActivo));
                    cliente.Activo = nuevoEstadoActivo;
                }
            }

            if (estadosParaActualizar.Count > 0)
            {
                _clienteRepository.UpdateEstadosBatch(estadosParaActualizar);
            }

            ApplyFilter();
            
            // Asegurar que el DataGrid se actualice
            // #region agent log
            try { File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"A\",\"location\":\"ClientesView.axaml.cs:177\",\"message\":\"After ApplyFilter\",\"data\":{{\"dataGridNull\":{DataGridClientes == null},\"displayCount\":{_clientesDisplay.Count},\"itemsSourceAssigned\":{DataGridClientes?.ItemsSource != null}}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion
            if (DataGridClientes != null && _clientesDisplay.Count > 0)
            {
                Console.WriteLine($"[ClientesView] Displaying {_clientesDisplay.Count} clients in DataGrid");
            }
        }
        catch (Exception ex)
        {
            // #region agent log
            try { File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"A\",\"location\":\"ClientesView.axaml.cs:185\",\"message\":\"LoadClientes error\",\"data\":{{\"error\":\"{ex.Message.Replace("\"", "\\\"")}\"}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion
            Console.Error.WriteLine($"[ClientesView] Error loading clients: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
        }
    }

    private void ApplyFilter()
    {
        // #region agent log
        var logPath = GetLogPath();
        try { File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"A\",\"location\":\"ClientesView.axaml.cs:192\",\"message\":\"ApplyFilter entry\",\"data\":{{\"dataGridNull\":{DataGridClientes == null},\"dataGridVisible\":{DataGridClientes?.IsVisible ?? false},\"dataGridIsLoaded\":{DataGridClientes?.IsLoaded ?? false},\"collectionCount\":{_clientesDisplay.Count},\"itemsSourceAssigned\":{DataGridClientes?.ItemsSource != null}}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
        // #endregion
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

        if (CmbFiltro != null)
        {
            clientesFiltrados = CmbFiltro.SelectedIndex switch
            {
                0 => clientesFiltrados,
                1 => clientesFiltrados.Where(c => ClienteService.EsActivo(c)).ToList(),
                2 => clientesFiltrados.Where(c => !ClienteService.EsActivo(c)).ToList(),
                _ => clientesFiltrados
            };
        }
        // #region agent log
        try { File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"A\",\"location\":\"ClientesView.axaml.cs:217\",\"message\":\"After filtering\",\"data\":{{\"filtradosCount\":{clientesFiltrados.Count}}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
        // #endregion

        // Preservar selección actual antes de filtrar
        var idsSeleccionados = _clientesDisplay.Where(c => c.IsSelected).Select(c => c.Id).ToHashSet();

        // Preparar los items a agregar
        var itemsToAdd = new List<ClienteDisplayItem>();
        foreach (var cliente in clientesFiltrados)
        {
            var dias = ClienteService.GetDiasRestantes(cliente);
            var color = dias < 0 ? "Red" : dias <= 7 ? "Orange" : "Green";

            itemsToAdd.Add(new ClienteDisplayItem
            {
                Id = cliente.Id,
                NombreCompleto = $"{cliente.Nombre} {cliente.Apellidos}".Trim(),
                Edad = cliente.Edad?.ToString() ?? "N/A",
                Peso = cliente.Peso,
                Telefono = cliente.Telefono ?? "N/A",
                FechaAlta = cliente.FechaAlta.ToString("yyyy-MM-dd"),
                FechaUltimoPago = cliente.FechaUltimoPago?.ToString("yyyy-MM-dd") ?? "N/A",
                FechaVencimiento = cliente.FechaVencimiento.ToString("yyyy-MM-dd"),
                Estado = ClienteService.GetEstadoVisual(cliente),
                DiasRestantes = ClienteService.GetDiasRestantesVisual(cliente),
                DiasRestantesColor = color,
                Cliente = cliente,
                IsSelected = idsSeleccionados.Contains(cliente.Id)
            });
        }

        // Actualizar la colección en el hilo UI (CRÍTICO para Avalonia)
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            // Limpiar y agregar items de manera más eficiente
            _clientesDisplay.Clear();
            foreach (var item in itemsToAdd)
            {
                _clientesDisplay.Add(item);
            }
            
            // Suscribirse a eventos de los nuevos items agregados
            foreach (var item in _clientesDisplay)
            {
                item.PropertyChanged += OnClienteDisplayItemPropertyChanged;
            }
            
            // #region agent log
            try { File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"E\",\"location\":\"ClientesView.axaml.cs:240\",\"message\":\"After adding to collection (UI Thread)\",\"data\":{{\"collectionCount\":{_clientesDisplay.Count}}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion

            ActualizarContadorResultados(clientesFiltrados.Count, _todosLosClientes.Count);
            ActualizarContadorSeleccionados();
            
            // Forzar actualización del DataGrid
            if (DataGridClientes != null)
            {
                // Asegurar que el ItemsSource esté asignado
                if (DataGridClientes.ItemsSource == null || DataGridClientes.ItemsSource != _clientesDisplay)
                {
                    DataGridClientes.ItemsSource = _clientesDisplay;
                }
                
                // Forzar actualización visual
                DataGridClientes.InvalidateVisual();
                DataGridClientes.UpdateLayout();
                
                // #region agent log
                try { File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"F\",\"location\":\"ClientesView.axaml.cs:255\",\"message\":\"ApplyFilter exit (UI Thread)\",\"data\":{{\"displayCount\":{_clientesDisplay.Count},\"totalCount\":{_todosLosClientes.Count},\"itemsSourceAssigned\":{DataGridClientes.ItemsSource != null},\"dataGridVisible\":{DataGridClientes.IsVisible},\"dataGridIsLoaded\":{DataGridClientes.IsLoaded},\"dataGridHeight\":{DataGridClientes.Height},\"dataGridWidth\":{DataGridClientes.Width}}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
                // #endregion
                Console.WriteLine($"[ClientesView] ApplyFilter completed (UI Thread), Display count: {_clientesDisplay.Count}, Total: {_todosLosClientes.Count}");
            }
        }, DispatcherPriority.Normal);
    }

    private void ActualizarContadorResultados(int filtrados, int total)
    {
        if (LblContador != null)
        {
            if (filtrados == total)
            {
                LblContador.Text = $"Total: {total} cliente(s)";
            }
            else
            {
                LblContador.Text = $"Mostrando {filtrados} de {total} cliente(s)";
            }
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

    private void OnFiltroChanged(object? sender, SelectionChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void OnAgregarClick(object? sender, RoutedEventArgs e)
    {
        AbrirModalAgregar();
    }

    private async void AbrirModalAgregar()
    {
        var modal = new ClienteModalView();
        var result = await modal.ShowDialog<bool>(this);
        if (result)
        {
            LoadClientes();
        }
    }

    private async void OnEditarClick(object? sender, RoutedEventArgs e)
    {
        if (GetDataGridClientes()?.SelectedItem is ClienteDisplayItem item)
        {
            var cliente = _clienteRepository.GetById(item.Id);
            if (cliente == null)
            {
                await DialogHelper.ShowMessageAsync(this, "Cliente no encontrado.", "Error");
                return;
            }

            var modal = new ClienteModalView(cliente);
            var result = await modal.ShowDialog<bool>(this);
            if (result)
            {
                LoadClientes();
            }
        }
        else
        {
            await DialogHelper.ShowMessageAsync(this, "Seleccione un cliente para editar.", "Validación");
        }
    }

    private async void OnEliminarClick(object? sender, RoutedEventArgs e)
    {
        if (GetDataGridClientes()?.SelectedItem is ClienteDisplayItem item)
        {
            var cliente = _clienteRepository.GetById(item.Id);
            if (cliente == null)
            {
                await DialogHelper.ShowMessageAsync(this, "Cliente no encontrado.", "Error");
                return;
            }

            var nombreCompleto = $"{cliente.Nombre} {cliente.Apellidos}".Trim();
            var confirmar = await DialogHelper.ShowConfirmAsync(this,
                $"¿Está seguro de eliminar al cliente '{nombreCompleto}'?",
                "Confirmar eliminación");

            if (confirmar)
            {
                try
                {
                    _clienteRepository.Delete(cliente.Id);
                    await DialogHelper.ShowMessageAsync(this, "Cliente eliminado exitosamente.", "Éxito");
                    LoadClientes();
                }
                catch (Exception ex)
                {
                    await DialogHelper.ShowMessageAsync(this, $"Error al eliminar cliente: {ex.Message}", "Error");
                }
            }
        }
        else
        {
            await DialogHelper.ShowMessageAsync(this, "Seleccione un cliente para eliminar.", "Validación");
        }
    }

    private async void OnVerPagosClick(object? sender, RoutedEventArgs e)
    {
        if (GetDataGridClientes()?.SelectedItem is ClienteDisplayItem item)
        {
            var cliente = _clienteRepository.GetById(item.Id);
            if (cliente == null)
            {
                await DialogHelper.ShowMessageAsync(this, "Cliente no encontrado.", "Error");
                return;
            }

            var pagos = _pagoRepository.GetByClienteId(item.Id);
            var nombreCompleto = $"{cliente.Nombre} {cliente.Apellidos}".Trim();

            if (pagos.Count == 0)
            {
                await DialogHelper.ShowMessageAsync(this,
                    $"El cliente '{nombreCompleto}' no tiene pagos registrados.",
                    "Información");
                return;
            }

            var mensaje = $"Historial de Pagos - {nombreCompleto}\n\n";
            var total = 0m;

            foreach (var pago in pagos)
            {
                mensaje += $"Fecha: {pago.FechaPago:yyyy-MM-dd} | Cantidad: €{pago.Cantidad:N2}\n";
                total += pago.Cantidad;
            }

            mensaje += $"\nTotal: €{total:N2}";
            mensaje += $"\nPagos registrados: {pagos.Count}";

            await DialogHelper.ShowMessageAsync(this, mensaje, "Historial de Pagos");
        }
        else
        {
            await DialogHelper.ShowMessageAsync(this,
                "Seleccione un cliente para ver su historial de pagos.",
                "Validación");
        }
    }

    private async void OnDataGridDoubleClick(object? sender, RoutedEventArgs e)
    {
        OnVerPagosClick(sender, e);
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Puede usarse para habilitar/deshabilitar botones según selección
    }

    private void LimpiarBusqueda()
    {
        if (TxtBusqueda != null)
        {
            TxtBusqueda.Text = string.Empty;
        }
        _busquedaTexto = string.Empty;
        ApplyFilter();
    }

    private void OnCellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
    {
        // Actualizar contador cuando se cambia una casilla
        if (e.Column.Header?.ToString() == "" && e.Row.DataContext is ClienteDisplayItem item)
        {
            // Forzar actualización del binding
            item.OnPropertyChanged(nameof(item.IsSelected));
            ActualizarContadorSeleccionados();
            
            // Forzar actualización visual de la celda
            if (DataGridClientes != null)
            {
                DataGridClientes.InvalidateVisual();
            }
        }
    }

    private void OnCurrentCellChanged(object? sender, EventArgs e)
    {
        // Actualizar contador cuando cambia la celda actual (útil para detectar cambios en checkboxes)
        ActualizarContadorSeleccionados();
    }

    private void ActualizarContadorSeleccionados()
    {
        if (LblSeleccionados != null)
        {
            var seleccionados = _clientesDisplay.Count(c => c.IsSelected);
            LblSeleccionados.Text = $"{seleccionados} cliente(s) seleccionado(s)";
        }
    }

    private void OnSeleccionarTodoClick(object? sender, RoutedEventArgs e)
    {
        foreach (var item in _clientesDisplay)
        {
            item.IsSelected = true;
        }
        ActualizarContadorSeleccionados();
        
        // Forzar actualización visual del DataGrid
        if (DataGridClientes != null)
        {
            DataGridClientes.InvalidateVisual();
            DataGridClientes.UpdateLayout();
            // Forzar actualización de las celdas
            DataGridClientes.ItemsSource = null;
            DataGridClientes.ItemsSource = _clientesDisplay;
        }
    }

    private void OnSeleccionarNingunoClick(object? sender, RoutedEventArgs e)
    {
        foreach (var item in _clientesDisplay)
        {
            item.IsSelected = false;
        }
        ActualizarContadorSeleccionados();
        
        // Forzar actualización visual del DataGrid
        if (DataGridClientes != null)
        {
            DataGridClientes.InvalidateVisual();
            DataGridClientes.UpdateLayout();
            // Forzar actualización de las celdas
            DataGridClientes.ItemsSource = null;
            DataGridClientes.ItemsSource = _clientesDisplay;
        }
    }

    private async void OnCambiarEstadoClick(object? sender, RoutedEventArgs e)
    {
        var seleccionados = _clientesDisplay.Where(c => c.IsSelected).ToList();
        
        if (seleccionados.Count == 0)
        {
            await DialogHelper.ShowMessageAsync(this, 
                "Seleccione al menos un cliente para cambiar su estado.", 
                "Validación");
            return;
        }

        // Crear diálogo para seleccionar nuevo estado
        var dialog = new Window
        {
            Title = "Cambiar Estado",
            Width = 450,
            MinWidth = 400,
            Height = 350,
            MinHeight = 300,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = true
        };

        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };

        var panel = new StackPanel 
        { 
            Margin = new Avalonia.Thickness(20), 
            Spacing = 15,
            MinWidth = 350
        };
        
        var lblInfo = new TextBlock 
        { 
            Text = $"Se cambiará el estado de {seleccionados.Count} cliente(s).",
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        };
        panel.Children.Add(lblInfo);

        var lblTipo = new TextBlock { Text = "Tipo de cambio:", FontWeight = Avalonia.Media.FontWeight.SemiBold };
        panel.Children.Add(lblTipo);

        var cmbTipo = new ComboBox
        {
            ItemsSource = new[] { "Estado Manual", "Activo/Vencido (basado en fecha)" },
            SelectedIndex = 0,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinWidth = 200
        };
        panel.Children.Add(cmbTipo);

        var lblEstado = new TextBlock { Text = "Nuevo estado:", FontWeight = Avalonia.Media.FontWeight.SemiBold, Margin = new Avalonia.Thickness(0, 10, 0, 0) };
        panel.Children.Add(lblEstado);

        var cmbEstado = new ComboBox
        {
            ItemsSource = new[] { "Activo", "Pendiente", "Vencido" },
            SelectedIndex = 0,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinWidth = 200
        };
        panel.Children.Add(cmbEstado);

        var lblFecha = new TextBlock 
        { 
            Text = "Nueva fecha de vencimiento:", 
            FontWeight = Avalonia.Media.FontWeight.SemiBold, 
            Margin = new Avalonia.Thickness(0, 10, 0, 0),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        };
        panel.Children.Add(lblFecha);

        var dtpFecha = new DatePicker
        {
            SelectedDate = DateTimeOffset.Now.AddDays(30),
            IsVisible = false,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinWidth = 200
        };
        panel.Children.Add(dtpFecha);

        // Mostrar/ocultar controles según el tipo
        cmbTipo.SelectionChanged += (s, args) =>
        {
            if (cmbTipo.SelectedIndex == 0)
            {
                cmbEstado.IsVisible = true;
                lblEstado.IsVisible = true;
                dtpFecha.IsVisible = false;
                lblFecha.IsVisible = false;
            }
            else
            {
                cmbEstado.IsVisible = false;
                lblEstado.IsVisible = false;
                dtpFecha.IsVisible = true;
                lblFecha.IsVisible = true;
            }
        };

        var btnAceptar = new Button { Content = "Aceptar", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Avalonia.Thickness(0, 20, 0, 0) };
        btnAceptar.Classes.Add("Primary");
        var btnCancelar = new Button { Content = "Cancelar", Margin = new Avalonia.Thickness(0, 20, 10, 0), HorizontalAlignment = HorizontalAlignment.Right };

        var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        btnPanel.Children.Add(btnCancelar);
        btnPanel.Children.Add(btnAceptar);
        panel.Children.Add(btnPanel);

        scrollViewer.Content = panel;
        dialog.Content = scrollViewer;

        bool resultado = false;
        btnAceptar.Click += (s, args) => { resultado = true; dialog.Close(); };
        btnCancelar.Click += (s, args) => { dialog.Close(); };

        await dialog.ShowDialog(this);

        if (!resultado)
            return;

        try
        {
            if (cmbTipo.SelectedIndex == 0)
            {
                // Cambiar estado manual
                var nuevoEstado = cmbEstado.SelectedItem?.ToString() ?? "Activo";
                foreach (var item in seleccionados)
                {
                    if (item.Cliente != null)
                    {
                        item.Cliente.Estado = nuevoEstado;
                        _clienteRepository.Update(item.Cliente);
                    }
                }
                await DialogHelper.ShowMessageAsync(this, 
                    $"Estado cambiado a '{nuevoEstado}' para {seleccionados.Count} cliente(s).", 
                    "Éxito");
            }
            else
            {
                // Cambiar fecha de vencimiento (y por tanto Activo/Vencido)
                var nuevaFecha = dtpFecha.SelectedDate?.DateTime ?? DateTime.Today.AddDays(30);
                foreach (var item in seleccionados)
                {
                    if (item.Cliente != null)
                    {
                        item.Cliente.FechaVencimiento = nuevaFecha;
                        item.Cliente.Activo = nuevaFecha >= DateTime.Today;
                        // Limpiar estado manual para que se calcule automáticamente
                        item.Cliente.Estado = string.Empty;
                        _clienteRepository.Update(item.Cliente);
                    }
                }
                await DialogHelper.ShowMessageAsync(this, 
                    $"Fecha de vencimiento actualizada para {seleccionados.Count} cliente(s).", 
                    "Éxito");
            }

            // Deseleccionar todos y recargar
            OnSeleccionarNingunoClick(null, new RoutedEventArgs());
            LoadClientes();
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(this, 
                $"Error al cambiar estado: {ex.Message}", 
                "Error");
        }
    }

    private async void OnRegistrarPagoClick(object? sender, RoutedEventArgs e)
    {
        if (GetDataGridClientes()?.SelectedItem is not ClienteDisplayItem item)
        {
            await DialogHelper.ShowMessageAsync(this, 
                "Seleccione un cliente para registrar un pago.", 
                "Validación");
            return;
        }

        var cliente = _clienteRepository.GetById(item.Id);
        if (cliente == null)
        {
            await DialogHelper.ShowMessageAsync(this, "Cliente no encontrado.", "Error");
            return;
        }

        // Crear diálogo para ingresar pago
        var dialog = new Window
        {
            Title = "Registrar Pago",
            Width = 450,
            MinWidth = 400,
            Height = 280,
            MinHeight = 250,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = true
        };

        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };

        var panel = new StackPanel 
        { 
            Margin = new Avalonia.Thickness(20), 
            Spacing = 15,
            MinWidth = 350
        };
        
        var lblCliente = new TextBlock 
        { 
            Text = $"Cliente: {item.NombreCompleto}",
            FontWeight = Avalonia.Media.FontWeight.SemiBold,
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        };
        panel.Children.Add(lblCliente);

        var lblFecha = new TextBlock { Text = "Fecha de pago:", FontWeight = Avalonia.Media.FontWeight.SemiBold };
        panel.Children.Add(lblFecha);

        var dtpFecha = new DatePicker
        {
            SelectedDate = new DateTimeOffset(DateTime.Today),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinWidth = 200
        };
        panel.Children.Add(dtpFecha);

        var lblCantidad = new TextBlock { Text = "Cantidad:", FontWeight = Avalonia.Media.FontWeight.SemiBold };
        panel.Children.Add(lblCantidad);

        var txtCantidad = new TextBox 
        { 
            Watermark = "Ingrese la cantidad",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinWidth = 200
        };
        panel.Children.Add(txtCantidad);

        var btnAceptar = new Button { Content = "Registrar", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Avalonia.Thickness(0, 20, 0, 0) };
        btnAceptar.Classes.Add("Success");
        var btnCancelar = new Button { Content = "Cancelar", Margin = new Avalonia.Thickness(0, 20, 10, 0), HorizontalAlignment = HorizontalAlignment.Right };

        var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        btnPanel.Children.Add(btnCancelar);
        btnPanel.Children.Add(btnAceptar);
        panel.Children.Add(btnPanel);

        scrollViewer.Content = panel;
        dialog.Content = scrollViewer;

        bool resultado = false;
        btnAceptar.Click += (s, args) => { resultado = true; dialog.Close(); };
        btnCancelar.Click += (s, args) => { dialog.Close(); };

        await dialog.ShowDialog(this);

        if (!resultado)
            return;

        if (!decimal.TryParse(txtCantidad.Text, out decimal cantidad) || cantidad <= 0)
        {
            await DialogHelper.ShowMessageAsync(this,
                "Ingrese una cantidad válida mayor a cero.",
                "Validación");
            return;
        }

        try
        {
            var fechaPago = dtpFecha.SelectedDate?.DateTime ?? DateTime.Today;

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

            LoadClientes();
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(this, $"Error al registrar pago: {ex.Message}", "Error");
        }
    }
}

