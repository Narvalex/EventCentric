using EventCentric;
using Microsoft.Practices.Unity;

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

                    return SagaNodeFactory<EmpresasQueueDenormalizer, EmpresasQueueProcessor>
                            .CreateDenormalizerNode<EmpresasReadModelDbContext>(container);
                });
        }
    }
}
