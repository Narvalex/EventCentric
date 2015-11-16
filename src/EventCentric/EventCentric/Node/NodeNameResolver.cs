namespace EventCentric
{
    public static class NodeNameResolver
    {
        public static string ResolveNameOf<T>()
        {
            return $"{typeof(T).FullName}_{typeof(T).GUID}";
        }
    }
}
