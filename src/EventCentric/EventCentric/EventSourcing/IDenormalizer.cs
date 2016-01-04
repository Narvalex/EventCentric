using EventCentric.Repository;

namespace EventCentric.EventSourcing
{
    public interface IDenormalizer : IEventSourced, IHandlerOf<ReadModelUpdated>
    {
        void Denormalize(IEventStoreDbContext context);
    }
}
