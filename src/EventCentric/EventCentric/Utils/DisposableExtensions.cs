namespace System
{
    public static class DisposableExtensions
    {
        public static void AsDisposable<T>(this T disposable, Action<T> action) where T : IDisposable
        {
            using (disposable)
            {
                action.Invoke(disposable);
            }
        }

        public static TResult AsDisposable<TResult, TDisposable>(this TDisposable disposable, Func<TDisposable, TResult> function) where TDisposable : IDisposable
        {
            using (disposable)
            {
                return function.Invoke(disposable);
            }
        }
    }
}
