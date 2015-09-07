using System.Data.Entity;

namespace EventCentric.Database
{
    /// <summary>
    /// A db context that can be programatically be optimized for 
    /// reads.
    /// </summary>
    public abstract class OptimizedDbContext<T> : DbContext where T : DbContext
    {
        static OptimizedDbContext()
        {
            System.Data.Entity.Database.SetInitializer<T>(null);
        }

        public OptimizedDbContext(bool isReadOnly, string nameOrConnectionString)
        {
            if (isReadOnly)
                this.Configuration.AutoDetectChangesEnabled = false;
        }
    }
}
