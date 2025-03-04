using System.Linq.Expressions;
using System.Reflection;
using Webinex.ActiveRecord.Annotations;

namespace Webinex.ActiveRecord;

public class FluentAuthorizationConventionBuilder<T>
{
    private Delegate? _access;
    private readonly LinkedList<AuthorizationRule> _rules = new();

    public FluentAuthorizationConventionBuilder<T> AccessDelegate(Delegate? @delegate)
    {
        _access = @delegate;
        return this;
    }

    public FluentAuthorizationConventionBuilder<T> AddActionRuleDelegate(
        Func<MethodInfo, ActionType, bool> match,
        Delegate predicate)
    {
        _rules.AddLast(new AuthorizationRule(match, predicate));
        return this;
    }

    public IAuthorizationConvention<T> Build()
    {
        return new Convention(_access, _rules.ToArray());
    }

    private class Convention(Delegate? accessDelegate, IEnumerable<AuthorizationRule> rules)
        : IAuthorizationConvention<T>
    {
        public AuthorizationSettings Create(Type type)
        {
            return new AuthorizationSettings(accessDelegate, rules);
        }
    }
}

public static class AuthorizationConventionBuilderExtensions
{
    public static FluentAuthorizationConventionBuilder<T> Access<T>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Func<IActionContext<T>, Expression<Func<T, bool>>> factory)
    {
        return builder.AccessDelegate(factory);
    }

    public static FluentAuthorizationConventionBuilder<T> Access<T, T1>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Func<T1, Expression<Func<T, bool>>> factory)
    {
        return builder.AccessDelegate(factory);
    }

    public static FluentAuthorizationConventionBuilder<T> Action<T, T1>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Expression<Func<T, Delegate>> method,
        Func<T1, Task<bool>> predicate)
    {
        var methodInfo = ResolveMethodInfo(method);
        return builder.AddActionRuleDelegate((m, _) => m == methodInfo, predicate);
    }

    public static FluentAuthorizationConventionBuilder<T> Action<T, T1, T2>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Expression<Func<T, Delegate>> method,
        Func<T1, T2, Task<bool>> predicate)
    {
        var methodInfo = ResolveMethodInfo(method);
        return builder.AddActionRuleDelegate((m, _) => m == methodInfo, predicate);
    }

    public static FluentAuthorizationConventionBuilder<T> Action<T, T1, T2, T3>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Expression<Func<T, Delegate>> method,
        Func<T1, T2, T3, Task<bool>> predicate)
    {
        var methodInfo = ResolveMethodInfo(method);
        return builder.AddActionRuleDelegate((m, _) => m == methodInfo, predicate);
    }

    public static FluentAuthorizationConventionBuilder<T> Action<T, T1>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Expression<Func<T, Delegate>> method,
        Func<T1, bool> predicate)
    {
        var methodInfo = ResolveMethodInfo(method);
        return builder.AddActionRuleDelegate((m, _) => m == methodInfo, predicate);
    }

    public static FluentAuthorizationConventionBuilder<T> Action<T, T1, T2>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Expression<Func<T, Delegate>> method,
        Func<T1, T2, bool> predicate)
    {
        var methodInfo = ResolveMethodInfo(method);
        return builder.AddActionRuleDelegate((m, _) => m == methodInfo, predicate);
    }

    public static FluentAuthorizationConventionBuilder<T> Action<T, T1, T2, T3>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Expression<Func<T, Delegate>> method,
        Func<T1, T2, T3, bool> predicate)
    {
        var methodInfo = ResolveMethodInfo(method);
        return builder.AddActionRuleDelegate((m, _) => m == methodInfo, predicate);
    }

    private static MethodInfo ResolveMethodInfo<T>(Expression<Func<T, Delegate>> select)
    {
        if (select.Body is not UnaryExpression unaryExpression)
            throw new ArgumentException($"Expression '{select.Body.GetType().Name}' is not a unary expression.");
        
        if (unaryExpression.NodeType != ExpressionType.Convert)
            throw new ArgumentException($"Expression '{select.Body.GetType().Name}' is not a convert expression.");
        
        if (unaryExpression.Operand is not MethodCallExpression methodCallExpression)
            throw new ArgumentException($"Expression '{select.Body.GetType().Name}' is not a method call.");
        
        if (methodCallExpression.Object is not ConstantExpression constantExpression)
            throw new ArgumentException($"Expression '{select.Body.GetType().Name}' is not a constant expression.");
        
        if (constantExpression.Value is not MethodInfo methodInfo)
            throw new ArgumentException($"Expression '{select.Body.GetType().Name}' is not a method info selector.");

        return methodInfo;
    }

    // public static FluentAuthorizationConventionBuilder<T> Action<T, T1>(
    //     this FluentAuthorizationConventionBuilder<T> builder,
    //     Delegate method,
    //     Func<T1, bool> predicate)
    // {
    // }
    //
    // public static FluentAuthorizationConventionBuilder<T> Action<T>(
    //     this FluentAuthorizationConventionBuilder<T> builder,
    //     Expression<Func<T, Delegate>> method,
    //     Func<T, IServiceProvider, bool> predicate)
    // {
    // }

    public static FluentAuthorizationConventionBuilder<T> EveryAction<T>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Func<IActionContext<T>, Task<bool>> predicate)
    {
        return builder.AddActionRuleDelegate((_, _) => true, predicate);
    }

    public static FluentAuthorizationConventionBuilder<T> EveryAction<T, T1>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Func<T1, bool> predicate)
    {
        return builder.AddActionRuleDelegate((_, _) => true, predicate);
    }

    public static FluentAuthorizationConventionBuilder<T> EveryAction<T, T1>(
        this FluentAuthorizationConventionBuilder<T> builder,
        ActionType type,
        Func<T1, Task<bool>> predicate)
    {
        return builder.AddActionRuleDelegate((_, t) => type.HasFlag(t), predicate);
    }

    public static FluentAuthorizationConventionBuilder<T> EveryAction<T, T1, T2>(
        this FluentAuthorizationConventionBuilder<T> builder,
        ActionType type,
        Func<T1, T2, Task<bool>> predicate)
    {
        return builder.AddActionRuleDelegate((_, t) => type.HasFlag(t), predicate);
    }

    public static FluentAuthorizationConventionBuilder<T> EveryAction<T, T1, T2, T3>(
        this FluentAuthorizationConventionBuilder<T> builder,
        ActionType type,
        Func<T1, T2, T3, Task<bool>> predicate)
    {
        return builder.AddActionRuleDelegate((_, t) => type.HasFlag(t), predicate);
    }

    public static FluentAuthorizationConventionBuilder<T> EveryAction<T, T1>(
        this FluentAuthorizationConventionBuilder<T> builder,
        ActionType type,
        Func<T1, bool> predicate)
    {
        return builder.AddActionRuleDelegate((_, t) => type.HasFlag(t), predicate);
    }

    public static FluentAuthorizationConventionBuilder<T> EveryAction<T, T1, T2>(
        this FluentAuthorizationConventionBuilder<T> builder,
        ActionType type,
        Func<T1, T2, bool> predicate)
    {
        return builder.AddActionRuleDelegate((_, t) => type.HasFlag(t), predicate);
    }

    public static FluentAuthorizationConventionBuilder<T> EveryAction<T, T1, T2, T3>(
        this FluentAuthorizationConventionBuilder<T> builder,
        ActionType type,
        Func<T1, T2, T3, bool> predicate)
    {
        return builder.AddActionRuleDelegate((_, t) => type.HasFlag(t), predicate);
    }
}

public class FluentAuthorizationConventionFactoryBuilder
{
    private LinkedList<object> Conventions { get; } = new();
    private bool _throwNotFound = false;

    public FluentAuthorizationConventionFactoryBuilder Type<T>(
        Action<FluentAuthorizationConventionBuilder<T>> configure)
    {
        var builder = new FluentAuthorizationConventionBuilder<T>();
        configure(builder);
        Conventions.AddLast(builder.Build());
        return this;
    }

    public FluentAuthorizationConventionFactoryBuilder ThrowNotFound(bool @throw = true)
    {
        _throwNotFound = @throw;
        return this;
    }

    public Func<Type, object?> Build()
    {
        return type =>
        {
            var convention = Conventions.FirstOrDefault(
                x => x.GetType().GetInterfaces().Any(
                    i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAuthorizationConvention<>) &&
                         i.GetGenericArguments()[0].IsAssignableFrom(type)));

            if (convention == null && _throwNotFound)
                throw new InvalidOperationException($"Unable to find convention for type {type.FullName}");

            return convention;
        };
    }
}