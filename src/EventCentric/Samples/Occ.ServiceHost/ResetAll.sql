use OccServer
go
truncate table EventStore.Events; truncate table EventStore.Snapshots; truncate table EventStore.Inbox;
update EventStore.Subscriptions set IsPoisoned = 0, DeadLetterPayload = '', ExceptionMessage = '', ProcessorBufferVersion = 0, PoisonEventCollectionVersion = 0, UpdateLocalTime = GETDATE();
go

use OccClient1
go
truncate table EventStore.Events; truncate table EventStore.Snapshots; truncate table EventStore.Inbox;
update EventStore.Subscriptions set IsPoisoned = 0, DeadLetterPayload = '', ExceptionMessage = '', ProcessorBufferVersion = 0, PoisonEventCollectionVersion = 0, UpdateLocalTime = GETDATE();

use OccClient2
go
truncate table EventStore.Events; truncate table EventStore.Snapshots; truncate table EventStore.Inbox;
update EventStore.Subscriptions set IsPoisoned = 0, DeadLetterPayload = '', ExceptionMessage = '', ProcessorBufferVersion = 0, PoisonEventCollectionVersion = 0, UpdateLocalTime = GETDATE();