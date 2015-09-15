using EasyTrade.EmpresasQueue.Especificaciones.Excepciones;
using System;
using System.Linq;

namespace EasyTrade.EmpresasQueue.Especificaciones
{
    public class AlReactivarEmpresa
    {
        public static void EstaDebeHaberSidoRegistrada(EmpresasQueueDbContext context, Guid idEmpresa)
        {
            if (!context.Empresas.Any(e => e.IdEmpresa == idEmpresa))
                throw new LaEmpresaNoEstaRegistrada(string.Format("No está registrada la empresa id {0}", idEmpresa));
        }
    }
}
