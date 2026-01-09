using System.IO;
using GestorClientes.Data;

namespace GestorClientes.Services;

public class BackupService
{
    private readonly string _backupDirectory;
    private readonly string _databasePath;

    public BackupService()
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _backupDirectory = Path.Combine(appDirectory, "Backups");
        _databasePath = Path.Combine(appDirectory, "gestor.db");

        // Crear directorio de backups si no existe
        if (!Directory.Exists(_backupDirectory))
        {
            Directory.CreateDirectory(_backupDirectory);
        }
    }

    public string CrearBackup()
    {
        if (!File.Exists(_databasePath))
        {
            throw new FileNotFoundException("No se encontró la base de datos para respaldar.");
        }

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupFileName = $"gestor_{timestamp}.db";
        var backupPath = Path.Combine(_backupDirectory, backupFileName);

        File.Copy(_databasePath, backupPath, true);
        return backupPath;
    }

    public void RestaurarBackup(string backupPath)
    {
        if (!File.Exists(backupPath))
        {
            throw new FileNotFoundException("No se encontró el archivo de backup especificado.");
        }

        // Cerrar cualquier conexión activa a la base de datos antes de restaurar
        // Para SQLite, simplemente copiamos el archivo
        
        // Hacer backup del archivo actual antes de restaurar (por si acaso)
        if (File.Exists(_databasePath))
        {
            var emergencyBackup = $"{_databasePath}.emergency_{DateTime.Now:yyyyMMdd_HHmmss}";
            File.Copy(_databasePath, emergencyBackup, true);
        }

        File.Copy(backupPath, _databasePath, true);
    }

    public List<BackupInfo> ListarBackups()
    {
        var backups = new List<BackupInfo>();

        if (!Directory.Exists(_backupDirectory))
        {
            return backups;
        }

        var backupFiles = Directory.GetFiles(_backupDirectory, "gestor_*.db")
            .OrderByDescending(f => File.GetCreationTime(f))
            .ToList();

        foreach (var file in backupFiles)
        {
            var fileInfo = new FileInfo(file);
            backups.Add(new BackupInfo
            {
                NombreArchivo = Path.GetFileName(file),
                RutaCompleta = file,
                FechaCreacion = fileInfo.CreationTime,
                Tamanio = fileInfo.Length
            });
        }

        return backups;
    }

    public void EliminarBackup(string backupPath)
    {
        if (!File.Exists(backupPath))
        {
            throw new FileNotFoundException("No se encontró el archivo de backup especificado.");
        }

        File.Delete(backupPath);
    }
}

public class BackupInfo
{
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaCompleta { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public long Tamanio { get; set; }

    public string TamanioFormateado => 
        Tamanio < 1024 ? $"{Tamanio} B" :
        Tamanio < 1024 * 1024 ? $"{Tamanio / 1024.0:F2} KB" :
        $"{Tamanio / (1024.0 * 1024.0):F2} MB";
}

