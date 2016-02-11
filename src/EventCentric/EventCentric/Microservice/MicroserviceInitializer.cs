using EventCentric.Database;
using System;
using System.Data.Entity;

namespace EventCentric
{
    public static class MicroserviceInitializer
    {
        private static object _lockObject = new object();
        private static IMicroservice _microservice = null;
        private static bool isRunning = false;

        public static void Run(Func<IMicroservice> microserviceFactory)
        {
            lock (_lockObject)
            {
                // Double checking
                if (_microservice != null || isRunning)
                    return;

                DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());
                _microservice = microserviceFactory.Invoke();
                _microservice.Start();
                isRunning = true;
            }
        }
    }
}
