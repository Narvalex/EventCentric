use Domain@InsertHereNodeName
go

begin transaction
delete EventStore.SubscribersHeartbeats

-- StreamType
insert into EventStore.SubscribersHeartbeats
(
	 SubscriberName
    ,Url
    ,HeartbeatCount
    ,LastHeartbeatTime
    ,UpdateLocalTime
    ,CreationLocalTime
)
values
(
	N'StreamType_guid', -- The stream type with guid is important, to avoid collisions within event sources
	N'http://localhost:80/heartbeat',
	0,
	getdate(),
	getdate(),
	getdate()
)
if(@@error<>0) goto errorHandler

-- Another StreamType

errorHandler: 
if(@@error<>0) rollback transaction
else commit transaction