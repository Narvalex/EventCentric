using EasyTrade.EmpresasQueue;
using EventCentric;
using EventCentric.Log;
using EventCentric.Queueing;
using EventCentric.Utils;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EasyTrade.Tests
{
    [TestClass]
    public class EmpresasQueueFixture
    {
        protected EmpresasQueueApp app;
        public EmpresasQueueFixture()
        {
            var container = new UnityContainer();
            var node = QueueNodeFactory<EmpresasQueueApp>.CreateCrudNode<EmpresasQueueDbContext>(container);
            System.Data.Entity.Database.SetInitializer<EmpresasQueueDbContext>(null);
            this.app = new EmpresasQueueApp(container.Resolve<ICrudEventQueue>(), container.Resolve<IGuidProvider>(), container.Resolve<ITimeProvider>());
        }

        //[TestMethod]
        //public void CAN_publish_lots_of_messages()
        //{
        //    for (int i = 0; i < 100; i++)
        //    {
        //        Task.Factory.StartNewLongRunning(() =>
        //        {
        //            var id = this.app.NuevaEmpresa(new NuevaEmpresaDto() { Nombre = $"Empresa {i}", Ruc = "-", Descripcion = "-" });
        //            Task.Factory.StartNewLongRunning(() =>
        //            {
        //                for (int j = 0; j < 100; j++)
        //                {
        //                    Thread.Sleep(10);
        //                    this.app.ActualizarEmpresa(new EmpresaDto() { IdEmpresa = id, Nombre = $"Empresa {i} actualizada", Ruc = "-", Descripcion = "-" });
        //                    this.app.DesactivarEmpresa(id);
        //                    this.app.ReactivarEmpresa(id);
        //                }
        //            });
        //        });
        //    }

        //    Thread.Sleep(10000);
        //}
    }

    public class FakeLogger : ILogger
    {
        public void Error(string format, params object[] args)
        {

        }

        public void Error(Exception ex, string format, params object[] args)
        {

        }

        public void Trace(params string[] text)
        {

        }

        public void Trace(string format, params object[] args)
        {

        }
    }
}
