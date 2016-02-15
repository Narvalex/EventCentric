using EventCentric.EventSourcing;
using EventCentric.Handling;
using EventCentric.Log;
using EventCentric.Messaging;

namespace PersistenceBenchmark
{
    public class UserManagementHandler : HandlerOf<UserManagement>,
        IHandles<CreateUser>
    {
        public UserManagementHandler(IBus bus, ILogger log, IEventStore<UserManagement> store) : base(bus, log, store) { }

        public IMessageHandling Handle(CreateUser c) =>
            base.FromNewStream(c.UserId, s =>
            s.Update(new UserCreated(c.UserId, c.Name)));
    }
}
