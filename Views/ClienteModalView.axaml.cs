using Avalonia.Controls;
using Avalonia.Interactivity;
using GestorClientes.Data;
using GestorClientes.Helpers;
using GestorClientes.Models;
using GestorClientes.Services;

namespace GestorClientes.Views;

public partial class ClienteModalView : Window
{
    private readonly ClienteRepository _clienteRepository;
    private readonly Cliente? _clienteEditando;
    public bool EsModoEdicion { get; private set; }

    // Los controles con x:Name se generan automáticamente por Avalonia

    // Constructor público sin parámetros requerido por Avalonia XAML loader
    public ClienteModalView()
    {
        _clienteRepository = new ClienteRepository();
        _clienteEditando = null;
        EsModoEdicion = false;
        
        try
        {
            InitializeComponent();
            ConfigurarModoAgregar();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error al inicializar ClienteModalView: {ex.Message}");
        }
    }

    public ClienteModalView(Cliente? cliente)
    {
        _clienteRepository = new ClienteRepository();
        _clienteEditando = cliente;
        EsModoEdicion = cliente != null;
        
        try
        {
            InitializeComponent();
            
            if (EsModoEdicion && cliente != null)
            {
                ConfigurarModoEdicion(cliente);
            }
            else
            {
                ConfigurarModoAgregar();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error al inicializar ClienteModalView: {ex.Message}");
        }
    }

    private void ConfigurarModoEdicion(Cliente cliente)
    {
        if (LblTitulo != null)
        {
            LblTitulo.Text = "Editar Cliente";
        }
        Title = "Editar Cliente";
        CargarDatosCliente(cliente);
        if (NumDiasMembresia != null)
        {
            NumDiasMembresia.IsReadOnly = true;
        }
    }

    private void ConfigurarModoAgregar()
    {
        if (LblTitulo != null)
        {
            LblTitulo.Text = "Agregar Cliente";
        }
        Title = "Agregar Cliente";
        if (DtpFechaAlta != null)
        {
            DtpFechaAlta.SelectedDate = new DateTimeOffset(DateTime.Today);
        }
    }

    private void CargarDatosCliente(Cliente cliente)
    {
        if (TxtNombre != null)
        {
            TxtNombre.Text = cliente.Nombre;
        }

        if (TxtApellidos != null)
        {
            TxtApellidos.Text = cliente.Apellidos ?? string.Empty;
        }

        if (NumEdad != null)
        {
            NumEdad.Value = cliente.Edad ?? 18;
        }

        if (NumPeso != null)
        {
            NumPeso.Value = cliente.Peso ?? 70;
        }

        if (TxtTelefono != null)
        {
            TxtTelefono.Text = cliente.Telefono ?? string.Empty;
        }

        if (DtpFechaAlta != null)
        {
            DtpFechaAlta.SelectedDate = new DateTimeOffset(cliente.FechaAlta);
        }
    }

    private void OnNombreChanged(object? sender, TextChangedEventArgs e)
    {
        ValidarNombre();
    }

    private void OnTelefonoChanged(object? sender, TextChangedEventArgs e)
    {
        ValidarTelefono();
    }

    private bool ValidarNombre()
    {
        if (TxtNombre == null || LblErrorNombre == null)
        {
            return false;
        }

        var (esValido, mensaje) = ClienteService.ValidarNombre(TxtNombre.Text ?? string.Empty);
        LblErrorNombre.Text = mensaje;
        LblErrorNombre.IsVisible = !esValido;
        return esValido;
    }

    private bool ValidarTelefono()
    {
        if (TxtTelefono == null || LblErrorTelefono == null)
        {
            return false;
        }

        var (esValido, mensaje) = ClienteService.ValidarTelefono(TxtTelefono.Text ?? string.Empty);
        LblErrorTelefono.Text = mensaje;
        LblErrorTelefono.IsVisible = !esValido;
        return esValido;
    }

    private async void OnGuardarClick(object? sender, RoutedEventArgs e)
    {
        if (TxtNombre == null || TxtTelefono == null || DtpFechaAlta == null)
        {
            return;
        }

        var nombreValido = ValidarNombre();
        var telefonoValido = ValidarTelefono();

        if (!nombreValido)
        {
            await DialogHelper.ShowMessageAsync(this, "Por favor, corrija los errores en el formulario.", "Validación");
            TxtNombre.Focus();
            return;
        }

        if (!telefonoValido)
        {
            await DialogHelper.ShowMessageAsync(this, "Por favor, corrija los errores en el formulario.", "Validación");
            TxtTelefono.Focus();
            return;
        }

        var nombre = TxtNombre.Text?.Trim() ?? string.Empty;
        var apellidos = TxtApellidos?.Text?.Trim() ?? string.Empty;
        var edad = NumEdad?.Value > 0 ? (int?)NumEdad.Value : null;
        var peso = NumPeso?.Value > 0 ? (decimal?)NumPeso.Value : null;
        var telefono = TxtTelefono.Text?.Trim() ?? string.Empty;

        try
        {
            if (EsModoEdicion && _clienteEditando != null)
            {
                if (_clienteRepository.ExistsByNombreAndTelefono(nombre, telefono, _clienteEditando.Id))
                {
                    await DialogHelper.ShowMessageAsync(this,
                        "Ya existe otro cliente con el mismo nombre y teléfono.",
                        "Validación");
                    return;
                }

                _clienteEditando.Nombre = nombre;
                _clienteEditando.Apellidos = apellidos;
                _clienteEditando.Edad = edad;
                _clienteEditando.Peso = peso;
                _clienteEditando.Telefono = telefono;
                _clienteEditando.FechaAlta = DtpFechaAlta.SelectedDate?.DateTime ?? DateTime.Today;

                _clienteRepository.Update(_clienteEditando);
                await DialogHelper.ShowMessageAsync(this, "Cliente actualizado exitosamente.", "Éxito");
            }
            else
            {
                if (_clienteRepository.ExistsByNombreAndTelefono(nombre, telefono))
                {
                    await DialogHelper.ShowMessageAsync(this,
                        "Ya existe un cliente con el mismo nombre y teléfono.",
                        "Validación");
                    return;
                }

                if (NumDiasMembresia == null)
                {
                    return;
                }

                var diasMembresia = (int)(NumDiasMembresia.Value ?? 30);
                var fechaAlta = DtpFechaAlta.SelectedDate?.DateTime ?? DateTime.Today;
                var cliente = new Cliente
                {
                    Nombre = nombre,
                    Apellidos = apellidos,
                    Edad = edad,
                    Peso = peso,
                    Telefono = telefono,
                    FechaAlta = fechaAlta,
                    FechaVencimiento = fechaAlta.AddDays(diasMembresia),
                    Activo = true,
                    Estado = "Activo"
                };

                _clienteRepository.Insert(cliente);
                await DialogHelper.ShowMessageAsync(this,
                    $"Cliente agregado exitosamente. Membresía válida por {diasMembresia} días.",
                    "Éxito");
            }

            Close(true);
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(this, $"Error al guardar cliente: {ex.Message}", "Error");
        }
    }

    private void OnCancelarClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}

