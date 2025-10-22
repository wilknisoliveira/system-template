using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Infra.Context.Repositories;

internal class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }
    
    private bool Exists(Guid id)
    {
        return _dbSet.Any(g => g.Id.Equals(id));
    }
    
    private T FindById(Guid id)
    {
        return _dbSet.SingleOrDefault(g => g.Id.Equals(id));
    }

    public virtual async Task<T> CreateAsync(T item, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.AddAsync(item, cancellationToken);
            return item;
        }
        catch
        {
            throw;
        }
    }

    public virtual async Task<List<T>> CreateRangeAsync(List<T> items, CancellationToken cancellationToken)
    {
        try
        {
            await _context.AddRangeAsync(items, cancellationToken);
            return items;
        }
        catch
        {
            throw;
        }
    }

    public virtual async Task<List<T>> FindAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<T> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.SingleOrDefaultAsync(g => g.Id.Equals(id), cancellationToken);
    }

    public virtual async Task<int> GetCountAsync(string name, string column, string table, CancellationToken cancellationToken = default)
    {
        string query = $@"SELECT COUNT(*) FROM {table} t WHERE 1 = 1";
        if (!string.IsNullOrWhiteSpace(name)) query = query + $" AND t.{column} ILIKE '%{name}%' ";

        var result = "";
        using (var connection = _context.Database.GetDbConnection())
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = query;
                var response = await command.ExecuteScalarAsync();
                result = response.ToString();
            }
        }
        return int.Parse(result);
    }

    public virtual T Update(T item)
    {
        if (!Exists(item.Id)) return null;

        var result = FindById(item.Id);
        if (result != null)
        {
            try
            {
                _context.Entry(result).CurrentValues.SetValues(item);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
        else return null;
    }

    public virtual void Delete(Guid id)
    {
        var result = FindById(id);
        if (result != null)
        {
            try
            {
                _dbSet.Remove(result);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(g => g.Id.Equals(id), cancellationToken);
    }
}