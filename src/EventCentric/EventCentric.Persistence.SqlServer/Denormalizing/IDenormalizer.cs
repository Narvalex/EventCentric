using EventCentric.EventSourcing;

namespace EventCentric.Persistence.SqlServer
{
    public interface IDenormalizer : IEventSourced, IUpdatesWhen<ReadModelUpdated>
    {
        void UpdateReadModel(IEventStoreDbContext context);
    }
}
