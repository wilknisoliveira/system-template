using Infra.Context.Repositories;
using Shared.Domain;

namespace Infra.Context.UnitOfWork;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private readonly Dictionary<Type, object> _repositories = new();
    private readonly ApplicationDbContext _context = context;

    public void Dispose()
    {
        _context.Dispose();
    }

    public void Commit()
    {
        _context.SaveChanges();
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Rollback()
    {
        //TODO: implement rollback
        throw new NotImplementedException();
    }

    public IRepository<T> GetRepository<T>() where T : BaseEntity
    {
        if (_repositories.ContainsKey(typeof(T)))
        {
            return (IRepository<T>)_repositories[typeof(T)];
        }

        var repository = new GenericRepository<T>(_context);
        _repositories.Add(typeof(T), repository);
        return repository;
    }
}