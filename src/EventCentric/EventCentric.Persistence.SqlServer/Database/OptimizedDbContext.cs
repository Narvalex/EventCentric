using System.Data.Entity;

namespace EventCentric.Database
{
    /// <summary>
    /// A db context that can be programatically be optimized for 
    /// reads.
    /// </summary>
    public abstract class OptimizedDbContext : DbContext
    {
        public OptimizedDbContext(bool isReadOnly, string connectionString)
            : base(connectionString)
        {
            if (isReadOnly)
                this.Configuration.AutoDetectChangesEnabled = false;
        }

        public OptimizedDbContext(string connectionString)
            : base(connectionString)
        { }
    }
}
