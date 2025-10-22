namespace Shared.Domain;

public interface IRepository<T> where T : BaseEntity
{
    Task<T> CreateAsync(T item, CancellationToken cancellationToken = default);
    Task<List<T>> CreateRangeAsync(List<T> items, CancellationToken cancellationToken);
    Task<List<T>> FindAllAsync(CancellationToken cancellationToken = default);
    Task<T> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(
        string name,
        string column,
        string table,
        CancellationToken cancellationToken = default);
    T Update(T item);
    void Delete(Guid id);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}