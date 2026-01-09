using System.Data.SQLite;
using GestorClientes.Models;

namespace GestorClientes.Data;

public class PagoRepository
{
    private readonly string _connectionString;

    public PagoRepository()
    {
        _connectionString = DatabaseContext.GetConnectionString();
    }

    public List<Pago> GetAll()
    {
        var pagos = new List<Pago>();
        
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();
        
        var query = "SELECT Id, ClienteId, FechaPago, Cantidad FROM Pagos ORDER BY FechaPago DESC";
        
        using var command = new SQLiteCommand(query, connection);
        using var reader = command.ExecuteReader();
        
        while (reader.Read())
        {
            var pago = new Pago
            {
                Id = reader.GetInt32(0),
                ClienteId = reader.GetInt32(1),
                FechaPago = DateTime.Parse(reader.GetString(2)),
                Cantidad = reader.GetDecimal(3)
            };
            pagos.Add(pago);
        }
        
        return pagos;
    }

    public List<Pago> GetByClienteId(int clienteId)
    {
        var pagos = new List<Pago>();
        
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();
        
        var query = "SELECT Id, ClienteId, FechaPago, Cantidad FROM Pagos WHERE ClienteId = @ClienteId ORDER BY FechaPago DESC";
        
        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@ClienteId", clienteId);
        using var reader = command.ExecuteReader();
        
        while (reader.Read())
        {
            var pago = new Pago
            {
                Id = reader.GetInt32(0),
                ClienteId = reader.GetInt32(1),
                FechaPago = DateTime.Parse(reader.GetString(2)),
                Cantidad = reader.GetDecimal(3)
            };
            pagos.Add(pago);
        }
        
        return pagos;
    }

    public int Insert(Pago pago)
    {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();
        
        var query = @"
            INSERT INTO Pagos (ClienteId, FechaPago, Cantidad)
            VALUES (@ClienteId, @FechaPago, @Cantidad);
            SELECT last_insert_rowid();";
        
        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@ClienteId", pago.ClienteId);
        command.Parameters.AddWithValue("@FechaPago", pago.FechaPago.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@Cantidad", pago.Cantidad);
        
        var result = command.ExecuteScalar();
        return Convert.ToInt32(result);
    }

    public List<Pago> GetByFechaRange(DateTime fechaInicio, DateTime fechaFin)
    {
        var pagos = new List<Pago>();
        
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();
        
        var query = @"
            SELECT Id, ClienteId, FechaPago, Cantidad 
            FROM Pagos 
            WHERE FechaPago >= @FechaInicio AND FechaPago <= @FechaFin 
            ORDER BY FechaPago DESC";
        
        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@FechaInicio", fechaInicio.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@FechaFin", fechaFin.ToString("yyyy-MM-dd"));
        using var reader = command.ExecuteReader();
        
        while (reader.Read())
        {
            var pago = new Pago
            {
                Id = reader.GetInt32(0),
                ClienteId = reader.GetInt32(1),
                FechaPago = DateTime.Parse(reader.GetString(2)),
                Cantidad = reader.GetDecimal(3)
            };
            pagos.Add(pago);
        }
        
        return pagos;
    }
}

