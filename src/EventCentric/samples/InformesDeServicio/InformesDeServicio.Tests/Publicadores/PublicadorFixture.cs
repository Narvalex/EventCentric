using EventCentric.Utils.Testing;
using InformesDeServicio.Messages.Publicadores.DTOs;
using InformesDeServicio.Messages.Publicadores.InProcess.Commands;
using InformesDeServicio.Messages.Publicadores.Stored.Events;
using InformesDeServicio.Publicadores;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace InformesDeServicio.Tests.Publicadores.PublicadorFixture
{
    [TestClass]
    public class DADO_ningun_publicador
    {
        private const string _streamType = "PublicadorInProcessApp";

        [TestMethod]
        public void CUANDO_se_registra_publicador_ENTONCES_quedan_registrados_sus_datos_y_es_dado_de_alta()
        {
            var publicadorId = Guid.NewGuid();
            var dto = new DatosDePublicador("Alexis", "Narvaez");
            var command = new RegistrarPublicador(publicadorId, dto, DateTime.Now);

            var aggregate = new Publicador(command.StreamId);
            Assert.IsFalse(((PublicadorMemento)aggregate.SaveToMemento()).PublicadorEstaDadoDeAlta);

            aggregate
                .When(command)
                .ThenExpectSingle<PublicadorRegistrado>()
                .AndNotAny<DatosDePublicadorActualizados>();

            Assert.AreEqual(dto, aggregate.SingleEventOfType<PublicadorRegistrado>().Datos);
            Assert.IsTrue(((PublicadorMemento)aggregate.SaveToMemento()).PublicadorEstaDadoDeAlta);
        }

        [TestMethod]
        public void DADO_publicador_registrado_CUANDO_se_actualizan_los_datos_ENTONCES_la_actualizacion_se_hace_efectiva()
        {
            var publicadorId = Guid.NewGuid();
            var dtoOriginal = new DatosDePublicador("Alexis", "Narvaez");
            var dtoActualizado = new DatosDePublicador("Alexis Darien", "Narváez Gamarra");


            var aggregate = new Publicador(publicadorId);

            aggregate
                .GivenOn(new PublicadorRegistrado(dtoOriginal))
                .When(new ActualizarDatosDePublicador(publicadorId, dtoActualizado, DateTime.Now))
                .ThenExpectAtLeastOne<DatosDePublicadorActualizados>()
                .AndNotAny<PublicadorRegistrado>();

            Assert.AreNotEqual(dtoOriginal, aggregate.SingleEventOfType<DatosDePublicadorActualizados>().DatosActualizados);
            Assert.AreEqual(dtoActualizado, aggregate.SingleEventOfType<DatosDePublicadorActualizados>().DatosActualizados);
        }

        [TestMethod]
        public void ProcessorTest_DADO_publicador_registrado_CUANDO_se_actualizan_los_datos_ENTONCES_la_actualizacion_se_hace_efectiva()
        {
            var publicadorId = Guid.NewGuid();
            var sut = new EventProcessorTestHelper<Publicador, PublicadorProcessor>(publicadorId);
            var processor = new PublicadorProcessor(sut.Bus, sut.Log, sut.Store);
            sut.Setup(processor);

            var dtoOriginal = new DatosDePublicador("Alexis", "Narvaez");
            var dtoActualizado = new DatosDePublicador("Alexis Darien", "Narváez Gamarra");

            sut.Given(new PublicadorRegistrado(dtoOriginal));
            var aggregate = sut.When(new ActualizarDatosDePublicador(publicadorId, dtoActualizado, DateTime.Now))
                               .ThenExpectAtLeastOne<DatosDePublicadorActualizados>()
                               .AndNotAny<PublicadorRegistrado>();

            var persistedMemento = sut.ThenPersistsNewSerializedMemento<PublicadorMemento>();
            var persistedEvents = sut.ThenPersistsNewSerializedEvents();

            Assert.IsNotNull(persistedMemento);
            Assert.IsTrue(persistedEvents.Count() == aggregate.PendingEvents.Count());
        }

        [TestMethod]
        public void DADO_publicador_registrado_CUANDO_se_le_da_de_baja_ENTONCES_la_baja_se_hace_efectiva()
        {
            var publicadorId = Guid.NewGuid();
            var dto = new DatosDePublicador("Alexis", "Narvaez");

            var fechaDeBaja = DateTime.Now;

            var aggregate = new Publicador(publicadorId);

            aggregate
                .GivenOn(new PublicadorRegistrado(dto))
                .When(new DarDeBajaAPublicador(publicadorId, fechaDeBaja))
                .ThenExpectSingle<PublicadorDadoDeBaja>();

            Assert.AreEqual(fechaDeBaja, aggregate.SingleEventOfType<PublicadorDadoDeBaja>().FechaDeBaja);

            Assert.IsFalse(((PublicadorMemento)aggregate.SaveToMemento()).PublicadorEstaDadoDeAlta);
        }

        [TestMethod]
        public void DADO_publicador_dado_de_baja_CUANDO_se_intenta_dar_de_baja_ENTONCES_se_mantiene_el_estado_de_baja()
        {
            var publicadorId = Guid.NewGuid();

            // DADO publicador dado de baja
            var aggregate = new Publicador(publicadorId, new PublicadorMemento(1, false));

            var fechaDeIntento = DateTime.Now;

            aggregate
                .When(new DarDeBajaAPublicador(publicadorId, fechaDeIntento))
                .ThenExpectSingle<SeIntentoDarDeBajaAPublicadorQueYaEstaDadoDeBaja>()
                .AndNotAny<PublicadorDadoDeBaja>();

            Assert.AreEqual(
                fechaDeIntento,
                aggregate.SingleEventOfType<SeIntentoDarDeBajaAPublicadorQueYaEstaDadoDeBaja>().FechaDeIntento);

            Assert.IsFalse(((PublicadorMemento)aggregate.SaveToMemento()).PublicadorEstaDadoDeAlta);
        }

        [TestMethod]
        public void DADO_publicador_dado_de_baja_CUANDO_se_intenta_dar_de_alta_ENTONCES_se_vuelve_a_dar_de_alta()
        {
            var publicadorId = Guid.NewGuid();

            var aggregate = new Publicador(publicadorId, new PublicadorMemento(1, true));

            var fechaDeIntento = DateTime.Now;

            aggregate
                .GivenOn(new PublicadorDadoDeBaja(DateTime.Now))
                .When(new VolverADarDeAltaAPublicador(publicadorId, fechaDeIntento))
                .ThenExpectSingle<PublicadorVueltoADarDeAlta>()
                .AndNotAny<SeIntentoVolverADarDeAltaAPublicadorQueYaEstaDadoDeAlta>();

            Assert.AreEqual(
                fechaDeIntento,
                aggregate.SingleEventOfType<PublicadorVueltoADarDeAlta>().FechaDeVueltaADarDeAlta);

            Assert.IsTrue(((PublicadorMemento)aggregate.SaveToMemento()).PublicadorEstaDadoDeAlta);
        }

        [TestMethod]
        public void DADO_publicador_dado_de_alta_CUANDO_se_vuele_a_intentar_dar_de_alta_ENTONCES_se_mantiene_el_estado_de_alta()
        {
            var publicadorId = Guid.NewGuid();

            var aggregate = new Publicador(publicadorId, new PublicadorMemento(1, true));

            var fechaDeIntento = DateTime.Now;

            aggregate
                .When(new VolverADarDeAltaAPublicador(publicadorId, fechaDeIntento))
                .ThenExpectSingle<SeIntentoVolverADarDeAltaAPublicadorQueYaEstaDadoDeAlta>()
                .AndNotAny<PublicadorVueltoADarDeAlta>();

            Assert.AreEqual(
                fechaDeIntento,
                aggregate.SingleEventOfType<SeIntentoVolverADarDeAltaAPublicadorQueYaEstaDadoDeAlta>().FechaDeIntento);

            Assert.IsTrue(((PublicadorMemento)aggregate.SaveToMemento()).PublicadorEstaDadoDeAlta);
        }
    }
}
