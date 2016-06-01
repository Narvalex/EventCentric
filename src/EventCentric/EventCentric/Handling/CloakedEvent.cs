using EventCentric.EventSourcing;

namespace EventCentric.Handling
{
    /// <summary>
    /// An event that was hidden for the consumer
    /// </summary>
    public class CloakedEvent : Event { }
}
