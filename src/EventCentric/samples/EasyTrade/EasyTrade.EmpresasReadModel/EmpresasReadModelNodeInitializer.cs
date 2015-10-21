using EasyTrade.EmpresasReadModel.Dao;
using EventCentric;
using EventCentric.Config;
using Microsoft.Practices.Unity;
using System;

namespace EasyTrade.EmpresasReadModel
{
    public class EmpresasReadModelNodeInitializer : NodeInitializer
    {
        public static void Initialize(IUnityContainer container)
        {
            Initialize(
                () =>
                {
                    System.Data.Entity.Database.SetInitializer<EmpresasReadModelDbContext>(null);

                    var node = ProcessorNodeFactory<EmpresasQueueDenormalizer, EmpresasQueueProcessor>
                            .CreateDenormalizerNode<EmpresasReadModelDbContext>(container);

                    var config = container.Resolve<IEventStoreConfig>();
                    Func<EmpresasReadModelDbContext> contextFactory = () => new EmpresasReadModelDbContext(TimeSpan.FromSeconds(30), true, config.ConnectionString);
                    var dao = new EmpresasReadModelDao(contextFactory);

                    container.RegisterInstance<IEmpresasReadModelDao>(dao);

                    return node;
                });
        }
    }
}
