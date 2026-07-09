using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Webinex.ActiveRecord.Annotations;
using Webinex.Asky;
using Webinex.Coded;

namespace Webinex.ActiveRecord;

public interface IActiveRecordInteractor<TType>
{
    Task<ListSegment<TType>> ListSegmentAsync(Query? query = null, bool includeTotal = true);
    Task<TType?> ByKeyAsync(object key);
    Task<IReadOnlyCollection<TType>> GetAllAsync(Query? query = null);
    Task<int> CountAsync(FilterRule? filterRule = null);
    Task<bool> AnyAsync(FilterRule? filterRule = null);

    Task<object?> InvokeAsync(
        ActiveRecordMethodDefinition method,
        object? id,
        object? body);
}

internal class ActiveRecordInteractor<TType> : IActiveRecordInteractor<TType>
    where TType : class
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ActiveRecordInteractor<TType>> _logger;
    private readonly IActiveRecordInteractorRepository<TType> _repository;
    private readonly IActiveRecordSettings<TType> _settings;
    private readonly IActiveRecordAuthorizationService<TType> _authorizationService;

    public ActiveRecordInteractor(
        IServiceProvider services,
        ILogger<ActiveRecordInteractor<TType>> logger,
        IActiveRecordInteractorRepository<TType> repository,
        IActiveRecordSettings<TType> settings,
        IActiveRecordAuthorizationService<TType> authorizationService)
    {
        _services = services;
        _logger = logger;
        _repository = repository;
        _settings = settings;
        _authorizationService = authorizationService;
    }

    public async Task<ListSegment<TType>> ListSegmentAsync(Query? query = null, bool includeTotal = true)
    {
        var defaultPredicate = await DefaultPredicateAsync(ActionType.GetAll);
        return await _repository.WithDefaultPredicate(defaultPredicate).ListSegmentAsync(query, includeTotal, readOnly: true);
    }

    public async Task<TType?> ByKeyAsync(object key)
    {
        var defaultPredicate = await DefaultPredicateAsync(ActionType.GetByKey);
        return await _repository.WithDefaultPredicate(defaultPredicate).ByKeyAsync(key);
    }

    public async Task<IReadOnlyCollection<TType>> GetAllAsync(Query? query = null)
    {
        var defaultPredicate = await DefaultPredicateAsync(ActionType.GetAll);
        return await _repository.WithDefaultPredicate(defaultPredicate).GetAllAsync(query);
    }

    public async Task<int> CountAsync(FilterRule? filterRule = null)
    {
        var defaultPredicate = await DefaultPredicateAsync(ActionType.GetAll);
        return await _repository.WithDefaultPredicate(defaultPredicate).CountAsync(filterRule);
    }

    public async Task<bool> AnyAsync(FilterRule? filterRule = null)
    {
        var defaultPredicate = await DefaultPredicateAsync(ActionType.GetAll);
        return await _repository.WithDefaultPredicate(defaultPredicate).AnyAsync(filterRule);
    }

    public Task<object?> InvokeAsync(ActiveRecordMethodDefinition method, object? id, object? body)
    {
        if (!method.MethodInfo.IsStatic && id == null)
            throw new InvalidOperationException("No id provided");

        return method.MethodInfo.IsStatic ? InvokeStaticAsync(method, body) : InvokeInstanceAsync(method, id!, body);
    }

    private async Task<Expression<Func<TType, bool>>?> DefaultPredicateAsync(ActionType actionType)
    {
        var context = new ActionContext<TType>(_services, actionType, _settings.Definition, null, null, null);
        return await _authorizationService.ExpressionAsync(context);
    }

    private async Task<object?> InvokeInstanceAsync(ActiveRecordMethodDefinition method, object id, object? body)
    {
        var services = ResolveServices(method);
        var instance = await _repository.ByKeyAsync(id);
        instance = instance ?? throw CodedException.NotFound(id);

        var context = new ActionContext<TType>(_services, method.Type, _settings.Definition, method, instance, body);
        await _authorizationService.ThrowAsync(context);

        var result = method.MethodInfo.Invoke(
            instance,
            method.Parameters.Select(x => x.ParameterSource == ParameterSource.DependencyInjection ? services[x] : body)
                .ToArray());

        var returnValue = await Result.UnwrapAsync(result);
        return Result.IsVoidOrTask(method.MethodInfo) ? typeof(void) : returnValue;
    }

    private async Task<object?> InvokeStaticAsync(ActiveRecordMethodDefinition method, object? body)
    {
        var context = new ActionContext<TType>(_services, method.Type, _settings.Definition, method, default, body);
        await _authorizationService.ThrowAsync(context);

        var services = ResolveServices(method);
        var result = method.MethodInfo.Invoke(
            null,
            method.Parameters.Select(x => x.ParameterSource == ParameterSource.DependencyInjection ? services[x] : body)
                .ToArray())!;

        var entity = (TType)(await Result.UnwrapAsync(result))!;

        await _repository.AddAsync(entity);
        return entity;
    }

    private IDictionary<ActiveRecordParameterDefinition, object> ResolveServices(ActiveRecordMethodDefinition method)
    {
        var parameters = method.Parameters
            .Where(x => x.ParameterSource == ParameterSource.DependencyInjection)
            .ToArray();
        var result = new Dictionary<ActiveRecordParameterDefinition, object>();

        foreach (var parameter in parameters)
        {
            try
            {
                result[parameter] = _services.GetRequiredService(parameter.ParameterInfo.ParameterType);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to resolve service for parameter {Parameter}",
                    parameter.ParameterInfo.Name);
                throw;
            }
        }

        return result;
    }
}