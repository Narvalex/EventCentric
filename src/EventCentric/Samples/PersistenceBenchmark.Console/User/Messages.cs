using EventCentric.EventSourcing;
using System;

namespace PersistenceBenchmark
{
    public class CreateOrUpdateUser : Command
    {
        public CreateOrUpdateUser(Guid userId, string name)
        {
            this.UserId = userId;
            this.Name = name;
        }

        public Guid UserId { get; }
        public string Name { get; }
    }

    public class UserCreatedOrUpdated : Event
    {
        public UserCreatedOrUpdated(Guid userId, string name)
        {
            this.UserId = userId;
            this.Name = name;
        }

        public Guid UserId { get; }
        public string Name { get; }
    }

    public class UserReceivedPoints : Event
    {
        public UserReceivedPoints(Guid userId, int points)
        {
            this.UserId = userId;
            this.Points = points;
        }

        public Guid UserId { get; }
        public int Points { get; }
    }
}
