using EventCentric.EventSourcing;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PersistenceBenchmark
{
    [Guid("f9344900-bcd3-32ec-866b-4da1b0aee120")]
    public class UserManagement : StateOf<UserManagement>,
        IUpdatesWhen<UserCreated>
    {
        private string name = string.Empty;

        public UserManagement(Guid id) : base(id) { }

        public UserManagement(Guid id, IEnumerable<IEvent> streamOfEvents) : base(id, streamOfEvents) { }

        public UserManagement(Guid id, ISnapshot snapshot) : base(id, snapshot)
        {
            var state = (UserManagementSnapshot)snapshot;
            this.name = state.Name;
        }

        public void When(UserCreated e) => this.name = e.Name;
    }
}
