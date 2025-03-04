using System.Reflection;
using Webinex.ActiveRecord.Annotations;

namespace Webinex.ActiveRecord;

public class AuthorizationRule
{
    public Func<MethodInfo, ActionType, bool> MethodPredicate { get; }
    public Delegate Authorize { get; }

    public AuthorizationRule(Func<MethodInfo, ActionType, bool> predicate, Delegate rule)
    {
        MethodPredicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        Authorize = rule ?? throw new ArgumentNullException(nameof(rule));
    }
}

public class AuthorizationSettings
{
    /// <summary>
    ///     Creates an <c>Expression&lt;Func&lt;TEntity,bool&gt;&gt;</c> that is automatically added 
    ///     to all data reading actions. Additionally, it will be incorporated into the search 
    ///     criteria by ID during update and delete operations.
    /// </summary>
    /// <remarks>
    ///     A typical delegate expression looks like:
    ///     <code>
    ///         (IActionContext context) => entity => entity.TenantId == context.Service&lt;IUser&gt;().TenantId
    ///     </code>
    ///     <br />
    ///     It can also include conditional branching if needed, for example:
    ///     <code>
    ///         (IActionContext context) => context.Service&lt;IUser&gt;().IsAdmin() ? entity => true : entity => entity.UserId == context.Service&lt;IUser&gt;().Id
    ///     </code>
    /// </remarks>
    /// <returns>
    ///     An expression that represents the filtering logic based on the <see cref="IActionContext"/>.
    /// </returns>
    /// <example>
    ///     <code>
    ///         (IActionContext context) => entity => entity.TenantId == context.Service&lt;IUser&gt;().TenantId
    ///     </code>
    /// </example>
    public Delegate? AccessExpressionFactory { get; protected set; }

    /// <summary>
    ///     Represents authorization rules that must be executed before invoking the active record entity method.
    ///     These rules can either be static methods or instance methods defined within the active record entity itself.
    /// </summary>
    /// <remarks>
    ///     The rules ensure that necessary authorization checks are performed before any action is executed on the entity.
    ///     They are typically evaluated before creating, modifying, updating, or deleting data or entity itself.
    /// </remarks>
    public IReadOnlyCollection<AuthorizationRule> Rules { get; }

    /// <summary>
    ///     Creates new instance of <see cref="AuthorizationSettings"/>
    /// </summary>
    /// <param name="accessExpressionFactory">
    ///     Factory which creates an <c>Expression&lt;Func&lt;TEntity,bool&gt;&gt;</c> that is automatically added 
    ///     to all data reading actions. Additionally, it will be incorporated into the search 
    ///     criteria by ID during update and delete operations.
    ///     <br />
    ///     See <see cref="AccessExpressionFactory"/> for details
    /// </param>
    /// <param name="rules">
    ///     Represents authorization rules that must be executed before invoking the active record entity method.
    ///     These rules can either be static methods or instance methods defined within the active record entity itself.
    ///     <br />
    ///     See <see cref="AuthorizationRule"/> for details
    /// </param>
    public AuthorizationSettings(Delegate? accessExpressionFactory, IEnumerable<AuthorizationRule>? rules)
    {
        AccessExpressionFactory = accessExpressionFactory ?? throw new ArgumentNullException(nameof(accessExpressionFactory));
        Rules = rules?.ToArray() ?? throw new ArgumentNullException(nameof(rules));
    }
}

// public class AuthorizationSettingsBuilder<TType> : AuthorizationSettings
// {
//     internal AuthorizationSettingsBuilder()
//     {
//     }
//
//     public void Unauthorized()
//     {
//         ReadExpressionFactory = null;
//     }
//
//     public AuthorizationSettingsBuilder<TType> Authorize<T1>(Func<T1, Expression<Func<TType, bool>>> authorize)
//     {
//         ReadExpressionFactory = authorize;
//         return this;
//     }
//
//     public AuthorizationSettingsBuilder<TType> Authorize<T1, T2>(
//         Func<T1, T2, Expression<Func<TType, bool>>> authorize)
//     {
//         ReadExpressionFactory = authorize;
//         return this;
//     }
//
//     public AuthorizationSettingsBuilder<TType> Authorize<T1, T2, T3>(
//         Func<T1, T2, T3, Expression<Func<TType, bool>>> authorize)
//     {
//         ReadExpressionFactory = authorize;
//         return this;
//     }
//
//     public AuthorizationSettingsBuilder<TType> Authorize<T1, T2, T3, T4>(
//         Func<T1, T2, T3, T4, Expression<Func<TType, bool>>> authorize)
//     {
//         ReadExpressionFactory = authorize;
//         return this;
//     }
//
//     public AuthorizationSettingsBuilder<TType> Authorize<T1, T2, T3, T4, T5>(
//         Func<T1, T2, T3, T4, T5, Expression<Func<TType, bool>>> authorize)
//     {
//         ReadExpressionFactory = authorize;
//         return this;
//     }
//
//     public AuthorizationSettingsBuilder<TType> Authorize<T1, T2, T3, T4, T5, T6>(
//         Func<T1, T2, T3, T4, T5, T6, Expression<Func<TType, bool>>> authorize)
//     {
//         ReadExpressionFactory = authorize;
//         return this;
//     }
//
//     public AuthorizationSettingsBuilder<TType> Authorize<T1, T2, T3, T4, T5, T6, T7>(
//         Func<T1, T2, T3, T4, T5, T6, T7, Expression<Func<TType, bool>>> authorize)
//     {
//         ReadExpressionFactory = authorize;
//         return this;
//     }
//
//     public AuthorizationSettingsBuilder<TType> Authorize<T1, T2, T3, T4, T5, T6, T7, T8>(
//         Func<T1, T2, T3, T4, T5, T6, T7, T8, Expression<Func<TType, bool>>> authorize)
//     {
//         ReadExpressionFactory = authorize;
//         return this;
//     }
//
//     public AuthorizationSettingsBuilder<TType> Authorize<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
//         Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Expression<Func<TType, bool>>> authorize)
//     {
//         ReadExpressionFactory = authorize;
//         return this;
//     }
//
//     public AuthorizationSettingsBuilder<TType> Authorize<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
//         Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Expression<Func<TType, bool>>> authorize)
//     {
//         ReadExpressionFactory = authorize;
//         return this;
//     }
//
//     public AuthorizationSettingsBuilder<TType> When<T1>(Func<MethodInfo, bool> predicate, Func<T1, bool> authorize)
//     {
//         return AddRule(predicate, authorize);
//     }
//
//     public AuthorizationSettingsBuilder<TType> When<T1, T2>(
//         Func<MethodInfo, bool> predicate,
//         Func<T1, T2, bool> authorize)
//     {
//         return AddRule(predicate, authorize);
//     }
//
//     public AuthorizationSettingsBuilder<TType> When<T1, T2, T3>(
//         Func<MethodInfo, bool> predicate,
//         Func<T1, T2, T3, bool> authorize)
//     {
//         return AddRule(predicate, authorize);
//     }
//
//     public AuthorizationSettingsBuilder<TType> When<T1, T2, T3, T4>(
//         Func<MethodInfo, bool> predicate,
//         Func<T1, T2, T3, T4, bool> authorize)
//     {
//         return AddRule(predicate, authorize);
//     }
//
//     public AuthorizationSettingsBuilder<TType> When<T1, T2, T3, T4, T5>(
//         Func<MethodInfo, bool> predicate,
//         Func<T1, T2, T3, T4, T5, bool> authorize)
//     {
//         return AddRule(predicate, authorize);
//     }
//
//     public AuthorizationSettingsBuilder<TType> When<T1, T2, T3, T4, T5, T6>(
//         Func<MethodInfo, bool> predicate,
//         Func<T1, T2, T3, T4, T5, T6, bool> authorize)
//     {
//         return AddRule(predicate, authorize);
//     }
//
//     public AuthorizationSettingsBuilder<TType> When<T1, T2, T3, T4, T5, T6, T7>(
//         Func<MethodInfo, bool> predicate,
//         Func<T1, T2, T3, T4, T5, T6, T7, bool> authorize)
//     {
//         return AddRule(predicate, authorize);
//     }
//
//     public AuthorizationSettingsBuilder<TType> When<T1, T2, T3, T4, T5, T6, T7, T8>(
//         Func<MethodInfo, bool> predicate,
//         Func<T1, T2, T3, T4, T5, T6, T7, T8, bool> authorize)
//     {
//         return AddRule(predicate, authorize);
//     }
//
//     public AuthorizationSettingsBuilder<TType> When<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
//         Func<MethodInfo, bool> predicate,
//         Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool> authorize)
//     {
//         return AddRule(predicate, authorize);
//     }
//
//     public AuthorizationSettingsBuilder<TType> When<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
//         Func<MethodInfo, bool> predicate,
//         Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool> authorize)
//     {
//         return AddRule(predicate, authorize);
//     }
//
//     private AuthorizationSettingsBuilder<TType> AddRule(Func<MethodInfo, bool> predicate, Delegate rule)
//     {
//         _rules.Add(new AuthorizationRule(predicate, rule));
//         return this;
//     }
//
//     public AuthorizationSettingsBuilder<TType> All<T1>(Func<T1, bool> authorize)
//     {
//         return AddRule(_ => true, authorize);
//     }
//
//     public AuthorizationSettingsBuilder<TType> All<T1, T2>(Func<T1, T2, bool> authorize)
//     {
//         return AddRule(_ => true, authorize);
//     }
//
//     public AuthorizationSettingsBuilder<TType> All<T1, T2, T3>(Func<T1, T2, T3, bool> authorize)
//     {
//         return AddRule(_ => true, authorize);
//     }
//
//     public AuthorizationSettingsBuilder<TType> All<T1, T2, T3, T4>(Func<T1, T2, T3, T4, bool> authorize)
//     {
//         return AddRule(_ => true, authorize);
//     }
//
//     public AuthorizationSettingsBuilder<TType> All<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, bool> authorize)
//     {
//         return AddRule(_ => true, authorize);
//     }
//
//     public AuthorizationSettingsBuilder<TType> All<T1, T2, T3, T4, T5, T6>(
//         Func<T1, T2, T3, T4, T5, T6, bool> authorize)
//     {
//         return AddRule(_ => true, authorize);
//     }
//
//     public AuthorizationSettingsBuilder<TType> All<T1, T2, T3, T4, T5, T6, T7>(
//         Func<T1, T2, T3, T4, T5, T6, T7, bool> authorize)
//     {
//         return AddRule(_ => true, authorize);
//     }
//
//     public AuthorizationSettingsBuilder<TType> All<T1, T2, T3, T4, T5, T6, T7, T8>(
//         Func<T1, T2, T3, T4, T5, T6, T7, T8, bool> authorize)
//     {
//         return AddRule(_ => true, authorize);
//     }
//
//     public AuthorizationSettingsBuilder<TType> All<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
//         Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool> authorize)
//     {
//         return AddRule(_ => true, authorize);
//     }
//
//     public AuthorizationSettingsBuilder<TType> All<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
//         Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool> authorize)
//     {
//         return AddRule(_ => true, authorize);
//     }
// }