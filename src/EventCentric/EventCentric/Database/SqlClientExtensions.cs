using System.Data.SqlClient;

namespace EventCentric.Database
{
    public static class SqlClientLiteExtensions
    {
        public static SqlClientLite AsMaster(this SqlClientLite sql)
        {
            var builder = new SqlConnectionStringBuilder(sql.ConnectionString);
            builder.InitialCatalog = "master";
            builder.AttachDBFilename = string.Empty;
            sql.ConnectionString = builder.ConnectionString;
            return sql;
        }
    }
}
