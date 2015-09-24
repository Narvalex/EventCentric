using EventCentric.Database;
using EventCentric.EventSourcing;
using EventCentric.Repository;
using EventCentric.Respository;
using EventCentric.Serialization;
using EventCentric.Tests.EventSourcing.Helpers;
using EventCentric.Tests.Helpers;
using EventCentric.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;

namespace EventCentric.Tests.EventSourcing.EventStoreFixture
{
    [TestClass]
    public class GIVEN_event_store : IDisposable
    {
        protected string connectionString;
        protected SqlClientLite sql;
        protected Func<bool, IEventStoreDbContext> contextFactory;
        protected EventStore<CartTestAggregate> sut;

        public GIVEN_event_store()
        {
            this.connectionString = ConfigurationManager.AppSettings["defaultConnection"];
            this.sql = new SqlClientLite(this.connectionString);

            EventCentricDbInitializer.CreateSagaDbObjects(this.connectionString);

            this.contextFactory = isReadOnly => new EventStoreDbContext(isReadOnly, this.connectionString);
            this.sut = new EventStore<CartTestAggregate>(new JsonTextSerializer(), this.contextFactory, new LocalTimeProvider(), new SequentialGuid(), new ConsoleLogger(), false);
        }

        [TestMethod]
        public void WHEN_handling_message_THEN_can_append_to_store()
        {
            var cartId = Guid.NewGuid();
            var createCartCommand = new CreateCart(cartId, Guid.Empty, Guid.NewGuid(), cartId);
            var aggregate = new CartTestAggregate(cartId);
            aggregate.Handle(createCartCommand);

            this.sut.Save(aggregate, createCartCommand);
        }

        [TestMethod]
        public void WHEN_handling_multiple_messages_THEN_can_append_and_rehydrate_from_message_to_message()
        {
            var cartId = Guid.NewGuid();
            var createCartCommand = new CreateCart(cartId, Guid.Empty, Guid.NewGuid(), cartId);
            var aggregate = new CartTestAggregate(cartId);
            aggregate.Handle(createCartCommand);

            this.sut.Save(aggregate, createCartCommand);

            // Adding 10 items
            aggregate = this.sut.Get(cartId);
            var addItemsCommand = new AddItems(cartId, Guid.Empty, Guid.NewGuid(), cartId, 10);
            aggregate.Handle(addItemsCommand);
            this.sut.Save(aggregate, addItemsCommand);

            // Removing 5 items
            aggregate = this.sut.Get(cartId);
            var removeItemsCommand = new RemoveItems(cartId, Guid.Empty, Guid.NewGuid(), cartId, 5);
            aggregate.Handle(removeItemsCommand);
            this.sut.Save(aggregate, removeItemsCommand);

            // Total items should be: 5 items (check db and debugger);
        }


        public void Dispose()
        {
            this.sql.DropDatabase();
        }
    }
}
