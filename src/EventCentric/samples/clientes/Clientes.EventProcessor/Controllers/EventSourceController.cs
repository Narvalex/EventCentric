using System.Web.Mvc;

namespace EventCentric.Publishing
{
    public class EventSourceController : Controller
    {
        private readonly IEventSource source;

        public EventSourceController(IEventSource source)
        {
            // We do not check null for performance reasons.
            //Ensure.NotNull(source, "source");

            this.source = source;
        }

        [HttpGet]
        public ActionResult Events(int eventBufferVersion)
        {
            var response = this.source.PollEvents(eventBufferVersion);
            return this.Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}
