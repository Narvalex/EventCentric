using EventCentric.EventSourcing;
using EventCentric.Messaging;
using System;
using System.Collections.Generic;

namespace EventCentric.Utils.Testing
{
    public class ApplicationServiceTestHelper<TApp> where TApp : ApplicationService
    {
        private EventBusStub eventBus;

        public ApplicationServiceTestHelper()
        {
            this.Time = new UtcTimeProvider();
            this.Guid = new SequentialGuid();
            this.eventBus = new EventBusStub();
        }

        public void Setup(TApp app)
            => this.App = app;

        public IUtcTimeProvider Time { get; }

        public IGuidProvider Guid { get; }

        public IEventBus EventBus => this.eventBus;

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

        public class EventBusStub : IEventBus
        {
            public void Publish(Guid transactionId, Guid streamId, IEvent @event)
            {
                ((Event)@event).TransactionId = transactionId;
                ((Event)@event).StreamId = streamId;
                this.PublishedEvents.Add(@event);
            }

            public List<IEvent> PublishedEvents { get; } = new List<IEvent>();
        }
    }
}
