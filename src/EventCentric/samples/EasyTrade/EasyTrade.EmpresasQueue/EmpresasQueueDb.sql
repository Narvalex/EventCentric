USE [master]
GO

declare @dbName varchar(max);
--==========================
-- 1/2 CHANGE DB NAME HERE
--==========================
set @dbName = N'EmpresasQueue';

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
USE [EmpresasQueue]
GO

-- Create EventStore.Events
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'EventStore')
EXECUTE sp_executesql N'CREATE SCHEMA [EventStore] AUTHORIZATION [dbo]';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[EventStore].[Events]') AND type in (N'U'))
CREATE TABLE [EventStore].[Events](
    [StreamType] [nvarchar] (255) NOT NULL,
	[StreamId] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
    [TransactionId] [uniqueidentifier] NOT NULL,
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
) ON [PRIMARY];

-- Create EventStore.SubscriberHeartbeats
IF NOT EXISTS(SELECT* FROM sys.objects WHERE object_id = OBJECT_ID(N'[EventStore].[SubscriberHeartbeats]') AND type in (N'U'))
create table [EventStore].[SubscriberHeartbeats] (
    [SubscriberName] [nvarchar](128) not null,
    [Url] [nvarchar](max) null,
    [HeartbeatCount] [int] not null,
    [LastHeartbeatTime] [datetime] not null,
    [UpdateTime] [datetime] not null,
    [CreationDate] [datetime] not null,
    primary key ([SubscriberName])
);

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'SetValidation')
EXECUTE sp_executesql N'CREATE SCHEMA [SetValidation] AUTHORIZATION [dbo]';

-- Create SetValidation.Empresas
IF NOT EXISTS(SELECT* FROM sys.objects WHERE object_id = OBJECT_ID(N'[SetValidation].[Empresas]') AND type in (N'U'))
create table [SetValidation].[Empresas] (
    [IdEmpresa] [uniqueidentifier] not null,
    [Nombre] [nvarchar] (255) NOT NULL,
    primary key ([Nombre])
);