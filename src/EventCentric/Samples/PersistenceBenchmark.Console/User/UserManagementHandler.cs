using EventCentric.EventSourcing;
using EventCentric.Handling;
using EventCentric.Log;
using EventCentric.Messaging;
using System;
using System.Collections.Generic;

namespace PersistenceBenchmark
{
    public class UserManagementHandler : HandlerOf<UserManagement>,
        IHandle<CreateOrUpdateUser>
    //IHandles<FreePointsRewardedToUser>
    {
        public UserManagementHandler(IBus bus, ILogger log, IEventStore<UserManagement> store) : base(bus, log, store) { }

        //public IMessageHandling Handle(FreePointsRewardedToUser e)
        //{
        //    return base.FromStream(e.UserId, s =>
        //    s.Update(new UserReceivedPoints(s.Id, e.Points)));
        //}

        public IMessageHandling Handle(CreateOrUpdateUser c) =>
            base.FromNewStreamIfNotExists(c.UserId, s =>
            s.Update(new UserCreatedOrUpdated(c.UserId, c.Name)));


        private List<CreateOrUpdateUser> CreateCreateUserCommands(int quantity, int waves)
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

        public void StressWithWavesOfConcurrentUsers(int wavesCount, int concurrentUsers)
        {
            Console.WriteLine($"System is receiving {wavesCount * concurrentUsers} messages.");
            var wavesOfCommands = new List<List<CreateOrUpdateUser>>();

            wavesOfCommands.Add(CreateCreateUserCommands(concurrentUsers, wavesCount));

            wavesOfCommands.ForEach(w =>
            {
                System.Threading.ThreadPool.UnsafeQueueUserWorkItem(_ => w.ForEach(c => this.Send(c.UserId, c)), null);
            });
            Console.WriteLine($"System is now handling {wavesCount * concurrentUsers} messages.");
        }
    }
}
