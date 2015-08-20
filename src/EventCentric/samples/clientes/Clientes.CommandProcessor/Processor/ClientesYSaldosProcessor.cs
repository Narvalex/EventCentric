using Clientes.Commands;
using EventCentric.EventSourcing;
using EventCentric.Messaging;
using EventCentric.Processing;

namespace Clientes.CommandProcessor.Processor
{
    public class ClientesYSaldosProcessor : EventProcessor<ClientesYSaldos>,
        ICommandHandler<RegistrarNuevoCliente>,
        ICommandHandler<AgregarSaldo>,
        ICommandHandler<QuitarSaldo>
    {
        public ClientesYSaldosProcessor(IBus bus, IEventStore<ClientesYSaldos> store, ISubscriptionWriter subsriptionWriter)
            : base(bus, store, subsriptionWriter)
        { }

        public void Handle(RegistrarNuevoCliente command)
        {
            base.CreateNewStream(command.EventId, command);
        }

        public void Handle(QuitarSaldo command)
        {
            base.Handle(command.IdCliente, command);
        }

        public void Handle(AgregarSaldo command)
        {
            base.Handle(command.IdCliente, command);
        }
    }
}
