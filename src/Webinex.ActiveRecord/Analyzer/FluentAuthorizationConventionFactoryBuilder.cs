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
    /// <summary>
    ///     Specifies an expression that must be added to any data access operation.
    ///     This can apply to actions such as <c>GetAll</c>, <c>GetById</c>, or any other Active Record action.
    ///     Therefore, this expression can be considered as automatically appended to each action.
    ///     <br />
    ///     <br />
    ///     <b>NOTE: Please be aware that multiple calls to <c>Access()</c> will completely overwrite previous calls.</b>
    ///     <br />
    ///     <br />
    ///     Each argument type can represent one of the following:
    ///     <ul>
    ///         <li><see cref="IActionContext{TType}"/></li>
    ///         <li><see cref="IActionContext"/></li>
    ///         <li>Any service registered in DI container</li>
    ///     </ul>
    /// </summary>
    /// <example>
    ///     <code lang="csharp">
    ///     o.Type&lt;MyEntity&gt;(x =&gt; x
    ///         .Access((IUser user) => entity => entity.TenantId == user.TenantId);
    ///     </code>
    /// </example>
    /// <param name="builder">Builder</param>
    /// <param name="factory">Expression factory</param>
    /// <typeparam name="T">Active record type</typeparam>
    public static FluentAuthorizationConventionBuilder<T> Access<T>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Func<Expression<Func<T, bool>>> factory)
    {
        return builder.AccessDelegate(factory);
    }

    /// <inheritdoc cref="Access{T}"/>
    public static FluentAuthorizationConventionBuilder<T> Access<T, T1>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Func<T1, Expression<Func<T, bool>>> factory)
    {
        return builder.AccessDelegate(factory);
    }

    /// <inheritdoc cref="Access{T}"/>
    public static FluentAuthorizationConventionBuilder<T> Access<T, T1, T2>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Func<T1, T2, Expression<Func<T, bool>>> factory)
    {
        return builder.AccessDelegate(factory);
    }

    /// <summary>
    ///     Allows adding authorization checks that must be executed before the method is executed.
    ///     If the result is false, an <see cref="UnauthorizedAccessException"/> will be thrown.
    ///     <br />
    ///     <br />
    ///     <b>
    ///         NOTE: Please note that the order of addition is crucial when constructing authorization conventions,
    ///         as the first match based on the criteria will be selected, and subsequent calls will be ignored.
    ///     </b>
    ///     <br />
    ///     <br />
    ///     Each argument type can represent one of the following:
    ///     <ul>
    ///         <li><see cref="IActionContext{TType}"/></li>
    ///         <li><see cref="IActionContext"/></li>
    ///         <li>Entity <typeparamref name="T"/></li>
    ///         <li>Any service registered in DI container</li>
    ///     </ul>
    /// </summary>
    /// <example>
    ///     Example of applying an authorization rule to an instance method:
    ///     <code>
    ///         .Type&lt;MyEntity&gt;(o =&gt; o
    ///             .Action(
    ///                 myEntity =&gt; myEntity.Update,
    ///                 (IUser user, MyEntity myEntity) =&gt; user.IsAdmin() &amp;&amp; myEntity.TenantId == user.TenantId));
    ///     </code>
    ///     <br />
    ///     Example of applying an authorization rule to a static method:<br />
    ///     <i>Please note that to access the request body, you can use <see cref="IActionContext"/></i>
    ///     <code>
    ///         .Type&lt;MyEntity&gt;(o =&gt; o
    ///             .Action(
    ///                 MyEntity.Create,
    ///                 (IUser user) =&gt; user.IsAdmin());
    ///     </code>
    /// </example>
    /// <remarks>
    ///     You can also access the request body using <see cref="IActionContext.Body"/>
    ///     if the checks depend on the incoming parameters.
    /// </remarks>
    /// <param name="builder"><see cref="FluentAuthorizationConventionBuilder{T}"/></param>
    /// <param name="method">Method selector</param>
    /// <param name="predicate">Authorization rule</param>
    public static FluentAuthorizationConventionBuilder<T> Action<T, T1>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Expression<Func<T, Delegate>> method,
        Func<T1, Task<bool>> predicate)
    {
        var methodInfo = ResolveMethodInfo(method);
        return builder.AddActionRuleDelegate((m, _) => m == methodInfo, predicate);
    }

    /// <inheritdoc cref="Action{T,T1}(Webinex.ActiveRecord.FluentAuthorizationConventionBuilder{T},System.Linq.Expressions.Expression{System.Func{T,System.Delegate}},System.Func{T1,System.Threading.Tasks.Task{bool}})"/>
    public static FluentAuthorizationConventionBuilder<T> Action<T, T1, T2>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Expression<Func<T, Delegate>> method,
        Func<T1, T2, Task<bool>> predicate)
    {
        var methodInfo = ResolveMethodInfo(method);
        return builder.AddActionRuleDelegate((m, _) => m == methodInfo, predicate);
    }

    /// <inheritdoc cref="Action{T,T1}(Webinex.ActiveRecord.FluentAuthorizationConventionBuilder{T},System.Linq.Expressions.Expression{System.Func{T,System.Delegate}},System.Func{T1,System.Threading.Tasks.Task{bool}})"/>
    public static FluentAuthorizationConventionBuilder<T> Action<T, T1, T2, T3>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Expression<Func<T, Delegate>> method,
        Func<T1, T2, T3, Task<bool>> predicate)
    {
        var methodInfo = ResolveMethodInfo(method);
        return builder.AddActionRuleDelegate((m, _) => m == methodInfo, predicate);
    }

    /// <inheritdoc cref="Action{T,T1}(Webinex.ActiveRecord.FluentAuthorizationConventionBuilder{T},System.Linq.Expressions.Expression{System.Func{T,System.Delegate}},System.Func{T1,System.Threading.Tasks.Task{bool}})"/>
    public static FluentAuthorizationConventionBuilder<T> Action<T, T1>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Expression<Func<T, Delegate>> method,
        Func<T1, bool> predicate)
    {
        var methodInfo = ResolveMethodInfo(method);
        return builder.AddActionRuleDelegate((m, _) => m == methodInfo, predicate);
    }

    /// <inheritdoc cref="Action{T,T1}(Webinex.ActiveRecord.FluentAuthorizationConventionBuilder{T},System.Linq.Expressions.Expression{System.Func{T,System.Delegate}},System.Func{T1,System.Threading.Tasks.Task{bool}})"/>
    public static FluentAuthorizationConventionBuilder<T> Action<T, T1, T2>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Expression<Func<T, Delegate>> method,
        Func<T1, T2, bool> predicate)
    {
        var methodInfo = ResolveMethodInfo(method);
        return builder.AddActionRuleDelegate((m, _) => m == methodInfo, predicate);
    }

    /// <inheritdoc cref="Action{T,T1}(Webinex.ActiveRecord.FluentAuthorizationConventionBuilder{T},System.Linq.Expressions.Expression{System.Func{T,System.Delegate}},System.Func{T1,System.Threading.Tasks.Task{bool}})"/>
    public static FluentAuthorizationConventionBuilder<T> Action<T, T1, T2, T3>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Expression<Func<T, Delegate>> method,
        Func<T1, T2, T3, bool> predicate)
    {
        var methodInfo = ResolveMethodInfo(method);
        return builder.AddActionRuleDelegate((m, _) => m == methodInfo, predicate);
    }

    /// <inheritdoc cref="Action{T,T1}(Webinex.ActiveRecord.FluentAuthorizationConventionBuilder{T},System.Linq.Expressions.Expression{System.Func{T,System.Delegate}},System.Func{T1,System.Threading.Tasks.Task{bool}})"/>
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

    /// <inheritdoc cref="Action{T,T1}(Webinex.ActiveRecord.FluentAuthorizationConventionBuilder{T},System.Linq.Expressions.Expression{System.Func{T,System.Delegate}},System.Func{T1,System.Threading.Tasks.Task{bool}})"/>
    public static FluentAuthorizationConventionBuilder<T> Action<T, T1>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Delegate method,
        Func<T1, Task<bool>> predicate)
    {
        return builder.AddActionRuleDelegate((m, _) => m == method.Method, predicate);
    }

    /// <inheritdoc cref="Action{T,T1}(Webinex.ActiveRecord.FluentAuthorizationConventionBuilder{T},System.Linq.Expressions.Expression{System.Func{T,System.Delegate}},System.Func{T1,System.Threading.Tasks.Task{bool}})"/>
    public static FluentAuthorizationConventionBuilder<T> Action<T, T1, T2>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Delegate method,
        Func<T1, T2, Task<bool>> predicate)
    {
        return builder.AddActionRuleDelegate((m, _) => m == method.Method, predicate);
    }

    /// <inheritdoc cref="Action{T,T1}(Webinex.ActiveRecord.FluentAuthorizationConventionBuilder{T},System.Linq.Expressions.Expression{System.Func{T,System.Delegate}},System.Func{T1,System.Threading.Tasks.Task{bool}})"/>
    public static FluentAuthorizationConventionBuilder<T> Action<T, T1, T2, T3>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Delegate method,
        Func<T1, T2, T3, Task<bool>> predicate)
    {
        return builder.AddActionRuleDelegate((m, _) => m == method.Method, predicate);
    }

    /// <inheritdoc cref="Action{T,T1}(Webinex.ActiveRecord.FluentAuthorizationConventionBuilder{T},System.Linq.Expressions.Expression{System.Func{T,System.Delegate}},System.Func{T1,System.Threading.Tasks.Task{bool}})"/>
    public static FluentAuthorizationConventionBuilder<T> Action<T, T1>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Delegate method,
        Func<T1, bool> predicate)
    {
        return builder.AddActionRuleDelegate((m, _) => m == method.Method, predicate);
    }

    /// <inheritdoc cref="Action{T,T1}(Webinex.ActiveRecord.FluentAuthorizationConventionBuilder{T},System.Linq.Expressions.Expression{System.Func{T,System.Delegate}},System.Func{T1,System.Threading.Tasks.Task{bool}})"/>
    public static FluentAuthorizationConventionBuilder<T> Action<T, T1, T2>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Delegate method,
        Func<T1, T2, bool> predicate)
    {
        return builder.AddActionRuleDelegate((m, _) => m == method.Method, predicate);
    }

    /// <inheritdoc cref="Action{T,T1}(Webinex.ActiveRecord.FluentAuthorizationConventionBuilder{T},System.Linq.Expressions.Expression{System.Func{T,System.Delegate}},System.Func{T1,System.Threading.Tasks.Task{bool}})"/>
    public static FluentAuthorizationConventionBuilder<T> Action<T, T1, T2, T3>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Delegate method,
        Func<T1, T2, T3, bool> predicate)
    {
        return builder.AddActionRuleDelegate((m, _) => m == method.Method, predicate);
    }
    
    /// <summary>
    ///     Unlike <c>Action()</c>, <c>EveryAction()</c> allows you to configure authorization rules not for a concrete action,
    ///     but for an entire group of actions.
    /// 
    ///     <br />
    ///     <br />
    ///
    ///     <b>
    ///         NOTE: It should be used at the very end of your Fluent configuration, as the order of method calls is important.
    ///         If it is called at the beginning, other rules will be ignored.
    ///     </b>
    ///     <br />
    ///     <br />
    ///     Each argument type can represent one of the following:
    ///     <ul>
    ///         <li><see cref="IActionContext{TType}"/></li>
    ///         <li><see cref="IActionContext"/></li>
    ///         <li>Entity <typeparamref name="T"/></li>
    ///         <li>Any service registered in DI container</li>
    ///     </ul>
    /// </summary>
    /// <example>
    ///     Let’s assume that only an admin can call the New() method, while all other company employees can perform Update and Delete:
    ///     <code>
    ///         o.Type&lt;MyEntity&gt;(x =&gt; x
    ///             .Authorize(MyEntity.New, (IUser user) =&gt; user.IsAdmin())
    ///             .EveryAction(ActionType.Update | ActionType.Delete, (IUser user) =&gt; user.IsEmployee())
    ///     </code>
    ///     <br/>
    ///     <br/>
    ///     It can also be useful if you want all other methods to be inaccessible by default unless an authorization rule is applied:
    ///     <code>
    ///         o.Type&lt;MyEntity&gt;(x =&gt; x
    ///             .Authorize(MyEntity.New, (IUser user) =&gt; user.IsAdmin())
    ///             .EveryAction(() => false)
    ///     </code>
    /// </example>
    /// <param name="builder"><see cref="FluentAuthorizationConventionBuilder{T}"/></param>
    /// <param name="predicate">Authorization predicate</param>
    public static FluentAuthorizationConventionBuilder<T> EveryAction<T, T1>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Func<T1, Task<bool>> predicate)
    {
        return builder.AddActionRuleDelegate((_, _) => true, predicate);
    }

    /// <inheritdoc cref="EveryAction{T,T1}(Webinex.ActiveRecord.FluentAuthorizationConventionBuilder{T},System.Func{T1,System.Threading.Tasks.Task{bool}})"/>
    public static FluentAuthorizationConventionBuilder<T> EveryAction<T, T1, T2>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Func<T1, T2, Task<bool>> predicate)
    {
        return builder.AddActionRuleDelegate((_, _) => true, predicate);
    }

    public static FluentAuthorizationConventionBuilder<T> EveryAction<T, T1, T2, T3>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Func<T1, T2, T3, Task<bool>> predicate)
    {
        return builder.AddActionRuleDelegate((_, _) => true, predicate);
    }

    public static FluentAuthorizationConventionBuilder<T> EveryAction<T, T1>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Func<T1, bool> predicate)
    {
        return builder.AddActionRuleDelegate((_, _) => true, predicate);
    }

    public static FluentAuthorizationConventionBuilder<T> EveryAction<T, T1, T2>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Func<T1, T2, bool> predicate)
    {
        return builder.AddActionRuleDelegate((_, _) => true, predicate);
    }

    public static FluentAuthorizationConventionBuilder<T> EveryAction<T, T1, T2, T3>(
        this FluentAuthorizationConventionBuilder<T> builder,
        Func<T1, T2, T3, bool> predicate)
    {
        return builder.AddActionRuleDelegate((_, _) => true, predicate);
    }
    
    /// <inheritdoc cref="EveryAction{T,T1}(Webinex.ActiveRecord.FluentAuthorizationConventionBuilder{T},System.Func{T1,System.Threading.Tasks.Task{bool}})"/>
    /// <param name="type"><see cref="ActionType"/> (this is a Flag Enum and can accept multiple values, such as <c>ActionType.Create | ActionType.Update</c>, for example)</param>
    public static FluentAuthorizationConventionBuilder<T> EveryAction<T, T1>(
        // ReSharper disable once InvalidXmlDocComment
        this FluentAuthorizationConventionBuilder<T> builder,
        ActionType type,
        // ReSharper disable once InvalidXmlDocComment
        Func<T1, Task<bool>> predicate)
    {
        return builder.AddActionRuleDelegate((_, t) => type.HasFlag(t), predicate);
    }

    /// <inheritdoc cref="EveryAction{T,T1}(Webinex.ActiveRecord.FluentAuthorizationConventionBuilder{T},Webinex.ActiveRecord.Annotations.ActionType,System.Func{T1,System.Threading.Tasks.Task{bool}})"/>
    public static FluentAuthorizationConventionBuilder<T> EveryAction<T, T1, T2>(
        this FluentAuthorizationConventionBuilder<T> builder,
        ActionType type,
        Func<T1, T2, Task<bool>> predicate)
    {
        return builder.AddActionRuleDelegate((_, t) => type.HasFlag(t), predicate);
    }

    /// <inheritdoc cref="EveryAction{T,T1}(Webinex.ActiveRecord.FluentAuthorizationConventionBuilder{T},Webinex.ActiveRecord.Annotations.ActionType,System.Func{T1,System.Threading.Tasks.Task{bool}})"/>
    public static FluentAuthorizationConventionBuilder<T> EveryAction<T, T1, T2, T3>(
        this FluentAuthorizationConventionBuilder<T> builder,
        ActionType type,
        Func<T1, T2, T3, Task<bool>> predicate)
    {
        return builder.AddActionRuleDelegate((_, t) => type.HasFlag(t), predicate);
    }

    /// <inheritdoc cref="EveryAction{T,T1}(Webinex.ActiveRecord.FluentAuthorizationConventionBuilder{T},Webinex.ActiveRecord.Annotations.ActionType,System.Func{T1,System.Threading.Tasks.Task{bool}})"/>
    public static FluentAuthorizationConventionBuilder<T> EveryAction<T, T1>(
        this FluentAuthorizationConventionBuilder<T> builder,
        ActionType type,
        Func<T1, bool> predicate)
    {
        return builder.AddActionRuleDelegate((_, t) => type.HasFlag(t), predicate);
    }

    /// <inheritdoc cref="EveryAction{T,T1}(Webinex.ActiveRecord.FluentAuthorizationConventionBuilder{T},Webinex.ActiveRecord.Annotations.ActionType,System.Func{T1,System.Threading.Tasks.Task{bool}})"/>
    public static FluentAuthorizationConventionBuilder<T> EveryAction<T, T1, T2>(
        this FluentAuthorizationConventionBuilder<T> builder,
        ActionType type,
        Func<T1, T2, bool> predicate)
    {
        return builder.AddActionRuleDelegate((_, t) => type.HasFlag(t), predicate);
    }

    /// <inheritdoc cref="EveryAction{T,T1}(Webinex.ActiveRecord.FluentAuthorizationConventionBuilder{T},Webinex.ActiveRecord.Annotations.ActionType,System.Func{T1,System.Threading.Tasks.Task{bool}})"/>
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

    /// <summary>
    ///     Indicates whether the Authorization Convention Factory should throw an exception if no conventions are found for a specific type.
    /// </summary>
    /// <remarks>
    ///     The default behavior is the same as <c>ThrowNotFound(true)</c>.
    /// </remarks>
    /// <param name="throw">Should throw</param>
    public FluentAuthorizationConventionFactoryBuilder ThrowNotFound(bool @throw = true)
    {
        _throwNotFound = @throw;
        return this;
    }

    /// <summary>
    ///     Returns the Authorization Convention Factory method, which can be used further when calling <c>ConfigureTypeAnalyzer(o => o.UseAuthorizationConvention(convention))</c>.
    /// </summary>
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