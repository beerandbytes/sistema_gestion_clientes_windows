using System.Data.SQLite;
using GestorClientes.Models;

namespace GestorClientes.Data;

public class UsuarioRepository
{
    private readonly string _connectionString;

    public UsuarioRepository()
    {
        _connectionString = DatabaseContext.GetConnectionString();
    }

    public Usuario? GetByNombreUsuario(string nombreUsuario)
    {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();

        var query = "SELECT Id, NombreUsuario, ContraseñaHash FROM Usuarios WHERE NombreUsuario = @NombreUsuario";

        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new Usuario
            {
                Id = reader.GetInt32(0),
                NombreUsuario = reader.GetString(1),
                ContraseñaHash = reader.GetString(2)
            };
        }

        return null;
    }

    public int Insert(Usuario usuario)
    {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();

        var query = @"
            INSERT INTO Usuarios (NombreUsuario, ContraseñaHash)
            VALUES (@NombreUsuario, @ContraseñaHash);
            SELECT last_insert_rowid();";

        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@NombreUsuario", usuario.NombreUsuario);
        command.Parameters.AddWithValue("@ContraseñaHash", usuario.ContraseñaHash);

        var result = command.ExecuteScalar();
        return Convert.ToInt32(result);
    }

    public bool ExisteUsuario(string nombreUsuario)
    {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();

        var query = "SELECT COUNT(*) FROM Usuarios WHERE NombreUsuario = @NombreUsuario";

        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);

        var count = Convert.ToInt32(command.ExecuteScalar());
        return count > 0;
    }

    public void InicializarUsuarioAdministrador()
    {
        const string nombreUsuarioAdmin = "admin";
        const string contraseñaAdmin = "admin123"; // Contraseña por defecto

        if (!ExisteUsuario(nombreUsuarioAdmin))
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(contraseñaAdmin);
            var usuario = new Usuario
            {
                NombreUsuario = nombreUsuarioAdmin,
                ContraseñaHash = hash
            };
            Insert(usuario);
        }
    }
}

