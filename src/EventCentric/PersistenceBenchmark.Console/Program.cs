using System;

namespace PersistenceBenchmark.ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.RunAsConfigured(UnityConfig.GetConfiguredContainer(true));
            Console.ReadLine();
        }
    }
}
