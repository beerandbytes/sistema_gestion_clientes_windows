using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Huskui.Avalonia.Controls;
using GestorClientes.Helpers;
using GestorClientes.Services;

namespace GestorClientes.Views;

public partial class BackupView : AppWindow
{
    private readonly BackupService _backupService;
    private ObservableCollection<BackupInfo> _backups = new();

    public BackupView()
    {
        _backupService = new BackupService();
        
        try
        {
            InitializeComponent();
            
            if (DataGridBackups != null)
            {
                DataGridBackups.ItemsSource = _backups;
            }
            
            CargarBackups();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error al inicializar BackupView: {ex.Message}");
        }
    }

    private void CargarBackups()
    {
        var backups = _backupService.ListarBackups();
        _backups.Clear();
        foreach (var backup in backups)
        {
            _backups.Add(backup);
        }
    }

    private async void OnCrearBackupClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var backupPath = _backupService.CrearBackup();
            var nombreArchivo = Path.GetFileName(backupPath);

            await DialogHelper.ShowMessageAsync(this,
                $"Backup creado exitosamente.\n\nArchivo: {nombreArchivo}",
                "Backup Exitoso");

            CargarBackups();
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(this,
                $"Error al crear backup: {ex.Message}",
                "Error");
        }
    }

    private async void OnRestaurarClick(object? sender, RoutedEventArgs e)
    {
        if (DataGridBackups?.SelectedItem is not BackupInfo backup)
        {
            await DialogHelper.ShowMessageAsync(this,
                "Seleccione un backup para restaurar.",
                "Validación");
            return;
        }

        var confirmar = await DialogHelper.ShowConfirmAsync(this,
            $"¿Está seguro de restaurar el backup '{backup.NombreArchivo}'?\n\n" +
            "Esta acción reemplazará la base de datos actual. Se creará una copia de seguridad de emergencia antes de restaurar.",
            "Confirmar Restauración");

        if (confirmar)
        {
            try
            {
                _backupService.RestaurarBackup(backup.RutaCompleta);

                await DialogHelper.ShowMessageAsync(this,
                    "Backup restaurado exitosamente.\n\n" +
                    "La aplicación se cerrará. Por favor, reiníciela para usar la base de datos restaurada.",
                    "Restauración Exitosa");

                // Cerrar aplicación
                if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.Shutdown(0);
                }
            }
            catch (Exception ex)
            {
                await DialogHelper.ShowMessageAsync(this,
                    $"Error al restaurar backup: {ex.Message}",
                    "Error");
            }
        }
    }

    private async void OnEliminarClick(object? sender, RoutedEventArgs e)
    {
        if (DataGridBackups?.SelectedItem is not BackupInfo backup)
        {
            await DialogHelper.ShowMessageAsync(this,
                "Seleccione un backup para eliminar.",
                "Validación");
            return;
        }

        var confirmar = await DialogHelper.ShowConfirmAsync(this,
            $"¿Está seguro de eliminar el backup '{backup.NombreArchivo}'?",
            "Confirmar Eliminación");

        if (confirmar)
        {
            try
            {
                _backupService.EliminarBackup(backup.RutaCompleta);

                await DialogHelper.ShowMessageAsync(this,
                    "Backup eliminado exitosamente.",
                    "Eliminación Exitosa");

                CargarBackups();
            }
            catch (Exception ex)
            {
                await DialogHelper.ShowMessageAsync(this,
                    $"Error al eliminar backup: {ex.Message}",
                    "Error");
            }
        }
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Puede usarse para habilitar/deshabilitar botones según selección
    }
}

