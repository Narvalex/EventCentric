using EventCentric.EventSourcing;

namespace EventCentric.Persistence
{
    public interface IDenormalizer : IEventSourced, IUpdatesWhen<ReadModelUpdated>
    {
        void UpdateReadModel(IEventStoreDbContext context);
    }
}
