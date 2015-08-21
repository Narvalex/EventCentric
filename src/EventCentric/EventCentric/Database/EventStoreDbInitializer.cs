using System.Data.SqlClient;
using System.Globalization;

namespace EventCentric.Database
{
    public class EventStoreDbInitializer
    {
        public static void CreateDatabaseObjects(string connectionString, bool createDatabase = false)
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
                    command.CommandText =
@"
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'EventStore')
EXECUTE sp_executesql N'CREATE SCHEMA [EventStore] AUTHORIZATION [dbo]';

-- Events
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[EventStore].[Events]') AND type in (N'U'))
CREATE TABLE [EventStore].[Events](
	[StreamId] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[EventId] [uniqueidentifier] NOT NULL,
	CONSTRAINT EventStore_Events_EventId UNIQUE(EventId),
	[EventType] [nvarchar] (255) NOT NULL,
	[CorrelationId] [uniqueidentifier] NULL,
	[CreationDate] [datetime] NOT NULL,
	[Payload] [nvarchar] (max) NOT NULL

PRIMARY KEY CLUSTERED 
(
	[StreamId] ASC, 
    [Version] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY];

-- Streams
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[EventStore].[Streams]') AND type in (N'U'))
CREATE TABLE [EventStore].[Streams](
    [StreamId] [uniqueidentifier] NOT NULL,
    [Version] [int] NOT NULL,
    [Memento] [nvarchar](max) NULL,
    [CreationDate] [datetime] NOT NULL,
    [StreamCollectionVersion] [int] IDENTITY(1,1) NOT NULL
PRIMARY KEY CLUSTERED 
(
	[StreamId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY];

-- Subscriptions
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[EventStore].[Subscriptions]') AND type in (N'U'))
CREATE TABLE [EventStore].[Subscriptions](
	[StreamType] [nvarchar] (255) NOT NULL,
    [StreamId] [uniqueidentifier] NOT NULL,
    [LastProcessedVersion] [int] NOT NULL,
	[LastProcessedEventId] [uniqueidentifier] NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[IsPoisoned] [bit] NOT NULL,
	[ExceptionMessage] [nvarchar] (max) NULL
PRIMARY KEY CLUSTERED 
(
	[StreamType] ASC,
	[StreamId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY];

-- Subscribed sources
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[EventStore].[SubscribedSources]') AND type in (N'U'))
CREATE TABLE [EventStore].[SubscribedSources](
	[StreamType] [nvarchar] (255) NOT NULL,
    [Url] [nvarchar] (500) NOT NULL,
    [StreamCollectionVersion] [int] NOT NULL
PRIMARY KEY CLUSTERED 
(
	[StreamType] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY];

-- Inbox
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[EventStore].[Inbox]') AND type in (N'U'))
CREATE TABLE [EventStore].[Inbox](
	[InboxId] [bigint] IDENTITY(1,1) NOT NULL,
	[EventId] [uniqueidentifier] NOT NULL,
	CONSTRAINT EventStore_Inbox_EventId UNIQUE(EventId),
	[StreamType] [nvarchar] (255) NOT NULL,
	[StreamId] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[EventType] [nvarchar] (255) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
    [Ignored] [bit] NULL,
	[Payload] [nvarchar] (max) NOT NULL

PRIMARY KEY CLUSTERED 
(
	[InboxId] ASC 
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY];
";
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
