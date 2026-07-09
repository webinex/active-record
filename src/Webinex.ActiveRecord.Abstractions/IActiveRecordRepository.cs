using Webinex.Asky;

namespace Webinex.ActiveRecord;

public interface IActiveRecordRepository<TType>
{
    Task<ListSegment<TType>> ListSegmentAsync(Query? query = null, bool includeTotal = true, bool readOnly = false);
    Task<int> CountAsync(FilterRule? filterRule);

    Task<IReadOnlyCollection<TType>> ByKeysAsync<TKey>(IEnumerable<TKey> keys, bool readOnly = false)
        where TKey : notnull;

    Task<IReadOnlyCollection<TType>> AddRangeAsync(IEnumerable<TType> entities);

    Task RemoveRangeAsync(IEnumerable<TType> entities);

    Task<bool> AnyAsync(FilterRule? filterRule = null);
}

public static class ActiveRecordRepositoryExtensions
{
    public static async Task<TType?> ByKeyAsync<TType, TKey>(this IActiveRecordRepository<TType> repository, TKey key)
    {
        repository = repository ?? throw new ArgumentNullException(nameof(repository));
        key = key ?? throw new ArgumentNullException(nameof(key));
        var result = await repository.ByKeysAsync([key]);
        return result.FirstOrDefault();
    }

    public static async Task<TType?> ByKeyAsync<TType>(this IActiveRecordRepository<TType> repository, object key)
    {
        repository = repository ?? throw new ArgumentNullException(nameof(repository));
        key = key ?? throw new ArgumentNullException(nameof(key));
        var result = await repository.ByKeysAsync([key]);
        return result.FirstOrDefault();
    }

    public static async Task<IReadOnlyCollection<TType>> GetAllAsync<TType>(
        this IActiveRecordRepository<TType> repository,
        Query? query = null,
        bool readOnly = false)
    {
        var result = await repository.ListSegmentAsync(query, includeTotal: false, readOnly: readOnly);
        return result.Items;
    }

    public static async Task<IReadOnlyCollection<TType>> GetAllAsync<TType>(
        this IActiveRecordRepository<TType> repository,
        FilterRule? filterRule = null,
        IEnumerable<SortRule>? sortRules = null,
        PagingRule? pagingRule = null,
        bool readOnly = false)
    {
        var query = new Query(filterRule, sortRules?.ToArray(), pagingRule);
        return await repository.GetAllAsync(query, readOnly);
    }

    public static async Task<TType?> FirstOrDefaultAsync<TType>(
        this IActiveRecordRepository<TType> repository,
        FilterRule? filterRule = null,
        IEnumerable<SortRule>? sortRules = null,
        bool readOnly = false)
    {
        var query = new Query(filterRule, sortRules?.ToArray(), PagingRule.TakeFirst(1));
        var result = await repository.GetAllAsync(query, readOnly);
        return result.FirstOrDefault();
    }

    public static async Task<TType?> FirstAsync<TType>(
        this IActiveRecordRepository<TType> repository,
        FilterRule? filterRule = null,
        IEnumerable<SortRule>? sortRules = null,
        bool readOnly = false)
    {
        var query = new Query(filterRule, sortRules?.ToArray(), PagingRule.TakeFirst(1));
        var result = await repository.GetAllAsync(query, readOnly);
        return result.First();
    }

    public static async Task<TType?> SingleOrDefaultAsync<TType>(
        this IActiveRecordRepository<TType> repository,
        FilterRule? filterRule = null,
        IEnumerable<SortRule>? sortRules = null,
        bool readOnly = false)
    {
        var query = new Query(filterRule, sortRules?.ToArray(), PagingRule.TakeFirst(1));
        var result = await repository.GetAllAsync(query, readOnly);
        return result.SingleOrDefault();
    }

    public static async Task<TType?> SingleAsync<TType>(
        this IActiveRecordRepository<TType> repository,
        FilterRule? filterRule = null,
        IEnumerable<SortRule>? sortRules = null,
        bool readOnly = false)
    {
        var query = new Query(filterRule, sortRules?.ToArray(), PagingRule.TakeFirst(1));
        var result = await repository.GetAllAsync(query, readOnly);
        return result.Single();
    }

    public static async Task<TType> AddAsync<TType>(this IActiveRecordRepository<TType> repository, TType entity)
    {
        repository = repository ?? throw new ArgumentNullException(nameof(repository));
        entity = entity ?? throw new ArgumentNullException(nameof(entity));
        var result = await repository.AddRangeAsync([entity]);
        return result.First();
    }

    public static async Task RemoveAsync<TType>(this IActiveRecordRepository<TType> repository, TType entity)
    {
        repository = repository ?? throw new ArgumentNullException(nameof(repository));
        entity = entity ?? throw new ArgumentNullException(nameof(entity));
        await repository.RemoveRangeAsync([entity]);
    }
}