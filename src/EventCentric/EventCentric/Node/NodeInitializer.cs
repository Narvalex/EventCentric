using EventCentric.Database;
using System;
using System.Data.Entity;

namespace EventCentric
{
    public static class NodeInitializer
    {
        private static object _lockObject = new object();
        private static INode _node = null;

        public static void Initialize(Func<INode> nodeFactory)
        {
            lock (_lockObject)
            {
                if (_node != null)
                    return;

                DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());
                _node = nodeFactory.Invoke();
                _node.Start();
            }
        }
    }
}
