using GestorClientes.Data;
using GestorClientes.Models;

namespace GestorClientes.Services;

public class MetricasService
{
    private readonly ClienteRepository _clienteRepository;
    private readonly PagoRepository _pagoRepository;

    public MetricasService()
    {
        _clienteRepository = new ClienteRepository();
        _pagoRepository = new PagoRepository();
    }

    public int GetTotalClientes()
    {
        return _clienteRepository.GetAll().Count;
    }

    public int GetClientesActivos()
    {
        var clientes = _clienteRepository.GetAll();
        var hoy = DateTime.Today;
        return clientes.Count(c => c.FechaVencimiento >= hoy);
    }

    public int GetClientesVencidos()
    {
        var clientes = _clienteRepository.GetAll();
        var hoy = DateTime.Today;
        return clientes.Count(c => c.FechaVencimiento < hoy);
    }

    public decimal GetIngresosDelMes()
    {
        var hoy = DateTime.Today;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
        var finMes = inicioMes.AddMonths(1).AddDays(-1);

        var pagos = _pagoRepository.GetByFechaRange(inicioMes, finMes);
        return pagos.Sum(p => p.Cantidad);
    }

    public decimal GetIngresosEsperados()
    {
        var clientesActivos = GetClientesActivos();
        if (clientesActivos == 0)
            return 0;

        // Calcular cuota mensual promedio basada en pagos recientes
        var hoy = DateTime.Today;
        var ultimos3Meses = hoy.AddMonths(-3);
        var pagos = _pagoRepository.GetByFechaRange(ultimos3Meses, hoy);
        
        if (pagos.Count > 0)
        {
            var promedio = pagos.Average(p => p.Cantidad);
            return clientesActivos * promedio;
        }
        
        // Si no hay pagos recientes, usar un promedio estimado
        return clientesActivos * 5000; // Valor estimado por defecto
    }

    public Dictionary<string, decimal> GetIngresosPorUltimosMeses(int meses)
    {
        var ingresos = new Dictionary<string, decimal>();
        var hoy = DateTime.Today;

        for (int i = meses - 1; i >= 0; i--)
        {
            var fecha = hoy.AddMonths(-i);
            var inicioMes = new DateTime(fecha.Year, fecha.Month, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);

            var pagos = _pagoRepository.GetByFechaRange(inicioMes, finMes);
            var total = pagos.Sum(p => p.Cantidad);
            
            ingresos.Add(fecha.ToString("MMM yyyy"), total);
        }

        return ingresos;
    }
}

