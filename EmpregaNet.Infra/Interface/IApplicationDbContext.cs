using EmpregaNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmpregaNet.Infra.Interface
{
    /// <summary>
    /// Represents the application database context interface.
    /// </summary>
    public interface IApplicationDbContext
    {
        /// <summary>
        /// Gets the DbSet of User entities.
        /// </summary>
        DbSet<User> Users { get; }

        /// <summary>
        /// Asynchronously saves all changes made in this context to the database.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
