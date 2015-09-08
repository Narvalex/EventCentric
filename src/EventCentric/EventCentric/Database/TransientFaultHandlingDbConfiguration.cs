using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;
using System.Runtime.Remoting.Messaging;

namespace EventCentric.Database
{
    /// <summary>
    /// An Entity Framework 6 Code-Based Configuration.
    /// </summary>
    /// <remarks>
    /// More info: http://msdn.microsoft.com/en-us/data/jj680699#Example
    /// </remarks>
    public class TransientFaultHandlingDbConfiguration : DbConfiguration
    {
        /// <summary>
        /// Creates a new instance of <see cref="TransientFaultHandlingDbConfiguration"/>
        /// </summary>
        /// <remarks>
        /// Initiates a Connection Resiliency / Retry Logic for EF6. Applications connecting to a database server 
        /// have always been vulnerable to connection breaks due to back-end failures and network instability.
        /// However, in a LAN based environment working against dedicated database servers these errors are rare 
        /// enough that extra logic to handle those failures is not often required.
        /// Connection Resiliency refers to the ability for EF to automatically retry any commands that fail due to these connection breaks.
        /// More info: http://msdn.microsoft.com/en-us/data/dn456835.aspx
        /// </remarks>
        public TransientFaultHandlingDbConfiguration()
        {
            this.SetExecutionStrategy(
                "System.Data.SqlClient",
                () => SuspendExecutionStrategy
                    ? (IDbExecutionStrategy)new DefaultExecutionStrategy()
                    : new SqlAzureExecutionStrategy(20, TimeSpan.FromSeconds(30)));
        }

        /// <summary>
        /// A SuspendExecutionStrategy flag to the code based configuration class that needs to use a user 
        /// initiated transaction.
        /// </summary>
        /// <remarks> 
        /// More info: http://msdn.microsoft.com/es-ES/data/dn307226 
        /// </remarks>
        public static bool SuspendExecutionStrategy
        {
            get
            {
                return (bool?)CallContext.LogicalGetData("SuspendExecutionStrategy") ?? false;
            }
            set
            {
                CallContext.LogicalSetData("SuspendExecutionStrategy", value);
            }
        }

    }
}
