using System;
using System.Runtime.Serialization;

namespace EasyTrade.EmpresasQueue.Especificaciones.Excepciones
{
    [Serializable]
    public class LaEmpresaNoEstaRegistrada : Exception
    {
        public LaEmpresaNoEstaRegistrada() { }

        public LaEmpresaNoEstaRegistrada(string message) : base(message) { }

        public LaEmpresaNoEstaRegistrada(string message, Exception inner) : base(message, inner) { }

        protected LaEmpresaNoEstaRegistrada(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
