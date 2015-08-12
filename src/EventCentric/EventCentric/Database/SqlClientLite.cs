using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace EventCentric.Database
{
    public class SqlClientLite
    {
        public string connectionString;
        public Dictionary<string, string> Connections { get; private set; }
        public string ConnectionString { get { return this.connectionString; } }

        private SqlClientLite()
        {
            this.Connections = new Dictionary<string, string>();
        }

        /// <summary>
        /// Constructs an instance of <see cref="SqlWrapper"/>
        /// </summary>
        /// <param name="connections">Connections to be resolved by clients of <see cref="SqlWrapper"/></param>
        public SqlClientLite(params KeyValuePair<string, string>[] connections)
            : this()
        {
            foreach (var connection in connections)
                this.Connections.Add(connection.Key, connection.Value);
        }

        /// <summary>
        /// Constructs an instance of <see cref="SqlWrapper"/>
        /// </summary>
        /// <param name="defaultConnectionString">A default connection string to be used by all al clients.</param>
        public SqlClientLite(string defaultConnectionString)
            : this()
        {
            this.connectionString = defaultConnectionString;
        }

        /// <summary>
        /// Sets up the connection for a given component, specified by its concrete type.
        /// </summary>
        /// <typeparam name="T">The concrete implementation of a component that needs a connection.</typeparam>
        /// <returns>An instance of <see cref="SqlWrapper"/></returns>
        public SqlClientLite SetUpConnectionFor<T>() where T : class
        {
            string connectionString;

            if (this.Connections.TryGetValue(typeof(T).Name, out connectionString))
                this.connectionString = connectionString;

            return this;
        }

        public IEnumerable<T> ExecuteReader<T>(string commandText, Func<IDataReader, T> project, params SqlParameter[] parameters)
        {
            var connection = new SqlConnection(this.connectionString);
            var command = new SqlCommand(commandText, connection);

            command.CommandType = CommandType.Text;
            if (parameters.Length > 0)
                command.Parameters.AddRange(parameters);

            connection.Open();
            using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                while (reader.Read())
                    yield return project(reader);
            }
        }

        public T ExecuteScalar<T>(string commandText, Func<IDataReader, T> project, params SqlParameter[] parameters)
        {
            var connection = new SqlConnection(this.connectionString);
            var command = new SqlCommand(commandText, connection);

            command.CommandType = CommandType.Text;
            if (parameters.Length > 0)
                command.Parameters.AddRange(parameters);

            connection.Open();
            using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                if (reader.Read())
                    return project(reader);
                else
                    return default(T);
            }
        }

        public static void DropDatabase(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;
            builder.InitialCatalog = "master";
            builder.AttachDBFilename = string.Empty;

            using (var connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        string.Format(
                            CultureInfo.InvariantCulture,
                            @"
    USE master
    IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}') 
    ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}') 
    DROP DATABASE [{0}];
    ",
                            databaseName);

                    command.ExecuteNonQuery();
                }
            }
        }

        public SqlParameterContainer AddParameters(string parameterName, object value)
        {
            return new SqlParameterContainer(parameterName, value);
        }

        public class SqlParameterContainer
        {
            private List<SqlParameter> parameters = new List<SqlParameter>();

            public SqlParameterContainer(string parameterName, object value)
            {
                this.parameters.Add(this.NewParameter(parameterName, value));
            }

            public SqlParameterContainer Add(string parameterName, object value)
            {
                this.parameters.Add(this.NewParameter(parameterName, value));
                return this;
            }

            public SqlParameter[] ToArray()
            {
                return this.parameters.ToArray();
            }

            /// <summary>
            /// Creates a new parameter for sql queries.
            /// </summary>
            /// <param name="parameterName">The parameter's name without '@'.</param>
            /// <param name="parameterObject">The Parameter</param>
            /// <returns></returns>
            private SqlParameter NewParameter(string parameterName, object value)
            {
                return new SqlParameter(string.Format("@{0}", parameterName), value);
            }
        }
    }
}
