using EventCentric.Database;
using EventCentric.Repository;
using EventCentric.Repository.Mapping;
using EventCentric.Respository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Linq;

namespace EventCentric.Tests.Playground
{
    [TestClass]
    public class AreOrUpdateFixture : IDisposable
    {
        protected string connectionString;
        protected Func<EventStoreDbContext> contextFactory;

        public AreOrUpdateFixture()
        {
            this.connectionString = ConfigurationManager.AppSettings["defaultConnection"];
            DbInitializer.CreateDatabaseObjects(connectionString, true);
            this.contextFactory = () => new EventStoreDbContext(this.connectionString);
        }

        public void Dispose()
        {
            SqlClientLite.DropDatabase(this.connectionString);
        }

        [TestMethod]
        public void WHEN_table_is_empty_THEN_can_add_or_update_new_item()
        {
            using (var context = contextFactory())
            {
                context.AddOrUpdate(
                    () => context.Subscriptions.Where(s => s.StreamType == "TestAggregate").SingleOrDefault(),
                    () => new SubscriptionEntity
                    {
                        CreationDate = DateTime.Now,
                        IsPoisoned = false,
                        StreamType = "TestAggregate"
                    },
                    s =>
                    {
                    });

                context.SaveChanges();
            }
        }

        [TestMethod]
        public void WHEN_table_has_item_THEN_can_update()
        {
            this.WHEN_table_is_empty_THEN_can_add_or_update_new_item();
            this.WHEN_table_is_empty_THEN_can_add_or_update_new_item();
        }

    }
}
