using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Webinex.Asky;

namespace Webinex.ActiveRecord;

internal class ActiveRecordRepository<T> : IActiveRecordInteractorRepository<T>
    where T : class
{
    private readonly IActiveRecordDbContextProvider _dbContextProvider;
    private readonly IActiveRecordSettings<T> _settings;

    public ActiveRecordRepository(
        IActiveRecordDbContextProvider dbContextProvider,
        IActiveRecordSettings<T> settings,
        IAskyFieldMap<T>? fieldMap = null)
    {
        _dbContextProvider = dbContextProvider;
        _settings = settings;
        FieldMap = fieldMap;
    }

    private ActiveRecordRepository(
        ActiveRecordRepository<T> repository,
        Expression<Func<T, bool>>? defaultPredicate)
        : this(
            repository._dbContextProvider,
            repository._settings,
            repository.FieldMap)
    {
        DefaultPredicate = defaultPredicate;
    }

    private DbContext DbContext => _dbContextProvider.Value;

    private IAskyFieldMap<T> FieldMap =>
        field ??
        throw new InvalidOperationException($"Field map for type {typeof(T).Name} not found in DI container.");

    private Expression<Func<T, bool>>? DefaultPredicate { get; }


    public async Task<ListSegment<T>> ListSegmentAsync(
        Query? query = null,
        bool includeTotal = true,
        bool readOnly = false)
    {
        var queryable = Queryable(query?.FilterRule, query?.SortRule);

        if (readOnly)
            queryable = queryable.AsNoTracking();

        if (query?.PagingRule == null)
        {
            var items = await queryable.ToArrayAsync();
            return new ListSegment<T>(items, includeTotal ? items.Length : -1);
        }

        var segment = await queryable.PageBy(query.PagingRule).ToArrayAsync();

        if (!includeTotal)
            return new ListSegment<T>(segment, -1);
        
        if (segment.Length < query.PagingRule.Take)
            return new ListSegment<T>(segment, query.PagingRule.Skip + segment.Length);

        var total = await queryable.CountAsync();
        return new ListSegment<T>(segment, total);
    }

    public async Task<int> CountAsync(FilterRule? filterRule = null)
    {
        return await Queryable(filterRule).CountAsync();
    }

    public async Task<bool> AnyAsync(FilterRule? filterRule = null)
    {
        return await Queryable(filterRule).AnyAsync();
    }

    public async Task<IReadOnlyCollection<T>> ByKeysAsync<TKey>(
        IEnumerable<TKey> keys,
        bool readOnly)
        where TKey : notnull
    {
        keys = keys?.Distinct().ToArray() ?? throw new ArgumentNullException(nameof(keys));
        if (!keys.Any()) return [];

        var queryable = Queryable();

        var expression = ActiveRecordExpression.KeyIn<T>(_settings, keys.Cast<object>());
        queryable = queryable.Where(expression);

        if (readOnly)
            queryable = queryable.AsNoTracking();

        return await queryable.ToArrayAsync();
    }

    public virtual async Task<IReadOnlyCollection<T>> AddRangeAsync(IEnumerable<T> entities)
    {
        entities = entities?.ToArray() ?? throw new ArgumentNullException(nameof(entities));
        await DbContext.Set<T>().AddRangeAsync(entities);
        return entities.ToArray();
    }

    public virtual Task RemoveRangeAsync(IEnumerable<T> entities)
    {
        entities = entities?.ToArray() ?? throw new ArgumentNullException(nameof(entities));
        DbContext.Set<T>().RemoveRange(entities);
        return Task.CompletedTask;
    }

    public IActiveRecordInteractorRepository<T> WithDefaultPredicate(Expression<Func<T, bool>>? predicate)
    {
        return new ActiveRecordRepository<T>(this, predicate);
    }

    private IQueryable<T> Queryable(
        FilterRule? filterRule = null,
        IEnumerable<SortRule>? sortRules = null,
        PagingRule? pagingRule = null)
    {
        var queryable = DbContext.Set<T>().AsQueryable();

        if (DefaultPredicate != null)
            queryable = queryable.Where(DefaultPredicate);

        if (filterRule != null)
            queryable = queryable.Where(FieldMap, filterRule);

        if (sortRules != null)
            queryable = queryable.SortBy(FieldMap, sortRules);

        if (pagingRule != null)
            queryable = queryable.PageBy(pagingRule);

        return queryable;
    }
}