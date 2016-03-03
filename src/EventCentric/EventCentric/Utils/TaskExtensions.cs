namespace System.Threading.Tasks
{
    /// <summary>
    /// Provides usability overloads for <see cref="TaskFactory"/>.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Runs an action. The action can theoretically live forever.
        /// </summary>
        /// <param name="factory">The task factory.</param>
        /// <param name="action">The action to be executed.</param>
        public static void StartNewLongRunning(this TaskFactory factory, Action action)
        {
            factory.StartNew(action, TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness);
        }
    }
}
