using EventCentric.EventSourcing;
using EventCentric.Messaging.Events;
using EventCentric.Processing;

namespace EventCentric.Messaging
{
    public class ClientBus : IClientBus
    {
        private readonly IBus bus;

        public ClientBus(IBus bus)
        {
            this.bus = bus;
        }

        public void Publish(IEvent @event)
        {
            this.PublishInBus(@event);
        }

        public void Send(ICommand command)
        {
            this.PublishInBus(command);
        }

        private void PublishInBus(IEvent @event)
        {
            this.bus.Publish(new NewIncomingEvent(@event));
        }
    }
}
