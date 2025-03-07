using Webinex.Asky;

namespace Webinex.ActiveRecord;

public interface IActiveRecordRepository<T>
{
    Task<IQueryable<T>> QueryableAsync(ActiveRecordQuery? query = null);
    Task<IReadOnlyCollection<T>> QueryAsync(ActiveRecordQuery? query = null);
    Task<int> CountAsync(FilterRule? filterRule);

    Task<IReadOnlyCollection<T>> ByKeysAsync<TKey>(IEnumerable<TKey> keys)
        where TKey : notnull;

    Task<IReadOnlyDictionary<TKey, T>> MapAsync<TKey>(IEnumerable<TKey> keys)
        where TKey : notnull;

    Task<IReadOnlyCollection<T>> AddRangeAsync(IEnumerable<T> entities);

    Task RemoveRangeAsync(IEnumerable<T> entities);

    Task<bool> AnyAsync(FilterRule? filterRule = null);
}

public static class ActiveRecordRepositoryExtensions
{
    public static async Task<T?> ByKeyAsync<T>(this IActiveRecordRepository<T> repository, object key)
    {
        repository = repository ?? throw new ArgumentNullException(nameof(repository));
        key = key ?? throw new ArgumentNullException(nameof(key));
        var result = await repository.ByKeysAsync([key]);
        return result.FirstOrDefault();
    }

    public static async Task<T> AddAsync<T>(this IActiveRecordRepository<T> repository, T entity)
    {
        repository = repository ?? throw new ArgumentNullException(nameof(repository));
        entity = entity ?? throw new ArgumentNullException(nameof(entity));
        var result = await repository.AddRangeAsync([entity]);
        return result.First();
    }

    public static async Task RemoveAsync<T>(this IActiveRecordRepository<T> repository, T entity)
    {
        repository = repository ?? throw new ArgumentNullException(nameof(repository));
        entity = entity ?? throw new ArgumentNullException(nameof(entity));
        await repository.RemoveRangeAsync([entity]);
    }
}