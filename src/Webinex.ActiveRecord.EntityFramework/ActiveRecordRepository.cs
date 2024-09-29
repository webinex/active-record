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

    Task<IQueryable<T>> IActiveRecordRepository<T>.QueryableAsync(
        FilterRule? filterRule,
        IReadOnlyCollection<SortRule>? sortRules,
        PagingRule? pagingRule)
    {
        return QueryableAsync(defaultPredicate: null, filterRule, sortRules, pagingRule);
    }

    async Task<IQueryable<T>> IActiveRecordServiceRepository<T>.QueryableAsync(
        FilterRule? filterRule,
        IReadOnlyCollection<SortRule>? sortRules,
        PagingRule? pagingRule)
    {
        var context = new ActionContext<T>(ActionType.GetAll, _settings.Definition, null, null, null);
        var defaultPredicate = await _authorizationService.ExpressionAsync(context);
        return await QueryableAsync(defaultPredicate, filterRule, sortRules, pagingRule);
    }

    async Task<IReadOnlyCollection<T>> IActiveRecordRepository<T>.QueryAsync(
        FilterRule? filterRule,
        IReadOnlyCollection<SortRule>? sortRules,
        PagingRule? pagingRule)
    {
        var queryable = await QueryableAsync(defaultPredicate: null, filterRule, sortRules, pagingRule);
        return await queryable.ToArrayAsync();
    }

    async Task<IReadOnlyCollection<T>> IActiveRecordServiceRepository<T>.QueryAsync(
        FilterRule? filterRule,
        IReadOnlyCollection<SortRule>? sortRules,
        PagingRule? pagingRule)
    {
        var context = new ActionContext<T>(ActionType.GetAll, _settings.Definition, null, null, null);
        var defaultPredicate = await _authorizationService.ExpressionAsync(context);
        var queryable = await QueryableAsync(defaultPredicate, filterRule, sortRules, pagingRule);
        return await queryable.ToArrayAsync();
    }

    private Task<IQueryable<T>> QueryableAsync(
        Expression<Func<T, bool>>? defaultPredicate,
        FilterRule? filterRule = null,
        IReadOnlyCollection<SortRule>? sortRules = null,
        PagingRule? pagingRule = null)
    {
        var queryable = DbContext.Set<T>().AsQueryable();

        if (defaultPredicate != null)
            queryable = queryable.Where(defaultPredicate);

        if (filterRule != null)
            queryable = queryable.Where(FieldMap, filterRule);

        if (sortRules != null)
            queryable = queryable.SortBy(FieldMap, sortRules);

        if (pagingRule != null)
            queryable = queryable.PageBy(pagingRule);

        return Task.FromResult(queryable);
    }

    async Task<int> IActiveRecordRepository<T>.CountAsync(FilterRule? filterRule)
    {
        var queryable = await QueryableAsync(defaultPredicate: null, filterRule: filterRule);
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
        IEnumerable<TId> keys)
        where TId : notnull
    {
        keys = keys?.Distinct().ToArray() ?? throw new ArgumentNullException(nameof(keys));
        if (!keys.Any()) return Array.Empty<T>();

        var queryable = DbContext.Set<T>().AsQueryable();

        if (defaultExpression != null)
            queryable = queryable.Where(defaultExpression);

        var expression = ActiveRecordExpression.KeyIn<T>(_settings, keys.Cast<object>());
        queryable = queryable.Where(expression);

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
}