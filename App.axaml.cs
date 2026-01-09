using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using GestorClientes.Data;
using GestorClientes.Views;

namespace GestorClientes;

public partial class App : Application
{
    public override void Initialize()
    {
        Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Inicializar usuario administrador
            var usuarioRepository = new UsuarioRepository();
            usuarioRepository.InicializarUsuarioAdministrador();

            // Mostrar ventana de login
            var loginView = new LoginView();
            desktop.MainWindow = loginView;
        }

        base.OnFrameworkInitializationCompleted();
    }
}

