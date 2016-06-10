using EventCentric.MicroserviceFactory;
using System;

namespace PersistenceBenchmark.ConsoleHost
{
    public class Program
    {
        public static bool VerboseIsEnabled = false;

        static void Main(string[] args)
        {
            var plugin = DbManager.SetPlugin(PersistencePlugin.InMemory);

            PrintWelcomeMessage(plugin);

            DbManager.ResetDbs(plugin);
            UnityConfig.InitializeMainContainer(plugin);

            // Holding in memory messages
            // Expected: Events: waves * users * 2;

            Console.WriteLine("Press enter to start....");
            Console.ReadLine();

            RunBenchmark(plugin);

            DbManager.DropDb(plugin);
        }

        private static void RunBenchmark(PersistencePlugin plugin)
        {
            var user1App = EventSystem.ResolveProcessor<UserManagement>("user1") as UserManagementHandler;
            var user2App = EventSystem.ResolveProcessor<UserManagement>("user2") as UserManagementHandler;

            // SQL SERVER ADO.NET --------------------------------------------
            // 100 througput,   completes in 20 s   7.498 messages      374 m/s
            // 100 througput,   completes in 40 s   12.650  messages    316 m/s
            // 100 througput,   completes in 15:00  70.066  messages    77 m/s
            if (plugin == PersistencePlugin.SqlServer)
            {
                user1App.StressWithWavesOfConcurrentUsers(wavesCount: 25, concurrentUsers: 1000);
                user2App.StressWithWavesOfConcurrentUsers(wavesCount: 25, concurrentUsers: 1000);

                //user1App.StressWithWavesOfConcurrentUsers(wavesCount: 1, concurrentUsers: 1);
            }

            // IN-MEMORY-------------------------------------------------------
            // 100 througput,   completes in 0:20 s 18.225 messgaes    911 m/s
            if (plugin == PersistencePlugin.InMemory)
            {
                user1App.StressWithWavesOfConcurrentUsers(wavesCount: 5, concurrentUsers: 1000);
                user2App.StressWithWavesOfConcurrentUsers(wavesCount: 5, concurrentUsers: 1000);

                //user1App.StressWithWavesOfConcurrentUsers(wavesCount: 1, concurrentUsers: 1);
            }

            if (plugin == PersistencePlugin.InMemory)
            {
                Console.WriteLine("");
                Console.WriteLine("Press enter to see results...");
                Console.ReadLine();
                UnityConfig.StatsMonitor.PrintStats();
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void PrintWelcomeMessage(PersistencePlugin plugin)
        {
            string pluginSelectedName = "undefined";
            switch (plugin)
            {
                case PersistencePlugin.InMemory:
                    pluginSelectedName = "In-Memory";
                    break;
                case PersistencePlugin.SqlServer:
                    pluginSelectedName = "Sql Server";
                    break;
                case PersistencePlugin.SqlServerCe:
                    pluginSelectedName = "Sql Server Compact Edition";
                    break;
            }

            Console.WriteLine($"Welcome to persistence bench for {pluginSelectedName}");
        }
    }
}
