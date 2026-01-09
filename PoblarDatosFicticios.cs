using GestorClientes.Data;
using GestorClientes.Models;

namespace GestorClientes;

public static class PoblarDatosFicticios
{
    public static void Ejecutar(bool limpiarAntes = false, bool confirmarLimpieza = true)
    {
        try
        {
            Console.WriteLine("=== POBLAR BASE DE DATOS CON DATOS FICTICIOS ===\n");

            // Inicializar la base de datos
            DatabaseContext.InitializeDatabase();

            var clienteRepository = new ClienteRepository();
            var pagoRepository = new PagoRepository();
            var usuarioRepository = new UsuarioRepository();

            // Verificar si hay datos existentes
            var clientesExistentes = clienteRepository.GetAll();
            if (clientesExistentes.Count > 0)
            {
                if (limpiarAntes)
                {
                    if (confirmarLimpieza)
                    {
                        Console.WriteLine($"⚠️  ADVERTENCIA: Se encontraron {clientesExistentes.Count} clientes existentes.");
                        Console.Write("¿Desea eliminar todos los datos existentes antes de poblar? (s/n): ");
                        var respuesta = Console.ReadLine()?.Trim().ToLower();
                        if (respuesta != "s" && respuesta != "si" && respuesta != "y" && respuesta != "yes")
                        {
                            Console.WriteLine("Operación cancelada.");
                            return;
                        }
                    }
                    Console.WriteLine("Eliminando datos existentes...");
                    clienteRepository.DeleteAll();
                    Console.WriteLine("✓ Datos existentes eliminados.\n");
                }
                else
                {
                    Console.WriteLine($"⚠️  Se encontraron {clientesExistentes.Count} clientes existentes.");
                    Console.WriteLine("Los nuevos datos se agregarán sin eliminar los existentes.\n");
                }
            }

            // Asegurar que existe un usuario administrador
            usuarioRepository.InicializarUsuarioAdministrador();
            Console.WriteLine("✓ Usuario administrador verificado.\n");

            // Crear clientes ficticios con diferentes estados
            Console.WriteLine("Creando clientes ficticios...");
            var clientes = CrearClientesFicticios();
            var clienteIds = new List<int>();

            foreach (var cliente in clientes)
            {
                var id = clienteRepository.Insert(cliente);
                clienteIds.Add(id);
                Console.WriteLine($"  ✓ Cliente creado: {cliente.Nombre} {cliente.Apellidos} (ID: {id})");
            }

            Console.WriteLine($"\n✓ {clientes.Count} clientes creados exitosamente.\n");

            // Crear pagos ficticios
            Console.WriteLine("Creando pagos ficticios...");
            var pagos = CrearPagosFicticios(clienteIds);
            var totalPagos = 0;

            foreach (var pago in pagos)
            {
                pagoRepository.Insert(pago);
                totalPagos++;
            }

            Console.WriteLine($"✓ {totalPagos} pagos creados exitosamente.\n");

            // Resumen final
            Console.WriteLine("=== RESUMEN ===");
            Console.WriteLine($"Total de clientes: {clienteRepository.GetAll().Count}");
            Console.WriteLine($"Total de pagos: {pagoRepository.GetAll().Count}");
            Console.WriteLine("\n✓ Base de datos poblada exitosamente con datos ficticios.");
            Console.WriteLine("\nLos datos incluyen:");
            Console.WriteLine("  - Clientes activos (con fechas de vencimiento futuras)");
            Console.WriteLine("  - Clientes vencidos (con fechas de vencimiento pasadas)");
            Console.WriteLine("  - Clientes próximos a vencer (en los próximos 7 días)");
            Console.WriteLine("  - Pagos del mes actual");
            Console.WriteLine("  - Pagos de meses anteriores (para reportes históricos)");
            Console.WriteLine("  - Diferentes edades, pesos y teléfonos");
            Console.WriteLine("\nPuede iniciar la aplicación para ver todos los datos.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"\n❌ Error al poblar la base de datos: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
    }

    private static List<Cliente> CrearClientesFicticios()
    {
        var hoy = DateTime.Today;
        var random = new Random(42); // Semilla fija para datos consistentes

        var nombres = new[]
        {
            "María", "José", "Ana", "Carlos", "Laura", "Juan", "Carmen", "Pedro",
            "Isabel", "Miguel", "Patricia", "Francisco", "Lucía", "Antonio", "Elena",
            "Manuel", "Sofía", "David", "Marta", "Javier", "Paula", "Daniel", "Cristina"
        };

        var apellidos = new[]
        {
            "García", "Rodríguez", "González", "Fernández", "López", "Martínez", "Sánchez",
            "Pérez", "Gómez", "Martín", "Jiménez", "Ruiz", "Hernández", "Díaz", "Moreno",
            "Álvarez", "Muñoz", "Romero", "Alonso", "Gutiérrez", "Navarro", "Torres", "Domínguez"
        };

        var clientes = new List<Cliente>();
        var telefonosUsados = new HashSet<string>();

        // Clientes activos (fecha vencimiento futura, más de 7 días)
        for (int i = 0; i < 15; i++)
        {
            var nombre = nombres[random.Next(nombres.Length)];
            var apellido = apellidos[random.Next(apellidos.Length)];
            var telefono = GenerarTelefonoUnico(telefonosUsados, random);
            
            var fechaAlta = hoy.AddMonths(-random.Next(1, 12));
            var diasVencimiento = random.Next(8, 60); // Entre 8 y 60 días
            var fechaVencimiento = hoy.AddDays(diasVencimiento);
            var fechaUltimoPago = random.Next(100) < 80 ? hoy.AddDays(-random.Next(1, 30)) : (DateTime?)null;

            clientes.Add(new Cliente
            {
                Nombre = nombre,
                Apellidos = apellido,
                Edad = random.Next(18, 65),
                Peso = Math.Round((decimal)(random.NextDouble() * 40 + 50), 1), // Entre 50 y 90 kg
                Telefono = telefono,
                FechaAlta = fechaAlta,
                FechaVencimiento = fechaVencimiento,
                FechaUltimoPago = fechaUltimoPago,
                Activo = true,
                Estado = "Activo"
            });
        }

        // Clientes próximos a vencer (en los próximos 7 días)
        for (int i = 0; i < 5; i++)
        {
            var nombre = nombres[random.Next(nombres.Length)];
            var apellido = apellidos[random.Next(apellidos.Length)];
            var telefono = GenerarTelefonoUnico(telefonosUsados, random);
            
            var fechaAlta = hoy.AddMonths(-random.Next(1, 8));
            var diasVencimiento = random.Next(1, 7); // Entre 1 y 7 días
            var fechaVencimiento = hoy.AddDays(diasVencimiento);
            var fechaUltimoPago = random.Next(100) < 60 ? hoy.AddDays(-random.Next(1, 45)) : (DateTime?)null;

            clientes.Add(new Cliente
            {
                Nombre = nombre,
                Apellidos = apellido,
                Edad = random.Next(18, 65),
                Peso = Math.Round((decimal)(random.NextDouble() * 40 + 50), 1),
                Telefono = telefono,
                FechaAlta = fechaAlta,
                FechaVencimiento = fechaVencimiento,
                FechaUltimoPago = fechaUltimoPago,
                Activo = true,
                Estado = "Próximo a vencer"
            });
        }

        // Clientes vencidos (fecha vencimiento pasada)
        for (int i = 0; i < 8; i++)
        {
            var nombre = nombres[random.Next(nombres.Length)];
            var apellido = apellidos[random.Next(apellidos.Length)];
            var telefono = GenerarTelefonoUnico(telefonosUsados, random);
            
            var fechaAlta = hoy.AddMonths(-random.Next(2, 15));
            var diasVencidos = random.Next(1, 90); // Vencidos entre 1 y 90 días
            var fechaVencimiento = hoy.AddDays(-diasVencidos);
            var fechaUltimoPago = random.Next(100) < 40 ? fechaVencimiento.AddDays(-random.Next(1, 30)) : (DateTime?)null;

            clientes.Add(new Cliente
            {
                Nombre = nombre,
                Apellidos = apellido,
                Edad = random.Next(18, 65),
                Peso = Math.Round((decimal)(random.NextDouble() * 40 + 50), 1),
                Telefono = telefono,
                FechaAlta = fechaAlta,
                FechaVencimiento = fechaVencimiento,
                FechaUltimoPago = fechaUltimoPago,
                Activo = false,
                Estado = "Vencido"
            });
        }

        return clientes;
    }

    private static string GenerarTelefonoUnico(HashSet<string> telefonosUsados, Random random)
    {
        string telefono;
        do
        {
            telefono = $"6{random.Next(10000000, 99999999)}";
        } while (telefonosUsados.Contains(telefono));
        
        telefonosUsados.Add(telefono);
        return telefono;
    }

    private static List<Pago> CrearPagosFicticios(List<int> clienteIds)
    {
        var pagos = new List<Pago>();
        var hoy = DateTime.Today;
        var random = new Random(42); // Misma semilla para consistencia

        // Pagos del mes actual
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
        var clientesParaPagoMesActual = clienteIds.Take(12).ToList();
        
        foreach (var clienteId in clientesParaPagoMesActual)
        {
            var fechaPago = inicioMes.AddDays(random.Next(0, hoy.Day));
            var cantidad = Math.Round((decimal)(random.NextDouble() * 2000 + 3000), 2); // Entre 3000 y 5000

            pagos.Add(new Pago
            {
                ClienteId = clienteId,
                FechaPago = fechaPago,
                Cantidad = cantidad
            });
        }

        // Pagos del mes anterior
        var mesAnterior = hoy.AddMonths(-1);
        var inicioMesAnterior = new DateTime(mesAnterior.Year, mesAnterior.Month, 1);
        var finMesAnterior = inicioMesAnterior.AddMonths(1).AddDays(-1);
        var clientesParaPagoMesAnterior = clienteIds.Take(18).ToList();

        foreach (var clienteId in clientesParaPagoMesAnterior)
        {
            var fechaPago = inicioMesAnterior.AddDays(random.Next(0, finMesAnterior.Day));
            var cantidad = Math.Round((decimal)(random.NextDouble() * 2000 + 3000), 2);

            pagos.Add(new Pago
            {
                ClienteId = clienteId,
                FechaPago = fechaPago,
                Cantidad = cantidad
            });
        }

        // Pagos de hace 2 meses
        var hace2Meses = hoy.AddMonths(-2);
        var inicioHace2Meses = new DateTime(hace2Meses.Year, hace2Meses.Month, 1);
        var finHace2Meses = inicioHace2Meses.AddMonths(1).AddDays(-1);
        var clientesParaPagoHace2Meses = clienteIds.Take(15).ToList();

        foreach (var clienteId in clientesParaPagoHace2Meses)
        {
            var fechaPago = inicioHace2Meses.AddDays(random.Next(0, finHace2Meses.Day));
            var cantidad = Math.Round((decimal)(random.NextDouble() * 2000 + 3000), 2);

            pagos.Add(new Pago
            {
                ClienteId = clienteId,
                FechaPago = fechaPago,
                Cantidad = cantidad
            });
        }

        // Pagos de hace 3 meses
        var hace3Meses = hoy.AddMonths(-3);
        var inicioHace3Meses = new DateTime(hace3Meses.Year, hace3Meses.Month, 1);
        var finHace3Meses = inicioHace3Meses.AddMonths(1).AddDays(-1);
        var clientesParaPagoHace3Meses = clienteIds.Take(10).ToList();

        foreach (var clienteId in clientesParaPagoHace3Meses)
        {
            var fechaPago = inicioHace3Meses.AddDays(random.Next(0, finHace3Meses.Day));
            var cantidad = Math.Round((decimal)(random.NextDouble() * 2000 + 3000), 2);

            pagos.Add(new Pago
            {
                ClienteId = clienteId,
                FechaPago = fechaPago,
                Cantidad = cantidad
            });
        }

        // Algunos clientes con múltiples pagos (historial)
        var clientesConHistorial = clienteIds.Take(8).ToList();
        foreach (var clienteId in clientesConHistorial)
        {
            // Agregar pagos adicionales en los últimos 6 meses
            for (int mes = 1; mes <= 6; mes++)
            {
                if (random.Next(100) < 70) // 70% de probabilidad de tener pago en ese mes
                {
                    var fechaMes = hoy.AddMonths(-mes);
                    var inicioMesPago = new DateTime(fechaMes.Year, fechaMes.Month, 1);
                    var finMesPago = inicioMesPago.AddMonths(1).AddDays(-1);
                    var fechaPago = inicioMesPago.AddDays(random.Next(0, Math.Min(finMesPago.Day, hoy.Day)));
                    var cantidad = Math.Round((decimal)(random.NextDouble() * 2000 + 3000), 2);

                    pagos.Add(new Pago
                    {
                        ClienteId = clienteId,
                        FechaPago = fechaPago,
                        Cantidad = cantidad
                    });
                }
            }
        }

        return pagos;
    }
}
