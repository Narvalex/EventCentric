using EventCentric.Repository;

namespace EventCentric.EventSourcing
{
    public interface IDenormalizer : IEventSourced, IUpdatesOn<ReadModelUpdated>
    {
        void Denormalize(IEventStoreDbContext context);
    }
}
