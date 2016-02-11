using EventCentric.EventSourcing;
using System;

namespace PersistenceBenchmark
{
    public class CreateUser : Command
    {
        public CreateUser(Guid userId, string name)
        {
            this.UserId = userId;
            this.Name = name;
        }

        public Guid UserId { get; }
        public string Name { get; }
    }

    public class UserCreated : Event
    {
        public UserCreated(Guid userId, string name)
        {
            this.UserId = userId;
            this.Name = name;
        }

        public Guid UserId { get; }
        public string Name { get; }
    }
}
