using EventCentric.EventSourcing;
using EventCentric.Utils.Testing;
using InformesDeServicio.Messages.Publicadores.DTOs;
using InformesDeServicio.Messages.Publicadores.InProcess.Commands;
using InformesDeServicio.Messages.Publicadores.Store.Events;
using InformesDeServicio.Publicadores;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace InformesDeServicio.Tests.Publicadores.PublicadorFixture
{
    [TestClass]
    public class DADO_ningun_publicador
    {
        private const string _streamType = "PublicadorInProcessApp";

        [TestMethod]
        public void CUANDO_se_registra_publicador_ENTONCES_quedan_registrados_sus_datos()
        {
            var publicadorId = Guid.NewGuid();
            var dto = new DatosDePublicador("Alexis", "Narvaez", DateTime.Now, DateTime.Now);
            var command = new RegistrarPublicador(dto).AsInProcessFormattedEvent(publicadorId, publicadorId, publicadorId, _streamType);

            var aggregate = new Publicador(command.StreamId);

            Assert.AreEqual(dto, aggregate.ExpectSingleEventOfType<PublicadorRegistrado>().Datos);
        }

        [TestMethod]
        public void DADO_publicador_registrado_CUANDO_se_actualizan_los_datos_ENTONCES_la_actualizacion_se_hace_efectiva()
        {
            var publicadorId = Guid.NewGuid();
            var dto = new DatosDePublicador("Alexis", "Narvaez", DateTime.Now, DateTime.Now);
            var e1 = new PublicadorRegistrado(dto).AsVersion(1).WithStreamIdOf(publicadorId);


            var aggregate = new Publicador(publicadorId);
            EventSourcedExtensionsForSpecifications.On(aggregate, e1);

            // TODO: when... then...
            Assert.IsNotNull(aggregate);
        }
    }
}
