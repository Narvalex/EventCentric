using EventCentric;
using EventCentric.Messaging;
using EventCentric.Utils;
using InformesDeServicio.Messages.Publicadores.DTOs;
using InformesDeServicio.Messages.Publicadores.InProcess.Commands;
using InformesDeServicio.Publicadores.DTOs;
using System;
using System.Runtime.InteropServices;

namespace InformesDeServicio.Publicadores
{
    #region +
    // StreamType: PublicadorApp_29c6550d-65de-4bb3-97e5-9b4682230938
    [Guid("29c6550d-65de-4bb3-97e5-9b4682230938")]
    #endregion
    public class PublicadorApp : ApplicationService, IPublicadorApp
    {
        public PublicadorApp(IEventBus bus, IGuidProvider guid, ITimeProvider time)
            : base(bus, guid, time)
        { }

        public Guid RegistrarPublicador(RegistrarOActualizarPublicadorDto dto)
        {
            var transactionId = this.NewGuid();
            var idPublicador = dto.IdPublicador;
            var now = this.Now;

            var datos = new DatosDePublicador(dto.Nombres, dto.Apellidos);
            var command = new RegistrarPublicador(idPublicador, datos, now);
            this.bus.Publish(transactionId, idPublicador, command);

            return transactionId;
        }

        public Guid ActualizarDatosDePublicador(RegistrarOActualizarPublicadorDto dto)
        {
            var transactionId = this.NewGuid();
            var now = this.Now;

            var datos = new DatosDePublicador(dto.Nombres, dto.Apellidos);
            this.bus.Publish(transactionId, dto.IdPublicador, new ActualizarDatosDePublicador(dto.IdPublicador, datos, now));

            return transactionId;
        }

        public Guid DarDeBajaAPublicador(Guid idPublicador)
        {
            var transactionId = this.NewGuid();
            var commmand = new DarDeBajaAPublicador(idPublicador, this.Now);
            this.bus.Publish(transactionId, idPublicador, commmand);
            return transactionId;
        }

        public Guid VolverADarDeAltaAPublicador(Guid idPublicador)
        {
            var transactionId = this.NewGuid();
            var command = new VolverADarDeAltaAPublicador(idPublicador, this.Now);
            this.bus.Publish(transactionId, idPublicador, command);
            return transactionId;
        }
    }
}
