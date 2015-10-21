namespace EventCentric
{
    public static class NodeNameProvider
    {
        public static string ResolveNameOf<T>()
        {
            return $"{typeof(T).Name}_{typeof(T).GUID}";
        }
    }
}
