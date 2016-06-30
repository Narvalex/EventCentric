CREATE TABLE [Events](
	[StreamType] [nvarchar](255) NOT NULL,
	[StreamId] [uniqueidentifier] NOT NULL,
	[Version] [bigint] NOT NULL,
    [TransactionId] [uniqueidentifier] NOT NULL,
	[EventId] [uniqueidentifier] NOT NULL,
	CONSTRAINT EventStore_Events_EventId UNIQUE(EventId),
	[EventType] [nvarchar] (255) NOT NULL,
	[CorrelationId] [uniqueidentifier] NULL,
    [EventCollectionVersion] [bigint] NOT NULL,
    [LocalTime] [datetime] NOT NULL,
	[UtcTime] [datetime] NOT NULL,
	[RowVersion] [rowversion] NOT NULL,
	[Payload] [ntext] NOT NULL
);
GO 
ALTER TABLE [Events] ADD CONSTRAINT [PK_Events] PRIMARY KEY ([StreamType], [StreamId], [Version]);
GO


CREATE TABLE [Snapshots](
	[StreamType] [nvarchar](255) NOT NULL,
    [StreamId] [uniqueidentifier] NOT NULL,
    [Version] [bigint] NOT NULL,
    [Payload] [ntext] NULL,
    [CreationLocalTime] [datetime] NOT NULL,
    [UpdateLocalTime] [datetime] NOT NULL
);
GO
ALTER TABLE [Snapshots] ADD CONSTRAINT [PK_Snapshots] PRIMARY KEY ([StreamType], [StreamId]);
GO


CREATE TABLE [Subscriptions](
	[SubscriberStreamType] [nvarchar](128) NOT NULL,
	[StreamType] [nvarchar] (255) NOT NULL,
    [Url] [nvarchar] (500) NOT NULL,
	[Token] [ntext] NOT NULL,
    [ProcessorBufferVersion] [bigint] NOT NULL,
	[ProducerVersion] [bigint] NULL,
	[ConsistencyPercentage] [nvarchar] (10) NULL,
    [IsPoisoned] [bit] NOT NULL,
	[WasCanceled] [bit] NOT NULL,
    [PoisonEventCollectionVersion] [bigint] NULL,
    [DeadLetterPayload] [ntext] NULL,
    [ExceptionMessage] [ntext] NULL,
    [CreationLocalTime] [datetime] NOT NULL,
    [UpdateLocalTime] [datetime] NOT NULL
);
GO
ALTER TABLE [Subscriptions] ADD CONSTRAINT [PK_Subscriptions] PRIMARY KEY ([SubscriberStreamType], [StreamType]);
GO


CREATE TABLE [Inbox](
	[InboxId] [bigint] IDENTITY(1,1) NOT NULL,
	[InboxStreamType] [nvarchar](128) NOT NULL,
    [EventId] [uniqueidentifier] NOT NULL,
    [TransactionId] [uniqueidentifier] NOT NULL,
	[StreamType] [nvarchar] (255) NOT NULL,
    [StreamId] [uniqueidentifier] NOT NULL,
    [Version] [bigint] NOT NULL,
    [EventType] [nvarchar] (255) NOT NULL,
    [EventCollectionVersion] [bigint] NOT NULL,
    [Ignored] [bit] NULL,
    [CreationLocalTime] [datetime] NOT NULL,
	[Payload] [ntext] NOT NULL
);
GO
ALTER TABLE [Inbox] ADD CONSTRAINT [PK_Inbox] PRIMARY KEY ([InboxId]);
GO
CREATE INDEX [IDX_EventId] ON [Inbox] ([EventId] ASC);
GO
ALTER TABLE [Inbox] ADD CONSTRAINT [Unique_EventId] UNIQUE ([EventId]);
GO
