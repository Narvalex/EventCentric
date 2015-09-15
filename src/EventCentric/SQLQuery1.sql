use EmpresasReadModel
go

select * from eventstore.subscriptions

update EventStore.Subscriptions set ProcessorBufferVersion = 0, IsPoisoned = 0