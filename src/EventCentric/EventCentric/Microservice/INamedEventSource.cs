namespace EventCentric
{
    /// <summary>
    /// Marker interface that represents that a class is an event source, 
    /// like an object that is <see cref="EventSourcing.IEventSourced"/>, or an <see cref="ApplicationService"/>.
    /// </summary>
    public interface INamedEventSource { }
}
