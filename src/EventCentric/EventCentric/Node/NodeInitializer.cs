using EventCentric.Database;
using System;
using System.Data.Entity;

namespace EventCentric
{
    public static class NodeInitializer
    {
        private static object _lockObject = new object();
        private static INode _node = null;
        private static bool isRunning = false;

        public static void Initialize(Func<INode> nodeFactory)
        {
            lock (_lockObject)
            {
                // Double checking
                if (_node != null || isRunning)
                    return;

                DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());
                _node = nodeFactory.Invoke();
                _node.Start();
                isRunning = true;
            }
        }
    }
}
