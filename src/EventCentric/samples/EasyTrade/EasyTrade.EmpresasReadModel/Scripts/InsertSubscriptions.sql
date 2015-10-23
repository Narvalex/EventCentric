use EmpresasReadModel
go

-- EmpresasQueueApp
insert into EventStore.Subscriptions
(
	StreamType,
	Url,
	Token,
	ProcessorBufferVersion,
	IsPoisoned,
	WasCanceled,
	CreationDate,
	UpdateTime
)
values
(
	N'EmpresasQueueApp',
	N'http://172.16.251.125:82/eventsource/events', -- fecoprod laptop
	N'#easytrade',
	0,
	0,
	0,
	getdate(),
	getdate()
)