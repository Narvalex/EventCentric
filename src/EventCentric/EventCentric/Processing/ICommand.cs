using EventCentric.EventSourcing;

namespace EventCentric.Processing
{
    /// <summary>
    /// Marks an event that is expecting a behavior to be performed. Still, 
    /// is an event. E.g. Client requested withdraw money.
    /// </summary>
    public interface ICommand : IEvent { }
}
