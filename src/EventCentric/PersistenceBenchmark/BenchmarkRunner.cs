using Microsoft.Practices.Unity;

namespace PersistenceBenchmark
{
    public static class BenchmarkRunner
    {
        public static void RunAsConfigured(IUnityContainer container)
        {
            var app = container.Resolve<AppService>();

            // Holding in memory messages
            app.StressWithWavesOfConcurrentUsers(10, 1000);
        }
    }
}
