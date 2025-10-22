using Shared.Domain;

namespace Infra.Context.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    void Commit();
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
    void Rollback();
    IRepository<T> GetRepository<T>() where T : BaseEntity;
}