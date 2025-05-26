namespace EmpregaNet.Infra.Interface
{
    public interface IUnitOfWork
    {
        Task<bool> Commit();
    }
}