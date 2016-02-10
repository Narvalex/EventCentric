using EventCentric.EventSourcing;
using EventCentric.Messaging;
using System;
using System.Collections.Generic;

namespace EventCentric.Utils.Testing
{
    public class ApplicationServiceTestHelper<TApp> where TApp : ApplicationService
    {
        private EventBusStub eventBus;

        public ApplicationServiceTestHelper(Func<IServiceBus, IGuidProvider, TApp> appFactory = null)
        {
            this.Guid = new SequentialGuid();
            this.eventBus = new EventBusStub();

            if (appFactory == null)
            {
                var appConstructor = typeof(TApp).GetConstructor(new[] { typeof(IServiceBus), typeof(IGuidProvider) });
                Ensure.CastIsValid(appConstructor, "Type TApp must have a constructor with the following signature: .ctor(IEventBus, IGuidProvider)");
                this.App = (TApp)appConstructor.Invoke(new object[] { this.eventBus, this.Guid });
            }
            else
                this.App = appFactory.Invoke(this.eventBus, this.Guid);
        }

        public IGuidProvider Guid { get; }

        public IServiceBus EventBus => this.eventBus;

        public TApp App { get; private set; }

        public ApplicationServiceTestHelper<TApp> When(Action<TApp> action)
        {
            action.Invoke(this.App);
            return this;
        }

        public ApplicationServiceTestHelper<TApp> Then(Action<IEnumerable<IEvent>> action)
        {
            action.Invoke(this.PublishedEvents);
            return this;
        }

        public List<IEvent> PublishedEvents => this.eventBus.PublishedEvents;

        public class EventBusStub : IServiceBus
        {
            public void Send(Guid transactionId, Guid streamId, Message @event)
            {
                ((Message)@event).TransactionId = transactionId;
                ((Message)@event).StreamId = streamId;
                this.PublishedEvents.Add(@event);
            }

            public List<IEvent> PublishedEvents { get; } = new List<IEvent>();
        }
    }
}
