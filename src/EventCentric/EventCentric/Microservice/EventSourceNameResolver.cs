using EventCentric.Microservice;

namespace EventCentric
{
    public static class EventSourceNameResolver
    {
        public static string ResolveNameOf<T>() where T : class, INamedEventSource
            => $"{typeof(T).FullName}_{typeof(T).GUID}";
    }
}
