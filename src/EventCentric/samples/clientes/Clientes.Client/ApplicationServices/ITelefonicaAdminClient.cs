using System;

namespace Clientes.Client
{
    public interface ITelefonicaAdminClient
    {
        void ClienteAbonoPorSaldo(Guid idCliente, int montoAbonado);
        void ClienteGastoSaldo(Guid idCliente, int saldoGastado);
        void SolicitudNuevoClienteRecibida(string nombre);
    }
}