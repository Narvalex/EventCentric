--=====================================================
-- README: This script can drop and create the schema. 
-- To achieve this, you need to specify the desired db 
-- name in 2 places bellow
--====================================================

USE [master]
GO


declare @dbName varchar(max);
--==========================
-- 1/2 CHANGE DB NAME HERE
--==========================
set @dbName = N'{dbName}';


-- Create database
-- More info: http://dba.stackexchange.com/questions/30349/how-to-drop-database-in-single-user-mode?rq=1
declare @createDbSql varchar(max);
set @createDbSql = N'

IF EXISTS (SELECT name FROM sys.databases WHERE name = N''' + @dbName + N''') 
ALTER DATABASE ' + @dbName  + ' SET MULTI_USER WITH ROLLBACK IMMEDIATE;

IF EXISTS (SELECT name FROM sys.databases WHERE name = N''' + @dbName + N''') 
ALTER DATABASE ' + @dbName  + ' SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

IF EXISTS (SELECT name FROM sys.databases WHERE name = N''' + @dbName + N''')
DROP DATABASE [' + @dbName + '];

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N''' + @dbName + N''') 
CREATE DATABASE [' + @dbName +'];'


EXEC(@createDbSql);
GO

--==========================
-- 2/2 CHANGE DB NAME HERE
--==========================
USE [{dbName}]
GO

-- Create EventStore schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'EventStore')
EXECUTE sp_executesql N'CREATE SCHEMA [EventStore] AUTHORIZATION [dbo]';

-- Create EventStore.Events
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[EventStore].[Events]') AND type in (N'U'))
CREATE TABLE [EventStore].[Events](
	[StreamType] [nvarchar](40) NOT NULL,
	[EventCollectionVersion] [bigint] NOT NULL,
	[StreamId] [uniqueidentifier] NOT NULL,
	[Version] [bigint] NOT NULL,
    [TransactionId] [uniqueidentifier] NOT NULL,
	[EventId] [uniqueidentifier] NOT NULL,
	[EventType] [nvarchar] (255) NOT NULL,
	[CorrelationId] [uniqueidentifier] NULL,
    [LocalTime] [datetime] NOT NULL,
	[UtcTime] [datetime] NOT NULL,
	[Payload] [nvarchar] (max) NOT NULL

PRIMARY KEY CLUSTERED 
(
	[StreamType] ASC,
    [EventCollectionVersion] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY];

CREATE NONCLUSTERED INDEX IX_EventStore_Rehydration  
    ON [EventStore].[Events] ([StreamType], [StreamId], [Version]);  

-- Create EventStore.Snapshots
IF NOT EXISTS(SELECT* FROM sys.objects WHERE object_id = OBJECT_ID(N'[EventStore].[Snapshots]') AND type in (N'U'))
CREATE TABLE[EventStore].[Snapshots](
	[StreamType] [nvarchar](40) NOT NULL,
    [StreamId] [uniqueidentifier] NOT NULL,
    [Version] [bigint] NOT NULL,
    [Payload] [nvarchar](max) NULL,
    [CreationLocalTime] [datetime] NOT NULL,
    [UpdateLocalTime] [datetime] NOT NULL
PRIMARY KEY CLUSTERED
(
	[StreamType] ASC,
    [StreamId] ASC
)WITH(PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON[PRIMARY]
) ON[PRIMARY];

-- Create EventStore.Subscriptions
IF NOT EXISTS(SELECT* FROM sys.objects WHERE object_id = OBJECT_ID(N'[EventStore].[SubscribedSources]') AND type in (N'U'))
CREATE TABLE[EventStore].[Subscriptions](
	[SubscriberStreamType] [nvarchar](40) NOT NULL,
	[StreamType] [nvarchar] (40) NOT NULL,
    [Url] [nvarchar] (2000) NOT NULL,
	[Token] [nvarchar] (max) NOT NULL,
    [ProcessorBufferVersion] [bigint] NOT NULL,
	[ProducerVersion] [bigint] NULL,
	[ConsistencyPercentage] [nvarchar] (10) NULL,
    [IsPoisoned] [bit] NOT NULL,
	[WasCanceled] [bit] NOT NULL,
    [PoisonEventCollectionVersion] [bigint] NULL,
    [DeadLetterPayload] [nvarchar] (max) NULL,
    [ExceptionMessage] [nvarchar] (max) NULL,
    [CreationLocalTime] [datetime] NOT NULL,
    [UpdateLocalTime] [datetime] NOT NULL
PRIMARY KEY CLUSTERED
(
	[SubscriberStreamType] ASC,
	[StreamType] ASC
)WITH(PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON[PRIMARY]
) ON[PRIMARY];

-- Create EventStore.Inbox
IF NOT EXISTS(SELECT* FROM sys.objects WHERE object_id = OBJECT_ID(N'[EventStore].[Inbox]') AND type in (N'U'))
CREATE TABLE[EventStore].[Inbox](
	[EventId] [uniqueidentifier] NOT NULL,
	[InboxStreamType] [nvarchar](40) NOT NULL,
    [TransactionId] [uniqueidentifier] NOT NULL,
	[StreamType] [nvarchar] (40) NULL,
    [StreamId] [uniqueidentifier] NULL,
    [Version] [bigint] NULL,
    [EventType] [nvarchar] (255) NULL,
    [EventCollectionVersion] [bigint] NULL,
    [CreationLocalTime] [datetime] NOT NULL,
	[Payload] [nvarchar] (max) NULL

PRIMARY KEY CLUSTERED
(
    [EventId] ASC
)WITH(PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON[PRIMARY]
) ON[PRIMARY];




