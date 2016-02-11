using EventCentric.Config;
using EventCentric.Database;
using EventCentric.Repository;

namespace PersistenceBenchmark.Web
{
    public static class DbManager
    {
        private static string connectionString;

        public static void CreateDb()
        {
            connectionString = EventStoreConfig.GetConfig().ConnectionString;

            DropDb();

            using (var context = new EventStoreDbContext(connectionString))
            {
                context.Database.Create();
            }
        }

        public static void DropDb()
        {
            new SqlClientLite(connectionString).DropDatabase();
        }
    }
}
