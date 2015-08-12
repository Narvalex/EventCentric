using EventCentric.Messaging;

namespace EventCentric
{
    public class FSM : Worker
    {
        public FSM(IBus bus)
            : base(bus)
        { }

        /// <summary>
        /// Starts engine
        /// </summary>
        public new void Start()
        {

        }

        /// <summary>
        /// Stops engine
        /// </summary>
        public new void Stop()
        {

        }
    }
}
