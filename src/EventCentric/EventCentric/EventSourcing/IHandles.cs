namespace EventCentric.EventSourcing
{
    public interface IHandles { }

    public interface IHandles<T> : IHandles where T : IEvent
    {
        void Handle(T message);
    }

    public interface IHandles<TMessage, TDomainService> : IHandles
        where TMessage : IEvent
        where TDomainService : IDomainService
    {
        void Handle(TMessage message, TDomainService service);
    }

    public interface IHandles<TMessage, TDomainService1, TDomainService2> : IHandles
        where TMessage : IEvent
        where TDomainService1 : IDomainService
        where TDomainService2 : IDomainService
    {
        void Handle(TMessage message, TDomainService1 service1, TDomainService2 service2);
    }

    public interface IHandles<TMessage, TDomainService1, TDomainService2, TDomainService3> : IHandles
        where TMessage : IEvent
        where TDomainService1 : IDomainService
        where TDomainService2 : IDomainService
        where TDomainService3 : IDomainService
    {
        void Handle(TMessage message, TDomainService1 service1, TDomainService2 service2, TDomainService3 service3);
    }

    public interface IHandles<TMessage, TDomainService1, TDomainService2, TDomainService3, TDomainService4> : IHandles
        where TMessage : IEvent
        where TDomainService1 : IDomainService
        where TDomainService2 : IDomainService
        where TDomainService3 : IDomainService
        where TDomainService4 : IDomainService
    {
        void Handle(TMessage message, TDomainService1 service1, TDomainService2 service2, TDomainService3 service3, TDomainService4 service4);
    }

    public interface IHandles<TMessage, TDomainService1, TDomainService2, TDomainService3, TDomainService4, TDomainService5> : IHandles
        where TMessage : IEvent
        where TDomainService1 : IDomainService
        where TDomainService2 : IDomainService
        where TDomainService3 : IDomainService
        where TDomainService4 : IDomainService
        where TDomainService5 : IDomainService
    {
        void Handle(TMessage message, TDomainService1 service1, TDomainService2 service2, TDomainService3 service3, TDomainService4 service4, TDomainService5 service5);
    }

    public interface IHandles<TMessage, TDomainService1, TDomainService2, TDomainService3, TDomainService4, TDomainService5, TDomainService6> : IHandles
        where TMessage : IEvent
        where TDomainService1 : IDomainService
        where TDomainService2 : IDomainService
        where TDomainService3 : IDomainService
        where TDomainService4 : IDomainService
        where TDomainService5 : IDomainService
        where TDomainService6 : IDomainService
    {
        void Handle(TMessage message, TDomainService1 service1, TDomainService2 service2, TDomainService3 service3, TDomainService4 service4, TDomainService5 service5, TDomainService6 service6);
    }

    public interface IHandles<TMessage, TDomainService1, TDomainService2, TDomainService3, TDomainService4, TDomainService5, TDomainService6, TDomainService7> : IHandles
        where TMessage : IEvent
        where TDomainService1 : IDomainService
        where TDomainService2 : IDomainService
        where TDomainService3 : IDomainService
        where TDomainService4 : IDomainService
        where TDomainService5 : IDomainService
        where TDomainService6 : IDomainService
        where TDomainService7 : IDomainService
    {
        void Handle(TMessage message, TDomainService1 service1, TDomainService2 service2, TDomainService3 service3, TDomainService4 service4, TDomainService5 service5, TDomainService6 service6, TDomainService7 service7);
    }

    public interface IHandles<TMessage, TDomainService1, TDomainService2, TDomainService3, TDomainService4, TDomainService5, TDomainService6, TDomainService7, TDomainService8> : IHandles
        where TMessage : IEvent
        where TDomainService1 : IDomainService
        where TDomainService2 : IDomainService
        where TDomainService3 : IDomainService
        where TDomainService4 : IDomainService
        where TDomainService5 : IDomainService
        where TDomainService6 : IDomainService
        where TDomainService7 : IDomainService
        where TDomainService8 : IDomainService
    {
        void Handle(TMessage message, TDomainService1 service1, TDomainService2 service2, TDomainService3 service3, TDomainService4 service4, TDomainService5 service5, TDomainService6 service6, TDomainService7 service7, TDomainService8 service8);
    }

    public interface IHandles<TMessage, TDomainService1, TDomainService2, TDomainService3, TDomainService4, TDomainService5, TDomainService6, TDomainService7, TDomainService8, TDomainService9> : IHandles
        where TMessage : IEvent
        where TDomainService1 : IDomainService
        where TDomainService2 : IDomainService
        where TDomainService3 : IDomainService
        where TDomainService4 : IDomainService
        where TDomainService5 : IDomainService
        where TDomainService6 : IDomainService
        where TDomainService7 : IDomainService
        where TDomainService8 : IDomainService
        where TDomainService9 : IDomainService
    {
        void Handle(TMessage message, TDomainService1 service1, TDomainService2 service2, TDomainService3 service3, TDomainService4 service4, TDomainService5 service5, TDomainService6 service6, TDomainService7 service7, TDomainService8 service8, TDomainService9 service9);
    }

    public interface IHandles<TMessage, TDomainService1, TDomainService2, TDomainService3, TDomainService4, TDomainService5, TDomainService6, TDomainService7, TDomainService8, TDomainService9, TDomainService10> : IHandles
        where TMessage : IEvent
        where TDomainService1 : IDomainService
        where TDomainService2 : IDomainService
        where TDomainService3 : IDomainService
        where TDomainService4 : IDomainService
        where TDomainService5 : IDomainService
        where TDomainService6 : IDomainService
        where TDomainService7 : IDomainService
        where TDomainService8 : IDomainService
        where TDomainService9 : IDomainService
        where TDomainService10 : IDomainService
    {
        void Handle(TMessage message, TDomainService1 service1, TDomainService2 service2, TDomainService3 service3, TDomainService4 service4, TDomainService5 service5, TDomainService6 service6, TDomainService7 service7, TDomainService8 service8, TDomainService9 service9, TDomainService10 service10);
    }

    public interface IHandles<TMessage, TDomainService1, TDomainService2, TDomainService3, TDomainService4, TDomainService5, TDomainService6, TDomainService7, TDomainService8, TDomainService9, TDomainService10, TDomainService11> : IHandles
       where TMessage : IEvent
       where TDomainService1 : IDomainService
       where TDomainService2 : IDomainService
       where TDomainService3 : IDomainService
       where TDomainService4 : IDomainService
       where TDomainService5 : IDomainService
       where TDomainService6 : IDomainService
       where TDomainService7 : IDomainService
       where TDomainService8 : IDomainService
       where TDomainService9 : IDomainService
       where TDomainService10 : IDomainService
       where TDomainService11 : IDomainService
    {
        void Handle(TMessage message, TDomainService1 service1, TDomainService2 service2, TDomainService3 service3, TDomainService4 service4, TDomainService5 service5, TDomainService6 service6, TDomainService7 service7, TDomainService8 service8, TDomainService9 service9, TDomainService10 service10, TDomainService11 service11);
    }

    public interface IHandles<TMessage, TDomainService1, TDomainService2, TDomainService3, TDomainService4, TDomainService5, TDomainService6, TDomainService7, TDomainService8, TDomainService9, TDomainService10, TDomainService11, TDomainService12> : IHandles
       where TMessage : IEvent
       where TDomainService1 : IDomainService
       where TDomainService2 : IDomainService
       where TDomainService3 : IDomainService
       where TDomainService4 : IDomainService
       where TDomainService5 : IDomainService
       where TDomainService6 : IDomainService
       where TDomainService7 : IDomainService
       where TDomainService8 : IDomainService
       where TDomainService9 : IDomainService
       where TDomainService10 : IDomainService
       where TDomainService11 : IDomainService
       where TDomainService12 : IDomainService
    {
        void Handle(TMessage message, TDomainService1 service1, TDomainService2 service2, TDomainService3 service3, TDomainService4 service4, TDomainService5 service5, TDomainService6 service6, TDomainService7 service7, TDomainService8 service8, TDomainService9 service9, TDomainService10 service10, TDomainService11 service11, TDomainService12 service12);
    }

    public interface IHandles<TMessage, TDomainService1, TDomainService2, TDomainService3, TDomainService4, TDomainService5, TDomainService6, TDomainService7, TDomainService8, TDomainService9, TDomainService10, TDomainService11, TDomainService12, TDomainService13> : IHandles
       where TMessage : IEvent
       where TDomainService1 : IDomainService
       where TDomainService2 : IDomainService
       where TDomainService3 : IDomainService
       where TDomainService4 : IDomainService
       where TDomainService5 : IDomainService
       where TDomainService6 : IDomainService
       where TDomainService7 : IDomainService
       where TDomainService8 : IDomainService
       where TDomainService9 : IDomainService
       where TDomainService10 : IDomainService
       where TDomainService11 : IDomainService
       where TDomainService12 : IDomainService
       where TDomainService13 : IDomainService
    {
        void Handle(TMessage message, TDomainService1 service1, TDomainService2 service2, TDomainService3 service3, TDomainService4 service4, TDomainService5 service5, TDomainService6 service6, TDomainService7 service7, TDomainService8 service8, TDomainService9 service9, TDomainService10 service10, TDomainService11 service11, TDomainService12 service12, TDomainService13 service13);
    }

    public interface IHandles<TMessage, TDomainService1, TDomainService2, TDomainService3, TDomainService4, TDomainService5, TDomainService6, TDomainService7, TDomainService8, TDomainService9, TDomainService10, TDomainService11, TDomainService12, TDomainService13, TDomainService14> : IHandles
       where TMessage : IEvent
       where TDomainService1 : IDomainService
       where TDomainService2 : IDomainService
       where TDomainService3 : IDomainService
       where TDomainService4 : IDomainService
       where TDomainService5 : IDomainService
       where TDomainService6 : IDomainService
       where TDomainService7 : IDomainService
       where TDomainService8 : IDomainService
       where TDomainService9 : IDomainService
       where TDomainService10 : IDomainService
       where TDomainService11 : IDomainService
       where TDomainService12 : IDomainService
       where TDomainService13 : IDomainService
       where TDomainService14 : IDomainService
    {
        void Handle(TMessage message, TDomainService1 service1, TDomainService2 service2, TDomainService3 service3, TDomainService4 service4, TDomainService5 service5, TDomainService6 service6, TDomainService7 service7, TDomainService8 service8, TDomainService9 service9, TDomainService10 service10, TDomainService11 service11, TDomainService12 service12, TDomainService13 service13, TDomainService14 service14);
    }

    public interface IHandles<TMessage, TDomainService1, TDomainService2, TDomainService3, TDomainService4, TDomainService5, TDomainService6, TDomainService7, TDomainService8, TDomainService9, TDomainService10, TDomainService11, TDomainService12, TDomainService13, TDomainService14, TDomainService15> : IHandles
       where TMessage : IEvent
       where TDomainService1 : IDomainService
       where TDomainService2 : IDomainService
       where TDomainService3 : IDomainService
       where TDomainService4 : IDomainService
       where TDomainService5 : IDomainService
       where TDomainService6 : IDomainService
       where TDomainService7 : IDomainService
       where TDomainService8 : IDomainService
       where TDomainService9 : IDomainService
       where TDomainService10 : IDomainService
       where TDomainService11 : IDomainService
       where TDomainService12 : IDomainService
       where TDomainService13 : IDomainService
       where TDomainService14 : IDomainService
       where TDomainService15 : IDomainService
    {
        void Handle(TMessage message, TDomainService1 service1, TDomainService2 service2, TDomainService3 service3, TDomainService4 service4, TDomainService5 service5, TDomainService6 service6, TDomainService7 service7, TDomainService8 service8, TDomainService9 service9, TDomainService10 service10, TDomainService11 service11, TDomainService12 service12, TDomainService13 service13, TDomainService14 service14, TDomainService15 service15);
    }

    public interface IHandles<TMessage, TDomainService1, TDomainService2, TDomainService3, TDomainService4, TDomainService5, TDomainService6, TDomainService7, TDomainService8, TDomainService9, TDomainService10, TDomainService11, TDomainService12, TDomainService13, TDomainService14, TDomainService15, TDomainService16> : IHandles
       where TMessage : IEvent
       where TDomainService1 : IDomainService
       where TDomainService2 : IDomainService
       where TDomainService3 : IDomainService
       where TDomainService4 : IDomainService
       where TDomainService5 : IDomainService
       where TDomainService6 : IDomainService
       where TDomainService7 : IDomainService
       where TDomainService8 : IDomainService
       where TDomainService9 : IDomainService
       where TDomainService10 : IDomainService
       where TDomainService11 : IDomainService
       where TDomainService12 : IDomainService
       where TDomainService13 : IDomainService
       where TDomainService14 : IDomainService
       where TDomainService15 : IDomainService
       where TDomainService16 : IDomainService
    {
        void Handle(TMessage message, TDomainService1 service1, TDomainService2 service2, TDomainService3 service3, TDomainService4 service4, TDomainService5 service5, TDomainService6 service6, TDomainService7 service7, TDomainService8 service8, TDomainService9 service9, TDomainService10 service10, TDomainService11 service11, TDomainService12 service12, TDomainService13 service13, TDomainService14 service14, TDomainService15 service15, TDomainService16 service16);
    }
}
