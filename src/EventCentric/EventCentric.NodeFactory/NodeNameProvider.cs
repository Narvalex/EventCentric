namespace EventCentric
{
    public static class NodeNameProvider
    {
        public static string ResolveNameOf<T>()
        {
            return $"{typeof(T)}_{typeof(T).GUID}";
        }
    }
}
