﻿using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Webinex.ActiveRecord.Annotations;

namespace Webinex.ActiveRecord;

public interface IActiveRecordAuthorizationService<T>
{
    Task<Expression<Func<T, bool>>?> ExpressionAsync(IActionContext<T> context);
    Task<bool> InvokeAsync(IActionContext<T> context);
}

internal class ActiveRecordAuthorizationService<T> : IActiveRecordAuthorizationService<T>
{
    private readonly IServiceProvider _serviceProvider;

    public ActiveRecordAuthorizationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<Expression<Func<T, bool>>?> ExpressionAsync(IActionContext<T> context)
    {
        if (context.Type != ActionType.GetAll && context.Type != ActionType.GetByKey)
            throw new InvalidOperationException($"Invalid action type {context.Type}");

        var authorize = context.Definition.Authorize;
        return authorize != null ? await InvokeAsync<Expression<Func<T, bool>>>(context, authorize) : null;
    }

    public async Task<bool> InvokeAsync(IActionContext<T> context)
    {
        if (context.Type != ActionType.Create && context.Type != ActionType.Delete && context.Type != ActionType.Update)
            throw new InvalidOperationException($"Invalid action type {context.Type}");
        
        if (context.MethodDefinition!.Authorize == null)
            return true;

        var authorize = context.MethodDefinition.Authorize;
        return await InvokeAsync<bool>(context, authorize);
    }

    private async Task<TResult?> InvokeAsync<TResult>(IActionContext<T> context, Delegate authorize)
    {
        var methodInfo = authorize.GetType().GetMethod("Invoke")!;
        var parameters = methodInfo.GetParameters();
        var arguments = parameters.Select(
            x =>
            {
                if (x.ParameterType == typeof(IActionContext<T>))
                    return context;

                if (x.ParameterType == typeof(T))
                    return context.Instance;

                return _serviceProvider.GetRequiredService(x.ParameterType);
            }).ToArray();

        var result = methodInfo.Invoke(authorize, arguments)!;
        var underlying = await Result.UnwrapAsync(result);
        return (TResult?)underlying;
    }
}

public static class ActiveRecordAuthorizationServiceExtensions
{
    public static async Task ThrowAsync<T>(this IActiveRecordAuthorizationService<T> service, IActionContext<T> context)
    {
        var authorized = await service.InvokeAsync(context);
        if (!authorized) throw new UnauthorizedAccessException();
    }
}