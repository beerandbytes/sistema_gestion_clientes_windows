using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using GestorClientes.Data;
using GestorClientes.Views;

namespace GestorClientes;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Configurar manejadores de excepciones globales
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        // Si se pasa el argumento --migrate o --migrar, ejecutar solo la migración y salir
        if (args.Length > 0 && (args[0] == "--migrate" || args[0] == "--migrar" || args[0] == "-m"))
        {
            try
            {
                // Ejecutar migración sin bloquear el proceso
                DatabaseContext.InitializeDatabase();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error durante la migración: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
            return;
        }
        
        // Si se pasa el argumento --importar, ejecutar la importación y salir
        if (args.Length > 0 && (args[0] == "--importar" || args[0] == "-i"))
        {
            string? filePath = args.Length > 1 ? args[1] : null;
            bool limpiarAntes = args.Contains("--limpiar") || args.Contains("-l");
            bool confirmarLimpieza = !args.Contains("--sin-confirmar") && !args.Contains("-y");
            ImportarClientes.Ejecutar(filePath, limpiarAntes, confirmarLimpieza);
            return;
        }

        // Si se pasa el argumento --poblar, ejecutar el poblamiento de datos ficticios y salir
        if (args.Length > 0 && (args[0] == "--poblar" || args[0] == "--poblar-datos" || args[0] == "-p"))
        {
            bool limpiarAntes = args.Contains("--limpiar") || args.Contains("-l");
            bool confirmarLimpieza = !args.Contains("--sin-confirmar") && !args.Contains("-y");
            PoblarDatosFicticios.Ejecutar(limpiarAntes, confirmarLimpieza);
            return;
        }

        try
        {
            // Configurar cultura a español (España) para mostrar euros
            var culture = new CultureInfo("es-ES");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // Inicializar la base de datos
            DatabaseContext.InitializeDatabase();

            // Build Avalonia app
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            LogError("Error fatal al iniciar la aplicación", ex);
            Environment.Exit(1);
        }
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        LogError("Excepción no controlada", exception);
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LogError("Excepción de tarea no observada", e.Exception);
        e.SetObserved(); // Prevenir que la aplicación se cierre
    }

    private static void LogError(string message, Exception? exception)
    {
        try
        {
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_log.txt");
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
            if (exception != null)
            {
                logMessage += $"Tipo: {exception.GetType().Name}\n";
                logMessage += $"Mensaje: {exception.Message}\n";
                logMessage += $"StackTrace: {exception.StackTrace}\n";
                if (exception.InnerException != null)
                {
                    logMessage += $"InnerException: {exception.InnerException.Message}\n";
                    logMessage += $"InnerStackTrace: {exception.InnerException.StackTrace}\n";
                }
            }
            logMessage += new string('-', 80) + "\n";
            
            File.AppendAllText(logPath, logMessage);
            Console.Error.WriteLine(logMessage);
        }
        catch
        {
            // Si no podemos escribir el log, al menos intentar escribir en consola
            Console.Error.WriteLine($"{message}: {exception?.Message}");
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}