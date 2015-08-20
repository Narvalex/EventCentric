namespace EventCentric.Messaging
{
    public static class BusExtensions
    {
        public static void Send(this IBus bus, IMessage message)
        {
            bus.Publish(message);
        }

        public static void Send(this IBus bus, params IMessage[] messages)
        {
            bus.Publish(messages);
        }

        public static void Publish(this IBus bus, params IMessage[] messages)
        {
            foreach (var message in messages)
                bus.Publish(message);
        }
    }
}
