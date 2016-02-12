using EventCentric;
using EventCentric.Config;
using EventCentric.Database;
using EventCentric.Repository;
using EventCentric.Repository.Mapping;
using System;
using System.Collections.Generic;

namespace PersistenceBenchmark
{
    public static class DbManager
    {
        private static List<string> connectionStrings = new List<string>();

        public static void CreateDbs(string promotionsDbConnection)
        {
            Console.WriteLine("Getting db connection from config.");
            connectionStrings.Add(EventStoreConfig.GetConfig().ConnectionString);
            connectionStrings.Add(promotionsDbConnection);

            Console.WriteLine("Reseting database");
            DropDb();

            CreateUserDb();
            CreatePromotionsDb(promotionsDbConnection);

            Console.WriteLine("Database reset");
        }

        private static void CreateUserDb()
        {
            using (var context = new EventStoreDbContext(EventStoreConfig.GetConfig().ConnectionString))
            {
                context.Database.Create();

                context.Subscriptions.Add(new SubscriptionEntity
                {
                    StreamType = EventSourceNameResolver.ResolveNameOf<UserAppService>(),
                    Url = "none",
                    Token = "#token",
                    ProcessorBufferVersion = 0,
                    IsPoisoned = false,
                    WasCanceled = false,
                    CreationLocalTime = DateTime.Now,
                    UpdateLocalTime = DateTime.Now
                });

                context.SaveChanges();
            }
        }

        private static void CreatePromotionsDb(string connectionString)
        {
            using (var context = new EventStoreDbContext(connectionString))
            {
                context.Database.Create();

                context.Subscriptions.Add(new SubscriptionEntity
                {
                    StreamType = EventSourceNameResolver.ResolveNameOf<UserManagement>(),
                    Url = "none",
                    Token = "#token",
                    ProcessorBufferVersion = 0,
                    IsPoisoned = false,
                    WasCanceled = false,
                    CreationLocalTime = DateTime.Now,
                    UpdateLocalTime = DateTime.Now
                });

                context.SaveChanges();
            }
        }

        public static void DropDb()
        {
            connectionStrings.ForEach(c => SqlClientLite.DropDatabase(c));
        }
    }
}
