using EventCentric.EventSourcing;
using System;
using System.Collections.Generic;

namespace PersistenceBenchmark
{
    public class UserManagement : StateOf<UserManagement>, IUpdatesWhen<UserCreated>
    {
        private string name = string.Empty;

        public UserManagement(Guid id) : base(id) { }

        public UserManagement(Guid id, IEnumerable<IEvent> streamOfEvents) : base(id, streamOfEvents) { }

        public UserManagement(Guid id, ISnapshot snapshot) : base(id, snapshot)
        {
            var state = (UserManagementSnapshot)snapshot;
            this.name = state.Name;
        }

        public override ISnapshot SaveToSnapshot()
        {
            return new UserManagementSnapshot(this.Version, this.name);
        }

        public void When(UserCreated e) => this.name = e.Name;
    }
}
