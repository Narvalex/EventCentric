use EmpresasReadModel
go

truncate table [EventStore].[Events];

truncate table EventStore.Streams ;

truncate table EventStore.Inbox;

truncate table EventStore.EventuallyConsistentResults;

-- Create ReadModel.Empresas
truncate table ReadModel.Empresas;

update EventStore.Subscriptions set ProcessorBufferVersion = 0, IsPoisoned = 0