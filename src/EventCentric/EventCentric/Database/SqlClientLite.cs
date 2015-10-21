using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace EventCentric.Database
{
    public class SqlClientLite
    {
        /// <summary>
        /// Constructs an instance of <see cref="SqlWrapper"/>
        /// </summary>
        /// <param name="defaultConnectionString">A default connection string to be used by all al clients.</param>
        public SqlClientLite(string defaultConnectionString, int timeoutInSeconds = 30)
            : this(timeoutInSeconds)
        {
            this.ConnectionString = defaultConnectionString;
        }

        public SqlClientLite(int timeoutInSeconds = 30)
        {
            this.TimeoutInSeconds = timeoutInSeconds;
        }

        public int TimeoutInSeconds { get; set; }
        public string ConnectionString { get; set; }

        public IEnumerable<T> ExecuteReader<T>(string commandText, Func<IDataReader, T> project, params SqlParameter[] parameters)
        {
            var connection = new SqlConnection(this.ConnectionString);
            var command = new SqlCommand(commandText, connection);
            command.CommandTimeout = this.TimeoutInSeconds;
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

        public IEnumerable<T> ExecuteReader<T>(int adhocTimeout, string commandText, Func<IDataReader, T> project, params SqlParameter[] parameters)
        {
            var connection = new SqlConnection(this.ConnectionString);
            var command = new SqlCommand(commandText, connection);
            command.CommandTimeout = adhocTimeout;
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

        public T ExecuteReaderFirstOrDefault<T>(string commandText, Func<IDataReader, T> project, params SqlParameter[] parameters)
        {
            var connection = new SqlConnection(this.ConnectionString);
            var command = new SqlCommand(commandText, connection);
            command.CommandTimeout = this.TimeoutInSeconds;
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

        public T ExecuteReaderFirstOrDefault<T>(int adhocTimeout, string commandText, Func<IDataReader, T> project, params SqlParameter[] parameters)
        {
            var connection = new SqlConnection(this.ConnectionString);
            var command = new SqlCommand(commandText, connection);
            command.CommandTimeout = adhocTimeout;
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

        public int ExecuteNonQuery(string commandText, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.CommandTimeout = this.TimeoutInSeconds;
                    command.CommandType = CommandType.Text;

                    if (parameters.Length > 0)
                        command.Parameters.AddRange(parameters);

                    return command.ExecuteNonQuery();
                }
            }
        }

        public int ExecuteNonQuery(int adhocTimeout, string commandText, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.CommandTimeout = adhocTimeout;
                    command.CommandType = CommandType.Text;

                    if (parameters.Length > 0)
                        command.Parameters.AddRange(parameters);

                    return command.ExecuteNonQuery();
                }
            }
        }

        public void DropDatabase()
        {
            DropDatabase(this.ConnectionString);
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

        public static string GetConnectionStringFromConfigFile(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }

        public static SqlClientLite CreateNewFromConfigFile(string name, int timeoutInSeconds = 30)
        {
            var connectionString = SqlClientLite.GetConnectionStringFromConfigFile(name);
            return new SqlClientLite(connectionString, timeoutInSeconds);
        }
    }
}
