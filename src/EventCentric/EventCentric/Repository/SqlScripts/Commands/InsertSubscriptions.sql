use InsertDbNameHere
go

begin transaction
delete EventStore.Subscriptions
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
	N'EventSourceStreamTypeWithGuid', -- The stream type with guid is important, to avoid collisions within event sources
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