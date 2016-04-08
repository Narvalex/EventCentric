using EventCentric;
using EventCentric.Log;
using EventCentric.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace PersistenceBenchmark
{
    [Guid("f9344900-bcd3-32ec-866b-4da1b0aee120")]
    public class UserAppService : ApplicationService
    {
        public UserAppService(IGuidProvider guid, ILogger log, string streamType, int eventsToPushMaxCount) : base(guid, log, streamType, eventsToPushMaxCount) { }

        public List<CreateOrUpdateUser> CreateCreateUserCommands(int quantity, int waves)
        {
            var list = new List<CreateOrUpdateUser>(quantity);
            for (int i = 0; i < quantity; i++)
            {
                var id = Guid.NewGuid();

                for (int j = 0; j < waves; j++)
                {
                    list.Add(new CreateOrUpdateUser(id, $"User_{id.ToString()}"));
                }
            }
            return list;
        }

        public void SendAWaveOfCommands(List<CreateOrUpdateUser> userCommands)
        {
            userCommands.ForEach(c => this.Send(c.UserId, c));
        }

        public void StressWithWavesOfConcurrentUsers(int wavesCount, int concurrentUsers)
        {
            this.log.Trace($"System is receiving {wavesCount * concurrentUsers} messages.");
            var wavesOfCommands = new List<List<CreateOrUpdateUser>>();

            wavesOfCommands.Add(CreateCreateUserCommands(concurrentUsers, wavesCount));

            wavesOfCommands.ForEach(w =>
            {
                System.Threading.ThreadPool.UnsafeQueueUserWorkItem(_ => SendAWaveOfCommands(w), null);
            });
            this.log.Trace($"System is now handling {wavesCount * concurrentUsers} messages.");
        }
    }
}
