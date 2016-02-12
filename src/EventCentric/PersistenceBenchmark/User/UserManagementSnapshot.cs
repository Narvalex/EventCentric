using EventCentric.EventSourcing;

namespace PersistenceBenchmark
{
    public class UserManagementSnapshot : Snapshot
    {
        public UserManagementSnapshot(long version, string name) : base(version)
        {
            this.Name = name;
        }

        public string Name { get; }
    }
}
