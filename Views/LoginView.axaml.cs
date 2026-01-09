using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Huskui.Avalonia.Controls;
using GestorClientes.Data;

namespace GestorClientes.Views;

public partial class LoginView : AppWindow
{
    private readonly UsuarioRepository _usuarioRepository;
    public bool Autenticado { get; private set; }

    // Los controles con x:Name se generan automáticamente por Avalonia

    public LoginView()
    {
        InitializeComponent();
        _usuarioRepository = new UsuarioRepository();
        _usuarioRepository.InicializarUsuarioAdministrador();
        CargarIcono();
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

    private void OnLoginClick(object? sender, RoutedEventArgs e)
    {
        if (TxtUsuario == null || TxtContraseña == null)
        {
            return;
        }

        var usuario = TxtUsuario.Text?.Trim() ?? string.Empty;
        var contraseña = TxtContraseña.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contraseña))
        {
            ShowError("Por favor ingrese usuario y contraseña.");
            return;
        }

        try
        {
            var usuarioDb = _usuarioRepository.GetByNombreUsuario(usuario);
            if (usuarioDb == null || !BCrypt.Net.BCrypt.Verify(contraseña, usuarioDb.ContraseñaHash))
            {
                ShowError("Usuario o contraseña incorrectos.");
                TxtContraseña.Text = string.Empty;
                return;
            }

            Autenticado = true;
            
            // Abrir MainWindow y cerrar LoginView
            var mainWindow = new MainWindow();
            mainWindow.Show();
            
            // Cerrar LoginView después de abrir MainWindow
            if (this.Owner != null)
            {
                this.Close();
            }
            else
            {
                // Si es la ventana principal, cambiar la MainWindow de la aplicación
                if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.MainWindow = mainWindow;
                    this.Close();
                }
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error al autenticar: {ex.Message}");
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            OnLoginClick(sender, e);
        }
    }

    private void ShowError(string message)
    {
        if (ErrorInfoBar != null)
        {
            ErrorInfoBar.Content = message;
            ErrorInfoBar.IsVisible = true;
        }
    }
}

