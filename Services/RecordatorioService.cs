using GestorClientes.Data;
using GestorClientes.Models;

namespace GestorClientes.Services;

public class RecordatorioService
{
    private readonly ClienteRepository _clienteRepository;

    public RecordatorioService()
    {
        _clienteRepository = new ClienteRepository();
    }

    public List<Cliente> GetClientesVencidos()
    {
        var clientes = _clienteRepository.GetAll();
        var hoy = DateTime.Today;
        return clientes.Where(c => c.FechaVencimiento < hoy).ToList();
    }

    public List<Cliente> GetClientesProximosAVencer(int dias = 7)
    {
        var clientes = _clienteRepository.GetAll();
        var hoy = DateTime.Today;
        var fechaLimite = hoy.AddDays(dias);
        return clientes.Where(c => c.FechaVencimiento >= hoy && c.FechaVencimiento <= fechaLimite).ToList();
    }

    public int GetDiasHastaVencimiento(Cliente cliente)
    {
        return (cliente.FechaVencimiento.Date - DateTime.Today).Days;
    }
}

