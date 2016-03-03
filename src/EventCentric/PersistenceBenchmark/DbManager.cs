using EventCentric.Database;
using EventCentric.Repository;
using EventCentric.Repository.Mapping;
using System;
using System.Collections.Generic;

namespace PersistenceBenchmark
{
    public static class DbManager
    {
        static DbManager()
        {
            connectionStrings = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("promo", PromoCs),
                new KeyValuePair<string, string>("user1", User1Cs),
                new KeyValuePair<string, string>("user2", User2Cs)
            };
        }

        private static List<KeyValuePair<string, string>> connectionStrings;

        public const string PromoCs = "server = (local); Database = PromotionsDb; User Id = sa; pwd = 123456";
        public const string User1Cs = "server = (local); Database = User1; User Id = sa; pwd = 123456";
        public const string User2Cs = "server = (local); Database = User2; User Id = sa; pwd = 123456";

        public static void CreateDbs()
        {
            DropDbs();

            connectionStrings.ForEach(x =>
            {
                Console.WriteLine($"Creating database for {x.Key}...");
                CreateUserDb(x);
                Console.WriteLine($"Database for {x.Key} was created successfully!");
            });
        }

        public static void DropDbs()
        {
            Console.WriteLine("Reseting all databases");
            connectionStrings.ForEach(x =>
            {
                Console.WriteLine($"Dropping db for {x.Key}...");
                SqlClientLite.DropDatabase(x.Value);
                Console.WriteLine($"Db for {x.Key} was dropped successfully!");
            });
            Console.WriteLine("All databases are cleansed");
        }

        private static void CreateUserDb(KeyValuePair<string, string> csByStreamName)
        {
            using (var context = new EventStoreDbContext(csByStreamName.Value))
            {
                context.Database.Create();

                // user app
                if (csByStreamName.Key.ToUpper().StartsWith("U"))
                {
                    context.Subscriptions.Add(new SubscriptionEntity
                    {
                        StreamType = csByStreamName.Key,
                        Url = "none",
                        Token = "#token",
                        ProcessorBufferVersion = 0,
                        IsPoisoned = false,
                        WasCanceled = false,
                        CreationLocalTime = DateTime.Now,
                        UpdateLocalTime = DateTime.Now
                    });

                    context.Subscriptions.Add(new SubscriptionEntity
                    {
                        StreamType = $"{csByStreamName.Key}_app",
                        Url = "none",
                        Token = "#token",
                        ProcessorBufferVersion = 0,
                        IsPoisoned = false,
                        WasCanceled = false,
                        CreationLocalTime = DateTime.Now,
                        UpdateLocalTime = DateTime.Now
                    });
                }
                else
                {
                    // promotions db
                    connectionStrings.ForEach(c =>
                    {
                        context.Subscriptions.Add(new SubscriptionEntity
                        {
                            StreamType = c.Key,
                            Url = "none",
                            Token = "#token",
                            ProcessorBufferVersion = 0,
                            IsPoisoned = false,
                            WasCanceled = false,
                            CreationLocalTime = DateTime.Now,
                            UpdateLocalTime = DateTime.Now
                        });
                    });
                }

                context.SaveChanges();
            }
        }
    }
}
