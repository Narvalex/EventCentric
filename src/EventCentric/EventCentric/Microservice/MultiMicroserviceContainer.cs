using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Events;
using System.Collections.Generic;
using System.Linq;

namespace EventCentric.Microservice
{
    /// <summary>
    /// This container listens for Fatal error ocurred messages in order to request a system halt.
    /// </summary>
    public class MultiMicroserviceContainer : MicroserviceWorker
    {
        private readonly IMicroservice[] services;

        public MultiMicroserviceContainer(IBus bus, ILogger log, IEnumerable<IMicroservice> services)
            : base(bus, log)
        {
            this.services = services.ToArray();
            this.services
                .ForEach(s => ((ICanRegisterExternalListeners)s)
                    .Register(b => ((IBusRegistry)b).Register<FatalErrorOcurred>(this)));
        }

        protected override void OnStarting() => this.services.ForEach(s => s.Start());

        protected override void OnStopping() => this.services.ForEach(s => s.Stop());

        public new void Start() => base.Start();

        public new void Stop() => base.Stop();

        protected override void RegisterHandlersInBus(IBusRegistry bus)
        {
            // no handlers...
        }
    }
}
