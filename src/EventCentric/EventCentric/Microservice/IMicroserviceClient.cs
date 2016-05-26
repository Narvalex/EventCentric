using System;

namespace EventCentric
{
    public interface IMicroserviceClient
    {
        TResponse Send<TRequest, TResponse>(string url, TRequest payload);
    }

    public interface IMultiMicroserviceClient<TEnum> : IMicroserviceClient where TEnum : struct, IConvertible
    {
        TResponse Send<TRequest, TResponse>(TEnum node, string url, TRequest payload);
    }
}
