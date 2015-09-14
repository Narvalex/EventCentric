select * from EventStore.Subscriptions

{    
	"$type": "EventCentric.Messaging.PoisonMessageException, EventCentric",    
	"ClassName": "EventCentric.Messaging.PoisonMessageException",   
	 "Message": "Poison message detected in Event Processor",    
	 "Data": null,    
	 "InnerException": 
	 {      
		"$type": "EventCentric.EventSourcing.StreamNotFoundException, EventCentric",     
		"ClassName": "EventCentric.EventSourcing.StreamNotFoundException",      
		"Message": "EmpresasQueueDenormalizer: e61006a3-357d-4d8d-b1d3-a5130111e680",      
		"Data": null,      
		"InnerException": null,      
		"HelpURL": null,      
		"StackTraceString": "   
			en EventCentric.Processing.EventProcessor`1.HandleSafelyWithStreamLocking(Guid id, Action handle) en C:\\Users\\anarvaez\\Source\\Repos\\EventCentric\\src\\EventCentric\\EventCentric\\Processing\\EventProcessor.cs:línea 172\r\n   en EventCentric.Processing.EventProcessor`1.Handle(Guid id, IEvent incomingEvent) en C:\\Users\\anarvaez\\Source\\Repos\\EventCentric\\src\\EventCentric\\EventCentric\\Processing\\EventProcessor.cs:línea 119\r\n   en EasyTrade.EmpresasReadModel.EmpresasQueueProcessor.Handle(EmpresaDesactivada incomingEvent) en C:\\Users\\anarvaez\\Source\\Repos\\EventCentric\\src\\EventCentric\\samples\\EasyTrade\\EasyTrade.EmpresasReadModel\\EmpresasQueueProcessor.cs:línea 18\r\n   en CallSite.Target(Closure , CallSite , Object , Object )\r\n   en System.Dynamic.UpdateDelegates.UpdateAndExecuteVoid2[T0,T1](CallSite site, T0 arg0, T1 arg1)\r\n   en CallSite.Target(Closure , CallSite , Object , Object )\r\n   en EventCentric.Processing.EventProcessor`1.Handle(NewIncomingEvent message) en C:\\Users\\anarvaez\\Source\\Repos\\EventCentric\\src\\EventCentric\\EventCentric\\Processing\\EventProcessor.cs:línea 61",      "RemoteStackTraceString": null,      "RemoteStackIndex": 0,      "ExceptionMethod": "8\nHandleSafelyWithStreamLocking\nEventCentric, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\nEventCentric.Processing.EventProcessor`1\nVoid HandleSafelyWithStreamLocking(System.Guid, System.Action)",      "HResult": -2146233088,      "Source": "EventCentric",      "WatsonBuckets": null,      "entityId": "e61006a3-357d-4d8d-b1d3-a5130111e680",      "entityType": "EmpresasQueueDenormalizer"    },    "HelpURL": null,    "StackTraceString": null,    "RemoteStackTraceString": null,    "RemoteStackIndex": 0,    "ExceptionMethod": null,    "HResult": -2146233088,    "Source": null,    "WatsonBuckets": null  }


			{    "$type": "EasyTrade.Events.EmpresasQueue.EmpresaDesactivada, EasyTrade.Events",    "IdEmpresa": "e61006a3-357d-4d8d-b1d3-a5130111e680",    "EventCollectionVersion": 50,    "TransactionId": "55482ac1-e7aa-4707-9160-a5130111f4b6",    "EventId": "412fcf5f-a2ba-488a-aca4-a5130111f4b7",    "ProcessorBufferVersion": 1,    "StreamId": "e61006a3-357d-4d8d-b1d3-a5130111e680",    "StreamType": "EmpresasQueueApp",    "Version": 2  }


{    "$type": "EasyTrade.Events.EmpresasQueue.EmpresaDesactivada, EasyTrade.Events",   
 "IdEmpresa": "e61006a3-357d-4d8d-b1d3-a5130111e680",    
 "EventCollectionVersion": 50,    
 "TransactionId": "55482ac1-e7aa-4707-9160-a5130111f4b6",    "EventId": "412fcf5f-a2ba-488a-aca4-a5130111f4b7",    "ProcessorBufferVersion": 1,    "StreamId": "e61006a3-357d-4d8d-b1d3-a5130111e680",    "StreamType": "EmpresasQueueApp",    "Version": 2  }


 select * from EventStore.EVents where StreamId = 'e61006a3-357d-4d8d-b1d3-a5130111e680'

 use EmpresasQueue
 go

 14CD97CF-CCCF-4A83-8F65-A50F00F57296

 select * from EventStore.Events where StreamId = 'e61006a3-357d-4d8d-b1d3-a5130111e680'