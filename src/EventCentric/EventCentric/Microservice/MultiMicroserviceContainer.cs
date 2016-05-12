using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Events;
using System.Collections.Generic;
using System.Linq;

namespace EventCentric.Microservice
{
    public class MultiMicroserviceContainer : MicroserviceWorker,
        IMessageHandler<FatalErrorOcurred>
    {
        private List<IMicroservice> services;

        public MultiMicroserviceContainer(ISystemBus bus, ILogger log, IEnumerable<IMicroservice> services)
            : base(bus, log)
        {
            this.services = services.ToList();
            this.services.ForEach(s => ((ICanRegisterExternalListeners)s).Register(this));
        }

        protected override void OnStarting() => this.services.ForEach(s => s.Start());

        protected override void OnStopping() => this.services.ForEach(s => s.Stop());

        public new void Start() => base.Start();
    }
}
