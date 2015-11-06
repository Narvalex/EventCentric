using InformesDeServicio.Messages.Publicadores.Stored.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace InformesDeServicio.Tests.Messages
{
    [TestClass]
    public class SerializationFixture
    {
        [TestMethod]
        public void CUANDO_se_serializa_ENTONCES_tambien_se_puede_deserializar()
        {
            var fechaDeBaja = new DateTime(2015, 12, 12);
            var originalEvent = new PublicadorDadoDeBaja(fechaDeBaja);
            // TODO...
        }
    }
}
