using System;

namespace EasyTrade.EmpresasQueue.DTOs
{
    public class EmpresaDto
    {
        public Guid IdEmpresa { get; set; }
        public string Nombre { get; set; }
        public string Ruc { get; set; }
        public string Descripcion { get; set; }
    }
}
