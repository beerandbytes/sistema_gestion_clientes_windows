using System.Data.SQLite;
using System.IO;
using System.Collections.Generic;

namespace GestorClientes.Data;

public static class DatabaseContext
{
    private static string? _databasePath;
    
    private static string DatabasePath
    {
        get
        {
            if (_databasePath != null)
                return _databasePath;
            
            // Buscar base de datos en múltiples ubicaciones, priorizando las que tienen datos
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Directory.GetParent(baseDir)?.Parent?.Parent?.Parent?.FullName ?? baseDir;
            
            var possiblePaths = new[]
            {
                // Ruta cuando se ejecuta desde Visual Studio (Debug net8.0-windows)
                Path.Combine(projectRoot, "bin", "Debug", "net8.0-windows", "gestor.db"),
                // Ruta cuando se ejecuta desde Visual Studio (Debug net8.0)
                Path.Combine(projectRoot, "bin", "Debug", "net8.0", "gestor.db"),
                // Ruta cuando se ejecuta desde Visual Studio (Release)
                Path.Combine(projectRoot, "bin", "Release", "net8.0-windows", "gestor.db"),
                // Ruta directa desde BaseDirectory (ejecutable)
                Path.Combine(baseDir, "gestor.db"),
                // Ruta en la raíz del proyecto
                Path.Combine(projectRoot, "gestor.db"),
            };

            string? bestPath = null;
            int maxClientCount = -1;

            foreach (var path in possiblePaths)
            {
                var normalizedPath = Path.GetFullPath(path);
                if (File.Exists(normalizedPath))
                {
                    try
                    {
                        // Verificar cuántos clientes tiene esta base de datos
                        var clientCount = GetClientCount(normalizedPath);
                        if (clientCount > maxClientCount)
                        {
                            maxClientCount = clientCount;
                            bestPath = normalizedPath;
                        }
                    }
                    catch
                    {
                        // Si hay error, usar esta base de datos de todas formas si es la única disponible
                        if (bestPath == null)
                            bestPath = normalizedPath;
                    }
                }
            }

            // Si no se encontró ninguna, usar la ubicación por defecto
            _databasePath = bestPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gestor.db");
            
            Console.WriteLine($"[DatabaseContext] Using database: {_databasePath}");
            if (maxClientCount >= 0)
            {
                Console.WriteLine($"[DatabaseContext] Database has {maxClientCount} clients");
            }
            
            return _databasePath;
        }
    }
    
    private static int GetClientCount(string dbPath)
    {
        try
        {
            var connString = $"Data Source={dbPath};Version=3;";
            using var connection = new SQLiteConnection(connString);
            connection.Open();
            using var command = new SQLiteCommand("SELECT COUNT(*) FROM Clientes", connection);
            var result = command.ExecuteScalar();
            return Convert.ToInt32(result);
        }
        catch
        {
            return 0;
        }
    }
    
    private static string ConnectionString => $"Data Source={DatabasePath};Version=3;";

    public static string GetConnectionString()
    {
        return ConnectionString;
    }
    
    public static string GetDatabasePath()
    {
        return DatabasePath;
    }

    public static void InitializeDatabase()
    {
        // Crear la base de datos si no existe
        if (!File.Exists(DatabasePath))
        {
            SQLiteConnection.CreateFile(DatabasePath);
        }

        // Crear las tablas si no existen
        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        // Crear tabla Clientes
        var createClientesTable = @"
            CREATE TABLE IF NOT EXISTS Clientes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                Apellidos TEXT,
                Edad INTEGER,
                Peso REAL,
                Telefono TEXT,
                FechaAlta TEXT NOT NULL,
                FechaVencimiento TEXT NOT NULL,
                FechaUltimoPago TEXT,
                Activo INTEGER NOT NULL DEFAULT 0
            );";

        using (var command = new SQLiteCommand(createClientesTable, connection))
        {
            command.ExecuteNonQuery();
        }

        // Migración: Agregar nuevas columnas si no existen (para bases de datos existentes)
        MigrateClientesTable(connection);

        // Crear tabla Pagos
        var createPagosTable = @"
            CREATE TABLE IF NOT EXISTS Pagos (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ClienteId INTEGER NOT NULL,
                FechaPago TEXT NOT NULL,
                Cantidad REAL NOT NULL,
                FOREIGN KEY (ClienteId) REFERENCES Clientes(Id)
            );";

        using (var command = new SQLiteCommand(createPagosTable, connection))
        {
            command.ExecuteNonQuery();
        }

        // Migración: Renombrar columna Monto a Cantidad si existe
        MigratePagosTable(connection);

        // Crear tabla Usuarios
        var createUsuariosTable = @"
            CREATE TABLE IF NOT EXISTS Usuarios (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                NombreUsuario TEXT NOT NULL UNIQUE,
                ContraseñaHash TEXT NOT NULL
            );";

        using (var command = new SQLiteCommand(createUsuariosTable, connection))
        {
            command.ExecuteNonQuery();
        }
    }

    private static void MigrateClientesTable(SQLiteConnection connection)
    {
        // Obtener las columnas existentes de la tabla
        var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            using var pragmaCommand = new SQLiteCommand("PRAGMA table_info(Clientes)", connection);
            using var reader = pragmaCommand.ExecuteReader();
            while (reader.Read())
            {
                var columnName = reader.GetString(1); // El índice 1 contiene el nombre de la columna
                existingColumns.Add(columnName);
            }
        }
        catch (SQLiteException)
        {
            // Si no se puede leer la información de la tabla, continuar con el método anterior
        }

        // Verificar si las columnas existen y agregarlas si no existen
        var columnsToAdd = new Dictionary<string, string>
        {
            { "Apellidos", "TEXT" },
            { "Edad", "INTEGER" },
            { "Peso", "REAL" },
            { "Estado", "TEXT" },
            { "FechaUltimoPago", "TEXT" }
        };

        foreach (var column in columnsToAdd)
        {
            // Solo agregar la columna si no existe
            if (!existingColumns.Contains(column.Key))
            {
                try
                {
                    var alterQuery = $"ALTER TABLE Clientes ADD COLUMN {column.Key} {column.Value}";
                    using var command = new SQLiteCommand(alterQuery, connection);
                    command.ExecuteNonQuery();
                }
                catch (SQLiteException)
                {
                    // Ignorar errores al agregar columnas
                }
            }
        }

        // Migrar datos existentes: inicializar Estado basado en Activo y FechaVencimiento
        // Solo si la columna Estado existe (ya existía o fue agregada)
        // Actualizar existingColumns después de agregar columnas
        foreach (var column in columnsToAdd)
        {
            if (!existingColumns.Contains(column.Key))
            {
                existingColumns.Add(column.Key); // Agregar a la lista después de crearla
            }
        }
        
        if (existingColumns.Contains("Estado"))
        {
            try
            {
                var hoy = DateTime.Today.ToString("yyyy-MM-dd");
                var updateQuery = @"
                    UPDATE Clientes 
                    SET Estado = CASE 
                        WHEN FechaVencimiento < @Hoy THEN 'Vencido'
                        WHEN Activo = 1 THEN 'Activo'
                        ELSE 'Pendiente'
                    END
                    WHERE Estado IS NULL OR Estado = ''";
                using var command = new SQLiteCommand(updateQuery, connection);
                command.Parameters.AddWithValue("@Hoy", hoy);
                command.ExecuteNonQuery();
            }
            catch (SQLiteException)
            {
                // Ignorar errores en la migración de datos
            }
        }
    }

    private static void MigratePagosTable(SQLiteConnection connection)
    {
        // Verificar si la columna Monto existe y renombrarla a Cantidad
        try
        {
            // Verificar si existe la columna Monto
            var columnExists = false;
            using var pragmaCommand = new SQLiteCommand("PRAGMA table_info(Pagos)", connection);
            using var reader = pragmaCommand.ExecuteReader();
            while (reader.Read())
            {
                var columnName = reader.GetString(1); // El índice 1 contiene el nombre de la columna
                if (columnName.Equals("Monto", StringComparison.OrdinalIgnoreCase))
                {
                    columnExists = true;
                    break;
                }
            }

            // Si existe Monto pero no Cantidad, renombrar la columna
            if (columnExists)
            {
                var cantidadExists = false;
                pragmaCommand.Dispose();
                reader.Dispose();
                
                using var pragmaCommand2 = new SQLiteCommand("PRAGMA table_info(Pagos)", connection);
                using var reader2 = pragmaCommand2.ExecuteReader();
                while (reader2.Read())
                {
                    var columnName = reader2.GetString(1);
                    if (columnName.Equals("Cantidad", StringComparison.OrdinalIgnoreCase))
                    {
                        cantidadExists = true;
                        break;
                    }
                }

                if (!cantidadExists)
                {
                    // SQLite no soporta ALTER TABLE RENAME COLUMN directamente en versiones antiguas
                    // Usamos el método de crear nueva tabla, copiar datos, eliminar vieja y renombrar
                    var migrateQuery = @"
                        CREATE TABLE IF NOT EXISTS Pagos_new (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            ClienteId INTEGER NOT NULL,
                            FechaPago TEXT NOT NULL,
                            Cantidad REAL NOT NULL,
                            FOREIGN KEY (ClienteId) REFERENCES Clientes(Id)
                        );
                        INSERT INTO Pagos_new (Id, ClienteId, FechaPago, Cantidad)
                        SELECT Id, ClienteId, FechaPago, Monto FROM Pagos;
                        DROP TABLE Pagos;
                        ALTER TABLE Pagos_new RENAME TO Pagos;";
                    
                    using var command = new SQLiteCommand(migrateQuery, connection);
                    command.ExecuteNonQuery();
                }
            }
        }
        catch (SQLiteException)
        {
            // Si hay error, intentar método alternativo más simple
            try
            {
                // Verificar si solo existe Monto
                var hasMonto = false;
                var hasCantidad = false;
                using var pragmaCommand = new SQLiteCommand("PRAGMA table_info(Pagos)", connection);
                using var reader = pragmaCommand.ExecuteReader();
                while (reader.Read())
                {
                    var columnName = reader.GetString(1);
                    if (columnName.Equals("Monto", StringComparison.OrdinalIgnoreCase))
                        hasMonto = true;
                    if (columnName.Equals("Cantidad", StringComparison.OrdinalIgnoreCase))
                        hasCantidad = true;
                }

                if (hasMonto && !hasCantidad)
                {
                    // Método alternativo: agregar columna Cantidad, copiar datos, eliminar Monto
                    var alterQuery1 = "ALTER TABLE Pagos ADD COLUMN Cantidad REAL";
                    using var cmd1 = new SQLiteCommand(alterQuery1, connection);
                    cmd1.ExecuteNonQuery();

                    var updateQuery = "UPDATE Pagos SET Cantidad = Monto WHERE Cantidad IS NULL";
                    using var cmd2 = new SQLiteCommand(updateQuery, connection);
                    cmd2.ExecuteNonQuery();

                    // Nota: SQLite no permite eliminar columnas directamente
                    // La columna Monto quedará pero no se usará
                }
            }
            catch
            {
                // Ignorar errores en la migración
            }
        }
    }
}

