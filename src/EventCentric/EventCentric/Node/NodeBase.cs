using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Utils;

namespace EventCentric
{
    public abstract class NodeBase : FSM
    {
        protected NodeBase(string name, IBus bus, ILogger log)
            : base(bus, log)
        {
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(name, "name");

            this.Name = name;
        }

        public string Name { get; private set; }
    }
}
