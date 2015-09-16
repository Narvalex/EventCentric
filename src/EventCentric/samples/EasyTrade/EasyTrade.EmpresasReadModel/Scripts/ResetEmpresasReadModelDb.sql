use EmpresasReadModel
go

drop table EventStore.events
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

drop table EventStore.Streams 
CREATE TABLE [EventStore].[Streams](
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
) ON[PRIMARY];

drop table EventStore.Inbox
CREATE TABLE[EventStore].[Inbox](
	[InboxId] [bigint] IDENTITY(1,1) NOT NULL,
    [EventId] [uniqueidentifier] NOT NULL,
CONSTRAINT EventStore_Inbox_EventId UNIQUE(EventId),
    [TransactionId] [uniqueidentifier] NOT NULL,
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

drop table EventStore.EventuallyConsistentResults
create table [EventStore].[EventuallyConsistentResults] (
    [TransactionId] [uniqueidentifier] not null,
    [ResultType] [int] not null,
    [Message] [nvarchar](max) null,
    primary key ([TransactionId])
);

-- Create ReadModel.Empresas
drop table ReadModel.Empresas
create table [ReadModel].[Empresas](
    [IdEmpresa] [uniqueidentifier] not null,
	[Nombre] [nvarchar](255) null,
	[Ruc] [nvarchar](50) null,
	[Activada] [bit] not null,
	[Descripcion] [nvarchar](max) null,
	[FechaRegistro] [datetime] null,
	[FechaActualizacion] [datetime] null
primary key
(
    [IdEmpresa] ASC
))


update EventStore.Subscriptions set ProcessorBufferVersion = 0, IsPoisoned = 0