USE [master]
GO


declare @dbName varchar(max);
--==========================
-- 1/2 CHANGE DB NAME HERE
--==========================
set @dbName = N'InsertDbNameHere';


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
USE [InsertDbNameHere]
GO

-- Create EventStore schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'EventStore')
EXECUTE sp_executesql N'CREATE SCHEMA [EventStore] AUTHORIZATION [dbo]';

-- Create EventStore.Events
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[EventStore].[Events]') AND type in (N'U'))
CREATE TABLE [EventStore].[Events](
	[StreamId] [uniqueidentifier] NOT NULL,
	[Version] [bigint] NOT NULL,
    [TransactionId] [uniqueidentifier] NOT NULL,
	[EventId] [uniqueidentifier] NOT NULL,
	CONSTRAINT EventStore_Events_EventId UNIQUE(EventId),
	[EventType] [nvarchar] (255) NOT NULL,
	[CorrelationId] [uniqueidentifier] NULL,
    [EventCollectionVersion] [bigint] IDENTITY(1,1) NOT NULL,
	CONSTRAINT EventStore_Events_EventCollectionVersion UNIQUE(EventCollectionVersion),
    [LocalTime] [datetime] NOT NULL,
	[UtcTime] [datetime] NOT NULL,
	[RowVersion] [rowversion] NOT NULL,
	[Payload] [nvarchar] (max) NOT NULL

PRIMARY KEY CLUSTERED 
(
	[StreamId] ASC, 
    [Version] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY];


-- Create EventStore.SubscribersHeartbeats
IF NOT EXISTS(SELECT* FROM sys.objects WHERE object_id = OBJECT_ID(N'[EventStore].[SubscriberHeartbeats]') AND type in (N'U'))
create table [EventStore].[SubscribersHeartbeats] (
    [SubscriberName] [nvarchar](128) not null,
    [Url] [nvarchar](max) null,
    [HeartbeatCount] [bigint] not null,
    [LastHeartbeatTime] [datetime] not null,
    [UpdateLocalTime] [datetime] not null,
    [CreationLocalTime] [datetime] not null,
    primary key ([SubscriberName])
);


