using EventCentric.Database;
using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Repository;
using EventCentric.Repository.Mapping;
using EventCentric.Serialization;
using EventCentric.Tests.EventSourcing.Helpers;
using EventCentric.Utils;
using System;

namespace EventCentric.Tests.EventSourcing
{
    public class GIVEN_store : TestConfig, IDisposable
    {
        protected string appStreamType;
        protected string streamType;
        protected SqlClientLite sql;
        protected EventStore<InventoryTestAggregate> sut;
        protected Func<bool, IEventStoreDbContext> contextFactory;

        public GIVEN_store()
        {
            this.streamType = NodeNameResolver.ResolveNameOf<InventoryTestAggregate>();
            this.appStreamType = $"App_{this.streamType}";
            this.sql = new SqlClientLite(defaultConnectionString);
            this.contextFactory = (isReadOnly) => new EventStoreDbContext(isReadOnly, defaultConnectionString);
            this.CreateFreshInstanceOfEventStore();

            this.sql.DropDatabase();

            using (var context = this.contextFactory.Invoke(false))
            {
                ((EventStoreDbContext)context).Database.Create();

                var now = DateTime.Now;

                context.Subscriptions.Add(new SubscriptionEntity
                {
                    StreamType = this.appStreamType,
                    Url = "self",
                    Token = string.Empty,
                    ProcessorBufferVersion = 0,
                    IsPoisoned = false,
                    WasCanceled = false,
                    CreationLocalTime = now,
                    UpdateLocalTime = now
                });

                context.SaveChanges();
            }
        }

        protected void CreateFreshInstanceOfEventStore()
        {
            this.sut = new EventStore<InventoryTestAggregate>(
                NodeNameResolver.ResolveNameOf<InventoryTestAggregate>(),
                new JsonTextSerializer(),
                this.contextFactory,
                new UtcTimeProvider(),
                new SequentialGuid(),
                new ConsoleLogger());
        }

        public void Dispose()
        => this.sql.DropDatabase();
        //{ }
    }
}
