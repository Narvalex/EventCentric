using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Processing;
using InformesDeServicio.Messages.Publicadores.InProcess;
using InformesDeServicio.Messages.Publicadores.InProcess.Commands;

namespace InformesDeServicio.Publicadores
{
    public class PublicadorProcessor : EventProcessor<Publicador>, IPublicadorInProcessMessageSubscriber
    {
        public PublicadorProcessor(IBus bus, ILogger log, IEventStore<Publicador> store)
            : base(bus, log, store)
        { }

        public void Receive(RegistrarPublicador command)
        {
            base.CreateNewStreamAndProcess(command.IdPublicador, command);
        }

        public void Receive(ActualizarDatosDePublicador command)
        {
            base.Process(command.IdPublicador, command);
        }

        public void Receive(DarDeBajaAPublicador command)
        {
            base.Process(command.IdPublicador, command);
        }

        public void Receive(VolverADarDeAltaAPublicador command)
        {
            base.Process(command.IdPublicador, command);
        }
    }
}
