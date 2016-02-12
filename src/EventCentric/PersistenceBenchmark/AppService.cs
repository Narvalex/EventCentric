using EventCentric;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersistenceBenchmark
{
    public class AppService : ApplicationService
    {
        public AppService(IServiceBus bus, IGuidProvider guid, ILogger log) : base(bus, guid, log) { }

        public List<CreateUser> CreateCreateUserCommands(int quantity)
        {
            var list = new List<CreateUser>(quantity);
            for (int i = 0; i < quantity; i++)
            {
                var id = Guid.NewGuid();
                list.Add(new CreateUser(id, $"User_{id.ToString()}"));
            }
            return list;
        }

        public void SendAWaveOfCommands(List<CreateUser> userCommands)
        {
            userCommands.ForEach(c => this.bus.Send(Guid.NewGuid(), c.UserId, c));
        }

        public void StressWithWavesOfConcurrentUsers(int wavesCount, int concurrentUsers)
        {
            this.log.Trace($"System is receiving {wavesCount * concurrentUsers} messages.");
            var wavesOfCommands = new List<List<CreateUser>>();
            for (int i = 0; i < wavesCount; i++)
            {
                wavesOfCommands.Add(CreateCreateUserCommands(concurrentUsers));
            }

            wavesOfCommands.ForEach(w =>
            {
                Task.Factory.StartNewLongRunning(() => SendAWaveOfCommands(w));
            });
            this.log.Trace($"System is now handling {wavesCount * concurrentUsers} messages.");
        }
    }
}
