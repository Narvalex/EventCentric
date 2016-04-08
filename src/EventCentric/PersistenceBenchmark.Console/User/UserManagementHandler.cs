using EventCentric.EventSourcing;
using EventCentric.Handling;
using EventCentric.Log;
using EventCentric.Messaging;

namespace PersistenceBenchmark
{
    public class UserManagementHandler : HandlerOf<UserManagement>,
        IHandles<CreateOrUpdateUser>
    {
        public UserManagementHandler(IBus bus, ILogger log, IEventStore<UserManagement> store) : base(bus, log, store) { }

        public IMessageHandling Handle(CreateOrUpdateUser c) =>
            base.FromNewStreamIfNotExists(c.UserId, s =>
            s.Update(new UserCreatedOrUpdated(c.UserId, c.Name)));
    }
}
