use InsertDbNameHere
go

begin transaction
delete EventStore.Subscriptions

-- app
insert into EventStore.Subscriptions
(
	SubscriberStreamType,
	StreamType,
	Url,
	Token,
	ProcessorBufferVersion,
	IsPoisoned,
	WasCanceled,
	CreationLocalTime,
	UpdateLocalTime
)
values
(
	N'StreamType',
	N'Domain.AggregateApp_20d1a331-3ca9-4f9c-bec0-739aea1cc3f1', -- The stream type with guid is important, to avoid collisions within event sources
	N'self', 
	N'self',
	0,
	0,
	0,
	getdate(),
	getdate()
)
if(@@error<>0) goto errorHandler

-- external node
insert into EventStore.Subscriptions
(
	StreamType,
	Url,
	Token,
	ProcessorBufferVersion,
	IsPoisoned,
	WasCanceled,
	CreationLocalTime,
	UpdateLocalTime
)
values
(
	N'Domain.AggregateApp_20d1a331-3ca9-4f9c-bec0-739aea1cc3f1', -- The stream type with guid is important, to avoid collisions within event sources
	N'http://192.168.0.0:80/eventsource/events', 
	N'insertTokenHere',
	0,
	0,
	0,
	getdate(),
	getdate()
)
if(@@error<>0) goto errorHandler

errorHandler: 
if(@@error<>0) rollback transaction
else commit transaction