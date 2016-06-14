-- Add event store first tables

-- Create EventStore.EventuallyConsistentResults
create table [EventStore].[EventuallyConsistentResults] (
	[Id] [bigint] IDENTITY(1,1) not null,
    [TransactionId] [uniqueidentifier] not null,
    [ResultType] [int] not null,
    [Message] [nvarchar](max) null,
    primary key ([Id])
);

CREATE NONCLUSTERED INDEX IX_EventStore_EventuallyConsistentResults 
    ON [EventStore].[EventuallyConsistentResults] ([TransactionId]);  

-- CREATE YOUR VIEW TABLES HERE



