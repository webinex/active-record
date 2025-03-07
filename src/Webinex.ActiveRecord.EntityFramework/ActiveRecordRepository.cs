using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Webinex.ActiveRecord.Annotations;
using Webinex.Asky;

namespace Webinex.ActiveRecord;

internal class ActiveRecordRepository<T> : IActiveRecordRepository<T>, IActiveRecordServiceRepository<T>
    where T : class
{
    private readonly IActiveRecordDbContextProvider _dbContextProvider;
    private readonly IAskyFieldMap<T>? _fieldMap;
    private readonly IActiveRecordSettings<T> _settings;
    private readonly IActiveRecordAuthorizationService<T> _authorizationService;

    public ActiveRecordRepository(
        IActiveRecordDbContextProvider dbContextProvider,
        IActiveRecordSettings<T> settings,
        IActiveRecordAuthorizationService<T> authorizationService,
        IAskyFieldMap<T>? fieldMap = null)
    {
        _dbContextProvider = dbContextProvider;
        _settings = settings;
        _authorizationService = authorizationService;
        _fieldMap = fieldMap;
    }

    private DbContext DbContext => _dbContextProvider.Value;

    private IAskyFieldMap<T> FieldMap =>
        _fieldMap ??
        throw new InvalidOperationException($"Field map for type {typeof(T).Name} not found in DI container.");

    Task<IQueryable<T>> IActiveRecordRepository<T>.QueryableAsync(ActiveRecordQuery? query)
    {
        return QueryableAsync(defaultPredicate: null, query);
    }

    async Task<IQueryable<T>> IActiveRecordServiceRepository<T>.QueryableAsync(ActiveRecordQuery? query)
    {
        var context = new ActionContext<T>(ActionType.GetAll, _settings.Definition, null, null, null);
        var defaultPredicate = await _authorizationService.ExpressionAsync(context);
        var queryable = await QueryableAsync(defaultPredicate, query);
        return queryable.AsNoTracking();
    }

    async Task<IReadOnlyCollection<T>> IActiveRecordRepository<T>.QueryAsync(ActiveRecordQuery? query)
    {
        var queryable = await QueryableAsync(defaultPredicate: null, query);
        return await queryable.ToArrayAsync();
    }

    async Task<IReadOnlyCollection<T>> IActiveRecordServiceRepository<T>.QueryAsync(ActiveRecordQuery? query)
    {
        var context = new ActionContext<T>(ActionType.GetAll, _settings.Definition, null, null, null);
        var defaultPredicate = await _authorizationService.ExpressionAsync(context);
        var queryable = await QueryableAsync(defaultPredicate, query);
        return await queryable.AsNoTracking().ToArrayAsync();
    }

    private Task<IQueryable<T>> QueryableAsync(
        Expression<Func<T, bool>>? defaultPredicate,
        ActiveRecordQuery? query)
    {
        var queryable = DbContext.Set<T>().AsQueryable();

        if (defaultPredicate != null)
            queryable = queryable.Where(defaultPredicate);

        if (query?.FilterRule != null)
            queryable = queryable.Where(FieldMap, query.FilterRule);

        if (query?.SortRules != null)
            queryable = queryable.SortBy(FieldMap, query.SortRules);

        if (query?.PagingRule != null)
            queryable = queryable.PageBy(query.PagingRule);

        return Task.FromResult(queryable);
    }

    async Task<int> IActiveRecordRepository<T>.CountAsync(FilterRule? filterRule)
    {
        var query = new ActiveRecordQuery(filterRule: filterRule);
        var queryable = await QueryableAsync(defaultPredicate: null, query: query);
        return await queryable.CountAsync();
    }

    async Task<IReadOnlyCollection<T>> IActiveRecordRepository<T>.ByKeysAsync<TId>(IEnumerable<TId> keys)
    {
        return await ByKeysAsync(defaultExpression: null, keys);
    }

    async Task<IReadOnlyCollection<T>> IActiveRecordServiceRepository<T>.ByKeysAsync<TId>(
        IEnumerable<TId> keys)
    {
        var context = new ActionContext<T>(ActionType.GetByKey, _settings.Definition, null, null, null);
        var defaultExpression = await _authorizationService.ExpressionAsync(context);
        return await ByKeysAsync(defaultExpression, keys);
    }

    private async Task<IReadOnlyCollection<T>> ByKeysAsync<TId>(
        Expression<Func<T, bool>>? defaultExpression,
        IEnumerable<TId> keys,
        bool noTracking = false)
        where TId : notnull
    {
        keys = keys?.Distinct().ToArray() ?? throw new ArgumentNullException(nameof(keys));
        if (!keys.Any()) return Array.Empty<T>();

        var queryable = DbContext.Set<T>().AsQueryable();

        if (defaultExpression != null)
            queryable = queryable.Where(defaultExpression);

        var expression = ActiveRecordExpression.KeyIn<T>(_settings, keys.Cast<object>());
        queryable = queryable.Where(expression);

        if (noTracking)
            queryable = queryable.AsNoTracking();

        return await queryable.ToArrayAsync();
    }

    async Task<IReadOnlyDictionary<TKey, T>> IActiveRecordRepository<T>.MapAsync<TKey>(IEnumerable<TKey> keys)
    {
        var values = await ByKeysAsync(defaultExpression: null, keys);
        var getKey = ActiveRecordExpression.GetKey<T, TKey>(_settings).Compile();
        return values.ToDictionary(x => getKey(x), x => x);
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

    public async Task<bool> AnyAsync(FilterRule? filterRule = null)
    {
        var query = new ActiveRecordQuery(filterRule: filterRule);
        var queryable = await QueryableAsync(defaultPredicate: null, query: query);
        return await queryable.AnyAsync();
    }
}