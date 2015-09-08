using System;

namespace EasyTrade.Events.EmpresasQueue.DTOs
{
    public class Empresa
    {
        public Empresa(Guid idEmpresa, string nombre, string ruc, string descripcion)
        {
            this.IdEmpresa = idEmpresa;
            this.Nombre = nombre;
            this.Ruc = ruc;
            this.Descripcion = descripcion;
        }

        public Guid IdEmpresa { get; private set; }
        public string Nombre { get; private set; }
        public string Ruc { get; private set; }
        public string Descripcion { get; private set; }
    }
}
