using EventCentric.Repository;

namespace EventCentric.EventSourcing
{
    public interface IDenormalizer : IEventSourced, IUpdatesWhen<ReadModelUpdated>
    {
        void UpdateReadModel(IEventStoreDbContext context);
    }
}
