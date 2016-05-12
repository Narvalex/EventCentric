﻿using EventCentric.MicroserviceFactory;
using Microsoft.Practices.Unity;
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
            var mainContainer = UnityConfig.GetConfiguredContainer(plugin);

            var user1App = UnityConfig.UserContainer1.Resolve<UserAppService>();
            var user2App = UnityConfig.UserContainer2.Resolve<UserAppService>();

            // Holding in memory messages
            // Expected: Events: waves * users * 2;

            Console.WriteLine("Press enter to start....");
            Console.ReadLine();

            // THIS MAKE CRASH, ALMOST

            // SQL SERVER ENTITY FRAMEWORK -----------------------------------
            // 50 througput,    completes in 2:48 m 10.000 messages    60 m/s
            // 100 througput,   completes in 4:44 m 20.000 messages    70 m/s

            // SQL SERVER ADO.NET ---------------------------------------------
            // 100 througput,   completes in 1:48 m 20.000 messages    185 m/s
            // SQL SERVER ADO.NET SSD -----------------------------------------
            // 200 througput,   completes in 38 s 10.000 messages    263 m/s
            //user1App.StressWithWavesOfConcurrentUsers(wavesCount: 5, concurrentUsers: 500);
            //user2App.StressWithWavesOfConcurrentUsers(wavesCount: 5, concurrentUsers: 500);

            // IN-MEMORY-------------------------------------------------------
            // 100 througput,   completes in 0:20 s 16.233 messgaes    811 m/s
            user1App.StressWithWavesOfConcurrentUsers(wavesCount: 5, concurrentUsers: 1000);
            user2App.StressWithWavesOfConcurrentUsers(wavesCount: 5, concurrentUsers: 1000);

            // SUPER TEST
            // 100 througput, completes in 07:08 s 40.000 messages     93 m/s
            //user1App.StressWithWavesOfConcurrentUsers(wavesCount: 10, concurrentUsers: 1000);
            //user2App.StressWithWavesOfConcurrentUsers(wavesCount: 10, concurrentUsers: 1000);

            //Light
            //user1App.StressWithWavesOfConcurrentUsers(wavesCount: 1, concurrentUsers: 1);
            //user2App.StressWithWavesOfConcurrentUsers(wavesCount: 1, concurrentUsers: 1);

            Console.WriteLine("Press enter to stop and clean...");
            Console.ReadLine();
            if (plugin == PersistencePlugin.InMemory)
                UnityConfig.StatsMonitor.PrintStats();

            DbManager.DropDb(plugin);
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
