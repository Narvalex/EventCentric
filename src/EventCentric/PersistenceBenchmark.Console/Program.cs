using Microsoft.Practices.Unity;
using System;

namespace PersistenceBenchmark.ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            //DbManager.CreateDbs();
            var mainContainer = UnityConfig.GetConfiguredContainer(true);

            var user1App = UnityConfig.UserContainer1.Resolve<UserAppService>();
            var user2App = UnityConfig.UserContainer2.Resolve<UserAppService>();

            // Holding in memory messages
            user1App.StressWithWavesOfConcurrentUsers(wavesCount: 10, concurrentUsers: 10000);
            user2App.StressWithWavesOfConcurrentUsers(wavesCount: 10, concurrentUsers: 10000);


            Console.ReadLine();
            DbManager.DropDbs();
        }
    }
}
