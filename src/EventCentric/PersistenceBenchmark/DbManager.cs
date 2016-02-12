using EventCentric;
using EventCentric.Config;
using EventCentric.Database;
using EventCentric.Repository;
using EventCentric.Repository.Mapping;
using System;

namespace PersistenceBenchmark
{
    public static class DbManager
    {
        private static string connectionString;

        public static void CreateDb()
        {
            Console.WriteLine("Getting db connection from config.");
            connectionString = EventStoreConfig.GetConfig().ConnectionString;

            Console.WriteLine("Reseting database");
            DropDb();

            using (var context = new EventStoreDbContext(connectionString))
            {
                context.Database.Create();

                context.Subscriptions.Add(new SubscriptionEntity
                {
                    StreamType = EventSourceNameResolver.ResolveNameOf<AppService>(),
                    Url = "self",
                    Token = "#token",
                    ProcessorBufferVersion = 0,
                    IsPoisoned = false,
                    WasCanceled = false,
                    CreationLocalTime = DateTime.Now,
                    UpdateLocalTime = DateTime.Now
                });

                context.SaveChanges();
            }

            Console.WriteLine("Database reset");
        }

        public static void DropDb()
        {
            new SqlClientLite(connectionString).DropDatabase();
        }
    }
}
