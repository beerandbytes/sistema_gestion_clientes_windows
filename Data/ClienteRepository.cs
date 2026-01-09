using System.Data.SQLite;
using GestorClientes.Models;

namespace GestorClientes.Data;

public class ClienteRepository
{
    private readonly string _connectionString;

    public ClienteRepository()
    {
        _connectionString = DatabaseContext.GetConnectionString();
    }

    public List<Cliente> GetAll()
    {
        var clientes = new List<Cliente>();
        
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();
        
        var query = "SELECT Id, Nombre, Apellidos, Edad, Peso, Telefono, FechaAlta, FechaVencimiento, FechaUltimoPago, Activo, Estado FROM Clientes ORDER BY Nombre";
        
        using var command = new SQLiteCommand(query, connection);
        using var reader = command.ExecuteReader();
        
        while (reader.Read())
        {
            var cliente = new Cliente
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Apellidos = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Edad = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                Peso = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                Telefono = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                FechaAlta = DateTime.Parse(reader.GetString(6)),
                FechaVencimiento = DateTime.Parse(reader.GetString(7)),
                FechaUltimoPago = reader.IsDBNull(8) ? null : DateTime.Parse(reader.GetString(8)),
                Activo = reader.GetInt32(9) == 1,
                Estado = reader.IsDBNull(10) ? string.Empty : reader.GetString(10)
            };
            clientes.Add(cliente);
        }
        
        return clientes;
    }

    public Cliente? GetById(int id)
    {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();
        
        var query = "SELECT Id, Nombre, Apellidos, Edad, Peso, Telefono, FechaAlta, FechaVencimiento, FechaUltimoPago, Activo, Estado FROM Clientes WHERE Id = @Id";
        
        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new Cliente
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Apellidos = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Edad = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                Peso = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                Telefono = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                FechaAlta = DateTime.Parse(reader.GetString(6)),
                FechaVencimiento = DateTime.Parse(reader.GetString(7)),
                FechaUltimoPago = reader.IsDBNull(8) ? null : DateTime.Parse(reader.GetString(8)),
                Activo = reader.GetInt32(9) == 1,
                Estado = reader.IsDBNull(10) ? string.Empty : reader.GetString(10)
            };
        }
        
        return null;
    }

    public int Insert(Cliente cliente)
    {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();
        
        var query = @"
            INSERT INTO Clientes (Nombre, Apellidos, Edad, Peso, Telefono, FechaAlta, FechaVencimiento, FechaUltimoPago, Activo, Estado)
            VALUES (@Nombre, @Apellidos, @Edad, @Peso, @Telefono, @FechaAlta, @FechaVencimiento, @FechaUltimoPago, @Activo, @Estado);
            SELECT last_insert_rowid();";
        
        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@Nombre", cliente.Nombre);
        command.Parameters.AddWithValue("@Apellidos", cliente.Apellidos ?? string.Empty);
        command.Parameters.AddWithValue("@Edad", cliente.Edad.HasValue ? (object)cliente.Edad.Value : DBNull.Value);
        command.Parameters.AddWithValue("@Peso", cliente.Peso.HasValue ? (object)cliente.Peso.Value : DBNull.Value);
        command.Parameters.AddWithValue("@Telefono", cliente.Telefono ?? string.Empty);
        command.Parameters.AddWithValue("@FechaAlta", cliente.FechaAlta.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@FechaVencimiento", cliente.FechaVencimiento.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@FechaUltimoPago", cliente.FechaUltimoPago.HasValue ? (object)cliente.FechaUltimoPago.Value.ToString("yyyy-MM-dd") : DBNull.Value);
        command.Parameters.AddWithValue("@Activo", cliente.Activo ? 1 : 0);
        command.Parameters.AddWithValue("@Estado", cliente.Estado ?? string.Empty);
        
        var result = command.ExecuteScalar();
        return Convert.ToInt32(result);
    }

    public void Update(Cliente cliente)
    {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();
        
        var query = @"
            UPDATE Clientes 
            SET Nombre = @Nombre, Apellidos = @Apellidos, Edad = @Edad, Peso = @Peso, 
                Telefono = @Telefono, FechaAlta = @FechaAlta, 
                FechaVencimiento = @FechaVencimiento, FechaUltimoPago = @FechaUltimoPago, 
                Activo = @Activo, Estado = @Estado
            WHERE Id = @Id";
        
        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@Id", cliente.Id);
        command.Parameters.AddWithValue("@Nombre", cliente.Nombre);
        command.Parameters.AddWithValue("@Apellidos", cliente.Apellidos ?? string.Empty);
        command.Parameters.AddWithValue("@Edad", cliente.Edad.HasValue ? (object)cliente.Edad.Value : DBNull.Value);
        command.Parameters.AddWithValue("@Peso", cliente.Peso.HasValue ? (object)cliente.Peso.Value : DBNull.Value);
        command.Parameters.AddWithValue("@Telefono", cliente.Telefono ?? string.Empty);
        command.Parameters.AddWithValue("@FechaAlta", cliente.FechaAlta.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@FechaVencimiento", cliente.FechaVencimiento.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@FechaUltimoPago", cliente.FechaUltimoPago.HasValue ? (object)cliente.FechaUltimoPago.Value.ToString("yyyy-MM-dd") : DBNull.Value);
        command.Parameters.AddWithValue("@Activo", cliente.Activo ? 1 : 0);
        command.Parameters.AddWithValue("@Estado", cliente.Estado ?? string.Empty);
        
        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();
        
        var query = "DELETE FROM Clientes WHERE Id = @Id";
        
        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        command.ExecuteNonQuery();
    }

    public bool ExistsByNombreAndTelefono(string nombre, string telefono, int? excludeId = null)
    {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();
        
        var query = "SELECT COUNT(*) FROM Clientes WHERE Nombre = @Nombre AND Telefono = @Telefono";
        if (excludeId.HasValue)
        {
            query += " AND Id != @ExcludeId";
        }
        
        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@Nombre", nombre.Trim());
        command.Parameters.AddWithValue("@Telefono", telefono?.Trim() ?? string.Empty);
        if (excludeId.HasValue)
        {
            command.Parameters.AddWithValue("@ExcludeId", excludeId.Value);
        }
        
        var count = Convert.ToInt32(command.ExecuteScalar());
        return count > 0;
    }

    public void UpdateEstadosBatch(List<(int id, bool activo)> estados)
    {
        if (estados == null || estados.Count == 0)
            return;

        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();
        
        using var transaction = connection.BeginTransaction();
        try
        {
            foreach (var (id, activo) in estados)
            {
                var query = "UPDATE Clientes SET Activo = @Activo WHERE Id = @Id";
                using var command = new SQLiteCommand(query, connection, transaction);
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@Activo", activo ? 1 : 0);
                command.ExecuteNonQuery();
            }
            
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void DeleteAll()
    {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();
        
        // Primero eliminar los pagos relacionados (por integridad referencial)
        var deletePagosQuery = "DELETE FROM Pagos";
        using var deletePagosCommand = new SQLiteCommand(deletePagosQuery, connection);
        deletePagosCommand.ExecuteNonQuery();
        
        // Luego eliminar todos los clientes
        var deleteClientesQuery = "DELETE FROM Clientes";
        using var deleteClientesCommand = new SQLiteCommand(deleteClientesQuery, connection);
        deleteClientesCommand.ExecuteNonQuery();
    }
}

