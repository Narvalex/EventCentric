using EventCentric;
using EventCentric.Log;
using EventCentric.Utils;
using Occ.Messages;

namespace Occ.Client.Shared
{
    public class ItemClientApp : ApplicationService
    {
        public ItemClientApp(IGuidProvider guid, ILogger log, string streamType, int eventsToPushMaxCount) : base(guid, log, streamType, eventsToPushMaxCount)
        {
        }

        public void CreateItem(string name)
        {
            var itemId = this.guid.NewGuid();
            base.Send(itemId, new CreateNewItem(itemId, name));
        }
    }
}
