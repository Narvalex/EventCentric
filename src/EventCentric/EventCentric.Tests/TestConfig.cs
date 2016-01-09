namespace EventCentric.Tests
{
    public abstract class TestConfig
    {
        protected const string DbIntegrationCategory = "DbIntegration";
        //protected const string defaultConnectionString = "server = (local); Database=EventCentricDbTest;User Id = sa; pwd =123456";
        protected const string defaultConnectionString = "Data Source=.\\SQLEXPRESS; Integrated Security=True; Initial Catalog=LabcoleVTestDb; MultipleActiveResultSets=True";
    }
}
