using InformesDeServicio.Publicadores.DTOs;
using System;
using System.Web.Http;
using System.Web.Http.Cors;

namespace InformesDeServicio.Publicadores.Server.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class PublicadorController : ApiController
    {
        private readonly IPublicadorApp app;

        public PublicadorController(IPublicadorApp app)
        {
            this.app = app;
        }

        [HttpPost]
        [Route("publicador/registrar")]
        public IHttpActionResult Registrar([FromBody]RegistrarOActualizarPublicadorDto dto)
        {
            var transactionId = this.app.RegistrarPublicador(dto);
            return this.Ok(transactionId);
        }

        [HttpPost]
        [Route("publicador/actualizar")]
        public IHttpActionResult Actualizar([FromBody]RegistrarOActualizarPublicadorDto dto)
        {
            var transactionId = this.app.ActualizarDatosDePublicador(dto);
            return this.Ok(transactionId);
        }

        [HttpPost]
        [Route("publicador/dar-de-baja/{idPublicador}")]
        public IHttpActionResult DarDeBaja([FromUri]Guid idPublicador)
        {
            var transactionId = this.app.DarDeBajaAPublicador(idPublicador);
            return this.Ok(transactionId);
        }

        [HttpPost]
        [Route("publicador/volver-a-dar-de-alta/{idPublicador}")]
        public IHttpActionResult VolverADarDeAlta([FromUri]Guid idPublicador)
        {
            var transactionId = this.app.VolverADarDeAltaAPublicador(idPublicador);
            return this.Ok(transactionId);
        }
    }
}
