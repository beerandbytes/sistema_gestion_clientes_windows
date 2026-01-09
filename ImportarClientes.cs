using System.Runtime.InteropServices;
using GestorClientes.Data;
using GestorClientes.Services;

namespace GestorClientes;

/// <summary>
/// Programa de consola para importar clientes desde CLIENTES.ods
/// Ejecutar desde la línea de comandos o desde el IDE
/// </summary>
public static class ImportarClientes
{
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool AllocConsole();

    public static void Ejecutar(string? filePath = null, bool limpiarAntes = false, bool confirmarLimpieza = true)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var logPath = Path.Combine(baseDir, "importacion_log.txt");
        
        // Escribir inicio del log inmediatamente con información de debug
        try
        {
            var debugInfo = $"=== Inicio de Importación - {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n";
            debugInfo += $"Directorio base: {baseDir}\n";
            debugInfo += $"Ruta del log: {logPath}\n";
            debugInfo += $"Archivo ODS proporcionado: {filePath ?? "null"}\n";
            File.WriteAllText(logPath, debugInfo);
        }
        catch (Exception ex)
        {
            // Si no se puede escribir el log, intentar en el directorio temporal
            try
            {
                logPath = Path.Combine(Path.GetTempPath(), "importacion_log.txt");
                File.WriteAllText(logPath, $"Error al escribir en directorio original: {ex.Message}\n");
            }
            catch { }
        }

        // Asignar una consola si no existe (para aplicaciones de escritorio)
        try
        {
            AllocConsole();
        }
        catch
        {
            // Si ya hay una consola, continuar
        }

        // Inicializar la base de datos
        try
        {
            DatabaseContext.InitializeDatabase();
            File.AppendAllText(logPath, "Base de datos inicializada correctamente.\n");
        }
        catch (Exception ex)
        {
            var error = $"Error al inicializar BD: {ex.Message}\n";
            File.AppendAllText(logPath, error);
            Console.WriteLine(error);
            return;
        }

        // Si no se proporciona ruta, usar la ruta por defecto
        if (string.IsNullOrWhiteSpace(filePath))
        {
            // Usar AppContext.BaseDirectory en lugar de Assembly.Location para compatibilidad con single-file
            var projectRoot = AppContext.BaseDirectory;
            if (projectRoot != null)
            {
                // Buscar en el directorio del ejecutable y subir niveles hasta encontrar CLIENTES.ods
                var currentDir = new DirectoryInfo(projectRoot);
                while (currentDir != null)
                {
                    var odsFile = Path.Combine(currentDir.FullName, "CLIENTES.ods");
                    if (File.Exists(odsFile))
                    {
                        filePath = odsFile;
                        break;
                    }
                    currentDir = currentDir.Parent;
                }
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                filePath = "CLIENTES.ods";
            }
        }

        Console.WriteLine("=== Importación de Clientes desde ODS ===");
        Console.WriteLine($"Archivo: {filePath}");
        Console.WriteLine();

        if (!File.Exists(filePath))
        {
            var error = $"ERROR: No se encontró el archivo: {filePath}\n";
            File.AppendAllText(logPath, error);
            Console.WriteLine(error);
            Console.WriteLine("Por favor, asegúrate de que el archivo CLIENTES.ods existe en la ruta especificada.");
            return;
        }
        
        File.AppendAllText(logPath, $"Archivo encontrado: {filePath}\n");

        ImportResult? result = null;
        try
        {
            if (limpiarAntes)
            {
                Console.WriteLine("⚠ ADVERTENCIA: Se limpiará toda la base de datos antes de importar.");
                if (confirmarLimpieza)
                {
                    Console.WriteLine("Presiona cualquier tecla para continuar o Ctrl+C para cancelar...");
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("Limpiando base de datos...");
                }
            }
            
            var importService = new ClienteImportService();
            result = importService.ImportarDesdeOds(filePath, limpiarAntes);

            Console.WriteLine("=== Resultado de la Importación ===");
            Console.WriteLine($"Clientes importados: {result.ClientesImportados}");
            Console.WriteLine($"Duplicados omitidos: {result.DuplicadosOmitidos}");
            Console.WriteLine($"Filas omitidas: {result.FilasOmitidas}");
            Console.WriteLine($"Filas con error: {result.FilasConError}");
            Console.WriteLine();

            if (result.Errores.Count > 0)
            {
                Console.WriteLine("=== Errores ===");
                foreach (var error in result.Errores)
                {
                    Console.WriteLine($"  - {error}");
                }
                Console.WriteLine();
            }

            if (result.ClientesImportados > 0)
            {
                Console.WriteLine($"✓ Importación completada exitosamente. {result.ClientesImportados} cliente(s) importado(s).");
            }
            else if (result.DuplicadosOmitidos > 0)
            {
                Console.WriteLine("ℹ No se importaron nuevos clientes. Todos los clientes ya existen en la base de datos.");
            }
            else
            {
                Console.WriteLine("⚠ No se importaron clientes. Revisa los errores arriba.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR CRÍTICO: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            
            // Crear un resultado con el error
            result = new ImportResult();
            result.Errores.Add($"ERROR CRÍTICO: {ex.Message}");
            result.Errores.Add($"Stack Trace: {ex.StackTrace}");
        }
        finally
        {
            // Siempre escribir el resumen final en el log
            try
            {
                File.AppendAllText(logPath, $"\n=== Resumen Final - {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n");
                
                if (result != null)
                {
                    File.AppendAllText(logPath, $"Clientes importados: {result.ClientesImportados}\n");
                    File.AppendAllText(logPath, $"Duplicados omitidos: {result.DuplicadosOmitidos}\n");
                    File.AppendAllText(logPath, $"Filas omitidas: {result.FilasOmitidas}\n");
                    File.AppendAllText(logPath, $"Filas con error: {result.FilasConError}\n");
                    
                    if (result.Errores.Count > 0)
                    {
                        File.AppendAllText(logPath, "\nErrores:\n");
                        foreach (var error in result.Errores)
                        {
                            File.AppendAllText(logPath, $"  - {error}\n");
                        }
                    }
                }
                else
                {
                    File.AppendAllText(logPath, "ERROR: No se pudo obtener el resultado de la importación.\n");
                }
                
                File.AppendAllText(logPath, $"\nLog guardado en: {logPath}\n");
                Console.WriteLine($"\nLog guardado en: {logPath}");
            }
            catch (Exception logEx)
            {
                Console.WriteLine($"Error al escribir el log: {logEx.Message}");
            }
        }
    }
}

