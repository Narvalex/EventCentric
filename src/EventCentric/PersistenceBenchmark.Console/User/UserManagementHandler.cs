using EventCentric.EventSourcing;
using EventCentric.Handling;
using EventCentric.Log;
using EventCentric.Messaging;

namespace PersistenceBenchmark
{
    public class UserManagementHandler : HandlerOf<UserManagement>,
        IHandles<CreateOrUpdateUser>
    //IHandles<FreePointsRewardedToUser>
    {
        public UserManagementHandler(ISystemBus bus, ILogger log, IEventStore<UserManagement> store) : base(bus, log, store) { }

        //public IMessageHandling Handle(FreePointsRewardedToUser e)
        //{
        //    return base.FromStream(e.UserId, s =>
        //    s.Update(new UserReceivedPoints(s.Id, e.Points)));
        //}

        public IMessageHandling Handle(CreateOrUpdateUser c) =>
            base.FromNewStreamIfNotExists(c.UserId, s =>
            s.Update(new UserCreatedOrUpdated(c.UserId, c.Name)));
    }
}
