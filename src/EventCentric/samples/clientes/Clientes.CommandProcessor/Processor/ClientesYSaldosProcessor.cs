using Clientes.CommandProcessor.Commands;
using Clientes.Events;
using EventCentric.EventSourcing;
using EventCentric.Messaging;
using EventCentric.Processing;
using EventCentric.Utils;

namespace Clientes.CommandProcessor.Processor
{
    public class ClientesYSaldosProcessor : EventProcessor<ClientesYSaldos>,
        ICommandHandler<AgregarSaldo>,
        ICommandHandler<AgregarCliente>,
        ICommandHandler<QuitarSaldo>,
        IEventHandler<ClienteRegistrado>
    {
        public ClientesYSaldosProcessor(IGuidProvider guidProvider, IBus bus, IEventStore<ClientesYSaldos> store, ISubscriptionWriter inboxWriter)
            : base(bus, store, inboxWriter)
        { }

        public void Handle(AgregarCliente command)
        {
            base.CreateNewStream(command);
        }

        public void Handle(ClienteRegistrado @event)
        {
            base.HandleFirstEvent(@event.IdCliente, @event);
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
