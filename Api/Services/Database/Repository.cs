using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Tavstal.MesterMC.Api.Services.Database.Interfaces;

namespace Tavstal.MesterMC.Api.Services.Database;

/// <inheritdoc />
public class Repository<T> : IRepository<T> where T : class
{
    /// <summary>
    /// The application database context used by the repository to access the database.
    /// </summary>
    protected readonly CustomDbContext _db;
    
    /// <summary>
    /// The EF Core <see cref="DbSet{T}"/> corresponding to the entity type <typeparamref name="T"/>.
    /// </summary>
    protected readonly DbSet<T> _set;

    /// <summary>
    /// Constructs a new repository instance bound to the provided <see cref="CustomDbContext"/>.
    /// After construction, <see cref="_set"/> is initialized to the context's set for <typeparamref name="T"/>.
    /// </summary>
    /// <param name="db">The database context to use. Typically provided by dependency injection.</param>
    public Repository(CustomDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    /// <inheritdoc />
    public async Task<T> AddAsync(T value, bool shouldSave = false)
    {
        var result = await _set.AddAsync(value);
        if (shouldSave)
            await _db.SaveChangesAsync();
        return result.Entity;
    }

    /// <inheritdoc />
    public async Task AddRangeAsync(IEnumerable<T> values, bool shouldSave = false)
    {
        await _set.AddRangeAsync(values);
        if (shouldSave)
            await _db.SaveChangesAsync();
    }
    
    /// <inheritdoc />
    public async Task<T> UpdateAsync(T value, bool shouldSave = false)
    {
        var result = _set.Update(value);
        if (shouldSave)
            await _db.SaveChangesAsync();
        return result.Entity;
    }

    /// <inheritdoc />
    public async Task RemoveAsync(T value, bool shouldSave = false)
    {
        _set.Remove(value);
        if (shouldSave)
            await _db.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RemoveRangeAsync(IEnumerable<T> values, bool shouldSave = false)
    {
        _set.RemoveRange(values);
        if (shouldSave)
            await _db.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>>? predicate)
    {
        if (predicate != null)
            return await _set.Where(predicate).ToListAsync();
        return await _set.ToListAsync();
    }

    /// <inheritdoc />
    public async Task<T?> FindAsync(Expression<Func<T, bool>>? predicate)
    {
        if (predicate != null)
            return await _set.FirstOrDefaultAsync(predicate);
        return await _set.FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _set.AnyAsync(predicate);
    }

    /// <inheritdoc />
    public async Task<T?> FindByIdAsync(object id)
    {
        return await _set.FindAsync(id);
    }
}