namespace EmpregaNet.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        Task<bool> Commit();
    }
}