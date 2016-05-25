using EventCentric.EventSourcing;

namespace PersistenceBenchmark
{
    public class TryAddNewSubscription : Command
    {
        public TryAddNewSubscription(string subscriberStreamType, string streamTypeOfProducer, string url, string token)
        {
            this.SubscriberStreamType = subscriberStreamType;
            this.StreamTypeOfProducer = streamTypeOfProducer;
            this.Url = url;
            this.Token = token;
        }

        public string SubscriberStreamType { get; }
        public string StreamTypeOfProducer { get; }
        public string Url { get; }
        public string Token { get; }
    }

    public class AddNewSubscription : Command
    {
        public AddNewSubscription(string subscriberStreamType, string streamTypeOfProducer, string url, string token)
        {
            this.SubscriberStreamType = subscriberStreamType;
            this.StreamTypeOfProducer = streamTypeOfProducer;
            this.Url = url;
            this.Token = token;
        }

        public string SubscriberStreamType { get; }
        public string StreamTypeOfProducer { get; }
        public string Url { get; }
        public string Token { get; }
    }

    public class NewSubscriptionAdded : Event { }
}
