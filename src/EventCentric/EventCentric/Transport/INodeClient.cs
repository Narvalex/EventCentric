using System;

namespace EventCentric.Transport
{
    public interface INodeClient
    {
        TResponse Send<TRequest, TResponse>(string url, TRequest payload);
    }

    public interface IMultiNodeClient<TEnum> : INodeClient where TEnum : struct, IConvertible
    {
        TResponse Send<TRequest, TResponse>(TEnum node, string url, TRequest payload);
    }
}
