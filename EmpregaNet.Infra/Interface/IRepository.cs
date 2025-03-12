
using EmpregaNet.Domain.Interface;

namespace EmpregaNet.Infra.Interface
{
    public interface IRepository<T> : IDisposable where T : IAggregateRoot
    {
        IUnitOfWork UnitOfWork { get; }
    }
}