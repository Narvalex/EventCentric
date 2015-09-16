using EasyTrade.EmpresasQueue.Especificaciones.Excepciones;
using System;
using System.Linq;

namespace EasyTrade.EmpresasQueue.Especificaciones
{
    public class AlActualizarDatosDeEmpresa : AlTrabajarConEmpresaRegistrada
    {
        public static void ElNombreActualizadoDebeSerUnico(EmpresasQueueDbContext context, string nombreActualizado, Guid idEmpresa)
        {
            if (context.Empresas.Where(e => e.Nombre == nombreActualizado && e.IdEmpresa != idEmpresa).Any())
                throw new ElNombreDeEmpresaYaExiste(string.Format("Ya se registró una empresa llamada {0}", nombreActualizado));
        }
    }
}
