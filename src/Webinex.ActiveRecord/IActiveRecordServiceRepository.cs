using Webinex.Asky;

namespace Webinex.ActiveRecord;

public interface IActiveRecordServiceRepository<T>
{
    Task<IQueryable<T>> QueryableAsync(
        FilterRule? filterRule = null,
        IReadOnlyCollection<SortRule>? sortRules = null,
        PagingRule? pagingRule = null);

    Task<IReadOnlyCollection<T>> QueryAsync(
        FilterRule? filterRule = null,
        IReadOnlyCollection<SortRule>? sortRules = null,
        PagingRule? pagingRule = null);

    Task<IReadOnlyCollection<T>> ByKeysAsync<TKey>(IEnumerable<TKey> keys)
        where TKey : notnull;

    Task<IReadOnlyCollection<T>> AddRangeAsync(IEnumerable<T> entities);
}

public static class ActiveRecordServiceRepositoryExtensions
{
    public static async Task<T?> ByKeyAsync<T>(this IActiveRecordServiceRepository<T> repository, object key)
    {
        repository = repository ?? throw new ArgumentNullException(nameof(repository));
        key = key ?? throw new ArgumentNullException(nameof(key));
        var result = await repository.ByKeysAsync([key]);
        return result.FirstOrDefault();
    }

    public static async Task<T> AddAsync<T>(this IActiveRecordServiceRepository<T> repository, T entity)
    {
        repository = repository ?? throw new ArgumentNullException(nameof(repository));
        entity = entity ?? throw new ArgumentNullException(nameof(entity));
        var result = await repository.AddRangeAsync([entity]);
        return result.First();
    }
}