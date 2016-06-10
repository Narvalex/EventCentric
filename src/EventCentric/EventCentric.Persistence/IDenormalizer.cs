using EventCentric.EventSourcing;

namespace EventCentric.Persistence
{
    public interface IDenormalizer : IEventSourced
    {
        void UpdateReadModel(IEventStoreDbContext context);
    }
}
