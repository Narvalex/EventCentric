using System.Data.SqlClient;
using System.Globalization;

namespace EventCentric.Respository
{
    public class DbInitializer
    {
        public static void CreateDatabaseObjects(string connectionString, bool createDatabase = false, bool isForclientNode = false)
        {
            if (createDatabase)
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
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}') CREATE DATABASE [{0}];
",
                                databaseName);

                        command.ExecuteNonQuery();
                    }
                }
            }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = isForclientNode ? EventsCreateTableScript
                                                          : EventsCreateTableScript + StreamsCreateTableScript
                                                            + SubscriptionsCreateTableScript + InboxCreateTableScript;
                    command.ExecuteNonQuery();
                }
            }
        }

        #region EventStore.Events
        private const string EventsCreateTableScript =
@"-- Events
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'EventStore')
EXECUTE sp_executesql N'CREATE SCHEMA [EventStore] AUTHORIZATION [dbo]';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[EventStore].[Events]') AND type in (N'U'))
CREATE TABLE [EventStore].[Events](
    [StreamType] [nvarchar] (255) NOT NULL,
	[StreamId] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[EventId] [uniqueidentifier] NOT NULL,
	CONSTRAINT EventStore_Events_EventId UNIQUE(EventId),
	[EventType] [nvarchar] (255) NOT NULL,
	[CorrelationId] [uniqueidentifier] NULL,
    [EventCollectionVersion] [int] IDENTITY(1,1) NOT NULL,
    [CreationDate] [datetime] NOT NULL,
	[Payload] [nvarchar] (max) NOT NULL

PRIMARY KEY CLUSTERED 
(
    [StreamType] ASC,
	[StreamId] ASC, 
    [Version] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY];";
        #endregion

        #region EventStore.Streams
        private const string StreamsCreateTableScript =
@"--Streams
IF NOT EXISTS(SELECT* FROM sys.objects WHERE object_id = OBJECT_ID(N'[EventStore].[Streams]') AND type in (N'U'))
CREATE TABLE[EventStore].[Streams](
    [StreamId] [uniqueidentifier] NOT NULL,
    [Version] [int] NOT NULL,
    [Memento] [nvarchar](max) NULL,
    [StreamCollectionVersion] [int] IDENTITY(1,1) NOT NULL,
    [CreationDate] [datetime] NOT NULL,
    [UpdateTime] [datetime] NOT NULL
PRIMARY KEY CLUSTERED
(
    [StreamId] ASC
)WITH(PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON[PRIMARY]
) ON[PRIMARY];";
        #endregion

        #region EventStore.Subscriptions
        private const string SubscriptionsCreateTableScript =
@"-- Subscriptons
IF NOT EXISTS(SELECT* FROM sys.objects WHERE object_id = OBJECT_ID(N'[EventStore].[SubscribedSources]') AND type in (N'U'))
CREATE TABLE[EventStore].[Subscriptions](
	[StreamType] [nvarchar] (255) NOT NULL,
    [Url] [nvarchar] (500) NOT NULL,
    [ProcessorBufferVersion] [int] NOT NULL,
    [IsPoisoned] [bit] NOT NULL,
    [PoisonEventCollectionVersion] [int] NOT NULL,
    [DeadLetterPayload] [nvarchar] (max) NULL,
    [ExceptionMessage] [nvarchar] (max) NULL,
    [CreationDate] [datetime] NOT NULL,
    [UpdateTime] [datetime] NOT NULL
PRIMARY KEY CLUSTERED
(
    [StreamType] ASC
)WITH(PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON[PRIMARY]
) ON[PRIMARY];";
        #endregion

        #region EventStore.Inbox
        private const string InboxCreateTableScript =
@"-- Inbox
IF NOT EXISTS(SELECT* FROM sys.objects WHERE object_id = OBJECT_ID(N'[EventStore].[Inbox]') AND type in (N'U'))
CREATE TABLE[EventStore].[Inbox](
	[InboxId] [bigint] IDENTITY(1,1) NOT NULL,
    [EventId] [uniqueidentifier] NOT NULL,
CONSTRAINT EventStore_Inbox_EventId UNIQUE(EventId),
	[StreamType] [nvarchar] (255) NOT NULL,
    [StreamId] [uniqueidentifier] NOT NULL,
    [Version] [int] NOT NULL,
    [EventType] [nvarchar] (255) NOT NULL,
    [EventCollectionVersion] [int] NOT NULL,
    [Ignored] [bit] NULL,
    [CreationDate] [datetime] NOT NULL,
	[Payload] [nvarchar] (max) NOT NULL

PRIMARY KEY CLUSTERED
(
    [InboxId] ASC
)WITH(PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON[PRIMARY]
) ON[PRIMARY];
";
        #endregion
    }
}
