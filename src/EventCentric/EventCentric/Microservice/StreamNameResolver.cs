using EventCentric.Microservice;

namespace EventCentric
{
    public static class StreamNameResolver
    {
        public static string ResolveFullNameOf<T>() where T : class, INamedEventSource
            => $"{ResolveNameOf<T>()}_{typeof(T).GUID}";

        public static string ResolveNameOf<T>() where T : class, INamedEventSource
            => $"{typeof(T).FullName.Replace('.', '@')}";
    }
}
