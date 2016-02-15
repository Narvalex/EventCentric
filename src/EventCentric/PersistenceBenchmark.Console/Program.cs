using System;

namespace PersistenceBenchmark.ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            UnityConfig.GetConfiguredContainer(true);
            BenchmarkRunner.RunAsConfigured(UnityConfig.UserContainer);
            Console.ReadLine();
            DbManager.DropDb();
        }
    }
}
