using EventCentric.Database;
using EventCentric.MicroserviceFactory;
using EventCentric.Persistence;
using EventCentric.Persistence.SqlServer;
using System;
using System.Collections.Generic;

namespace PersistenceBenchmark
{
    public static class DbManager
    {
        private static PersistencePlugin selected = PersistencePlugin.InMemory;

        public static PersistencePlugin SelectedPlugin => selected;

        public static PersistencePlugin SetPlugin(PersistencePlugin plugin)
        {
            selected = plugin;
            return selected;
        }

        public static string FixedConnectionstring
        {
            get
            {
                switch (SelectedPlugin)
                {
                    case PersistencePlugin.SqlServer:
                        return "server = (local); Database = PersistenceBench; User Id = sa; pwd = 123456";
                    case PersistencePlugin.SqlServerCe:
                        return $"Data Source=|DataDirectory|PersistenceBenchCe.sdf; default lock timeout = 600000";
                    default:
                        return string.Empty;
                };
            }
        }

        public static void ResetDbs(PersistencePlugin plugin)
        {
            if (plugin == PersistencePlugin.InMemory)
                return;

            using (var context = new EventStoreDbContext(FixedConnectionstring))
            {
                if (context.Database.Exists())
                {
                    Console.WriteLine("Truncating current db...");
                    switch (SelectedPlugin)
                    {
                        case PersistencePlugin.InMemory:
                            break;
                        case PersistencePlugin.SqlServer:
                            context.Database.ExecuteSqlCommand
                            (@"
                                truncate table eventstore.events;
                                truncate table eventstore.inbox;
                                truncate table eventstore.snapshots;
                                truncate table eventstore.subscriptions;"
                            );
                            break;

                        case PersistencePlugin.SqlServerCe:
                            context.Events.ForEach(e => context.Events.Remove(e));
                            context.Inbox.ForEach(e => context.Inbox.Remove(e));
                            context.Subscriptions.ForEach(e => context.Subscriptions.Remove(e));
                            context.Snapshots.ForEach(e => context.Snapshots.Remove(e));
                            break;
                    }

                    Console.WriteLine("Db truncated!");
                    Console.WriteLine("Adding subscriptons");
                    AddSubscriptions(context);

                    return;
                }
            }

            DropDb(plugin);
            CreateDb();
        }

        public static void DropDb(PersistencePlugin plugin)
        {
            if (plugin != PersistencePlugin.SqlServer)
                return;
            Console.WriteLine("Drop db started...");

            SqlClientLite.DropDatabase(FixedConnectionstring);

            Console.WriteLine("Db was droped!");
        }

        private static void CreateDb()
        {
            Console.WriteLine("Creating Db...");
            using (var context = new EventStoreDbContext(FixedConnectionstring))
            {
                context.Database.Create();

                Console.WriteLine("Db created");
                Console.WriteLine("Adding subscriptions...");
                // promo
                AddSubscriptions(context);
            }
        }

        public static List<SubscriptionEntity> GetSubscriptions()
        {
            var subs = new List<SubscriptionEntity>();
            subs.Add(new SubscriptionEntity
            {
                SubscriberStreamType = "promo",
                StreamType = "promo",
                Url = "none",
                Token = "#token",
                ProcessorBufferVersion = 0,
                IsPoisoned = false,
                WasCanceled = false,
                CreationLocalTime = DateTime.Now,
                UpdateLocalTime = DateTime.Now
            });
            subs.Add(new SubscriptionEntity
            {
                SubscriberStreamType = "promo",
                StreamType = "user1",
                Url = "none",
                Token = "#token",
                ProcessorBufferVersion = 0,
                IsPoisoned = false,
                WasCanceled = false,
                CreationLocalTime = DateTime.Now,
                UpdateLocalTime = DateTime.Now
            });

            // USER1 will add a on the fly subscription

            subs.Add(new SubscriptionEntity
            {
                SubscriberStreamType = "promo",
                StreamType = "user2",
                Url = "none",
                Token = "#token",
                ProcessorBufferVersion = 0,
                IsPoisoned = false,
                WasCanceled = false,
                CreationLocalTime = DateTime.Now,
                UpdateLocalTime = DateTime.Now
            });

            // user1
            subs.Add(new SubscriptionEntity
            {
                SubscriberStreamType = "user1",
                StreamType = "user1",
                Url = "none",
                Token = "#token",
                ProcessorBufferVersion = 0,
                IsPoisoned = false,
                WasCanceled = false,
                CreationLocalTime = DateTime.Now,
                UpdateLocalTime = DateTime.Now
            });
            subs.Add(new SubscriptionEntity
            {
                SubscriberStreamType = "user1",
                StreamType = "user2",
                Url = "none",
                Token = "#token",
                ProcessorBufferVersion = 0,
                IsPoisoned = false,
                WasCanceled = false,
                CreationLocalTime = DateTime.Now,
                UpdateLocalTime = DateTime.Now
            });
            subs.Add(new SubscriptionEntity
            {
                SubscriberStreamType = "user1",
                StreamType = "promo",
                Url = "none",
                Token = "#token",
                ProcessorBufferVersion = 0,
                IsPoisoned = false,
                WasCanceled = false,
                CreationLocalTime = DateTime.Now,
                UpdateLocalTime = DateTime.Now
            });

            // user2
            subs.Add(new SubscriptionEntity
            {
                SubscriberStreamType = "user2",
                StreamType = "promo",
                Url = "none",
                Token = "#token",
                ProcessorBufferVersion = 0,
                IsPoisoned = false,
                WasCanceled = false,
                CreationLocalTime = DateTime.Now,
                UpdateLocalTime = DateTime.Now
            });
            subs.Add(new SubscriptionEntity
            {
                SubscriberStreamType = "user2",
                StreamType = "user1",
                Url = "none",
                Token = "#token",
                ProcessorBufferVersion = 0,
                IsPoisoned = false,
                WasCanceled = false,
                CreationLocalTime = DateTime.Now,
                UpdateLocalTime = DateTime.Now
            });
            subs.Add(new SubscriptionEntity
            {
                SubscriberStreamType = "user2",
                StreamType = "user2",
                Url = "none",
                Token = "#token",
                ProcessorBufferVersion = 0,
                IsPoisoned = false,
                WasCanceled = false,
                CreationLocalTime = DateTime.Now,
                UpdateLocalTime = DateTime.Now
            });
            return subs;
        }

        private static void AddSubscriptions(EventStoreDbContext context)
        {
            GetSubscriptions().ForEach(s => context.Subscriptions.Add(s));
            context.SaveChanges();

            Console.WriteLine("Susbscriptions added!");
        }
    }
}
