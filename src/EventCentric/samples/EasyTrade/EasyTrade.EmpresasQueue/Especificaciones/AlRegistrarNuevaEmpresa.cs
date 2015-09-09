using EasyTrade.EmpresasQueue.Especificaciones.Excepciones;
using System.Linq;


namespace EasyTrade.EmpresasQueue.Especificaciones
{
    public class AlRegistrarNuevaEmpresa
    {
        public static void ElNombreDebeSerUnico(EmpresasQueueDbContext context, string nombreDeNuevaEmpresa)
        {
            if (context.Empresas.Where(e => e.Nombre == nombreDeNuevaEmpresa).Any())
                throw new ElNombreDeEmpresaYaExiste(string.Format("Ya se registró una empresa llamada {0}", nombreDeNuevaEmpresa));
        }
    }
}
