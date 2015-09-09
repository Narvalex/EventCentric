using System;
using System.Runtime.Serialization;

namespace EasyTrade.EmpresasQueue.Especificaciones.Excepciones
{
    [Serializable]
    public class ElNombreDeEmpresaYaExiste : Exception
    {
        public ElNombreDeEmpresaYaExiste() { }

        public ElNombreDeEmpresaYaExiste(string message) : base(message) { }

        public ElNombreDeEmpresaYaExiste(string message, Exception inner) : base(message, inner) { }

        protected ElNombreDeEmpresaYaExiste(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
