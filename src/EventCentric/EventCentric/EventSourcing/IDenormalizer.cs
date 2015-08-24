using EventCentric.Repository;

namespace EventCentric.EventSourcing
{
    public interface IDenormalizer : IEventSourced
    {
        void Denormalize(IEventStoreDbContext context);
    }
}
