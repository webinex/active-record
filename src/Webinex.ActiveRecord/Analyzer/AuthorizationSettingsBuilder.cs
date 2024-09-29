using System.Linq.Expressions;
using System.Reflection;

namespace Webinex.ActiveRecord;

public record AuthorizationRule(Func<MethodInfo, bool> Predicate, Delegate Rule);

public class AuthorizationSettings
{
    protected readonly List<AuthorizationRule> _rules = new();

    public Delegate? AuthorizeDelegate { get; protected set; }
    public IReadOnlyCollection<AuthorizationRule> Rules => _rules.AsReadOnly();
}

public class AuthorizationSettingsBuilder<TType> : AuthorizationSettings
{
    internal AuthorizationSettingsBuilder()
    {
    }

    public void Unauthorized()
    {
        AuthorizeDelegate = null;
    }

    public AuthorizationSettingsBuilder<TType> Authorize<T1>(Func<T1, Expression<Func<TType, bool>>> authorize)
    {
        AuthorizeDelegate = authorize;
        return this;
    }

    public AuthorizationSettingsBuilder<TType> Authorize<T1, T2>(
        Func<T1, T2, Expression<Func<TType, bool>>> authorize)
    {
        AuthorizeDelegate = authorize;
        return this;
    }

    public AuthorizationSettingsBuilder<TType> Authorize<T1, T2, T3>(
        Func<T1, T2, T3, Expression<Func<TType, bool>>> authorize)
    {
        AuthorizeDelegate = authorize;
        return this;
    }

    public AuthorizationSettingsBuilder<TType> Authorize<T1, T2, T3, T4>(
        Func<T1, T2, T3, T4, Expression<Func<TType, bool>>> authorize)
    {
        AuthorizeDelegate = authorize;
        return this;
    }

    public AuthorizationSettingsBuilder<TType> Authorize<T1, T2, T3, T4, T5>(
        Func<T1, T2, T3, T4, T5, Expression<Func<TType, bool>>> authorize)
    {
        AuthorizeDelegate = authorize;
        return this;
    }

    public AuthorizationSettingsBuilder<TType> Authorize<T1, T2, T3, T4, T5, T6>(
        Func<T1, T2, T3, T4, T5, T6, Expression<Func<TType, bool>>> authorize)
    {
        AuthorizeDelegate = authorize;
        return this;
    }

    public AuthorizationSettingsBuilder<TType> Authorize<T1, T2, T3, T4, T5, T6, T7>(
        Func<T1, T2, T3, T4, T5, T6, T7, Expression<Func<TType, bool>>> authorize)
    {
        AuthorizeDelegate = authorize;
        return this;
    }

    public AuthorizationSettingsBuilder<TType> Authorize<T1, T2, T3, T4, T5, T6, T7, T8>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, Expression<Func<TType, bool>>> authorize)
    {
        AuthorizeDelegate = authorize;
        return this;
    }

    public AuthorizationSettingsBuilder<TType> Authorize<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Expression<Func<TType, bool>>> authorize)
    {
        AuthorizeDelegate = authorize;
        return this;
    }

    public AuthorizationSettingsBuilder<TType> Authorize<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Expression<Func<TType, bool>>> authorize)
    {
        AuthorizeDelegate = authorize;
        return this;
    }

    public AuthorizationSettingsBuilder<TType> When<T1>(Func<MethodInfo, bool> predicate, Func<T1, bool> authorize)
    {
        return AddRule(predicate, authorize);
    }

    public AuthorizationSettingsBuilder<TType> When<T1, T2>(
        Func<MethodInfo, bool> predicate,
        Func<T1, T2, bool> authorize)
    {
        return AddRule(predicate, authorize);
    }

    public AuthorizationSettingsBuilder<TType> When<T1, T2, T3>(
        Func<MethodInfo, bool> predicate,
        Func<T1, T2, T3, bool> authorize)
    {
        return AddRule(predicate, authorize);
    }

    public AuthorizationSettingsBuilder<TType> When<T1, T2, T3, T4>(
        Func<MethodInfo, bool> predicate,
        Func<T1, T2, T3, T4, bool> authorize)
    {
        return AddRule(predicate, authorize);
    }

    public AuthorizationSettingsBuilder<TType> When<T1, T2, T3, T4, T5>(
        Func<MethodInfo, bool> predicate,
        Func<T1, T2, T3, T4, T5, bool> authorize)
    {
        return AddRule(predicate, authorize);
    }

    public AuthorizationSettingsBuilder<TType> When<T1, T2, T3, T4, T5, T6>(
        Func<MethodInfo, bool> predicate,
        Func<T1, T2, T3, T4, T5, T6, bool> authorize)
    {
        return AddRule(predicate, authorize);
    }

    public AuthorizationSettingsBuilder<TType> When<T1, T2, T3, T4, T5, T6, T7>(
        Func<MethodInfo, bool> predicate,
        Func<T1, T2, T3, T4, T5, T6, T7, bool> authorize)
    {
        return AddRule(predicate, authorize);
    }

    public AuthorizationSettingsBuilder<TType> When<T1, T2, T3, T4, T5, T6, T7, T8>(
        Func<MethodInfo, bool> predicate,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, bool> authorize)
    {
        return AddRule(predicate, authorize);
    }

    public AuthorizationSettingsBuilder<TType> When<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        Func<MethodInfo, bool> predicate,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool> authorize)
    {
        return AddRule(predicate, authorize);
    }

    public AuthorizationSettingsBuilder<TType> When<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        Func<MethodInfo, bool> predicate,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool> authorize)
    {
        return AddRule(predicate, authorize);
    }

    private AuthorizationSettingsBuilder<TType> AddRule(Func<MethodInfo, bool> predicate, Delegate rule)
    {
        _rules.Add(new AuthorizationRule(predicate, rule));
        return this;
    }

    public AuthorizationSettingsBuilder<TType> All<T1>(Func<T1, bool> authorize)
    {
        return AddRule(_ => true, authorize);
    }

    public AuthorizationSettingsBuilder<TType> All<T1, T2>(Func<T1, T2, bool> authorize)
    {
        return AddRule(_ => true, authorize);
    }

    public AuthorizationSettingsBuilder<TType> All<T1, T2, T3>(Func<T1, T2, T3, bool> authorize)
    {
        return AddRule(_ => true, authorize);
    }

    public AuthorizationSettingsBuilder<TType> All<T1, T2, T3, T4>(Func<T1, T2, T3, T4, bool> authorize)
    {
        return AddRule(_ => true, authorize);
    }

    public AuthorizationSettingsBuilder<TType> All<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, bool> authorize)
    {
        return AddRule(_ => true, authorize);
    }

    public AuthorizationSettingsBuilder<TType> All<T1, T2, T3, T4, T5, T6>(
        Func<T1, T2, T3, T4, T5, T6, bool> authorize)
    {
        return AddRule(_ => true, authorize);
    }

    public AuthorizationSettingsBuilder<TType> All<T1, T2, T3, T4, T5, T6, T7>(
        Func<T1, T2, T3, T4, T5, T6, T7, bool> authorize)
    {
        return AddRule(_ => true, authorize);
    }

    public AuthorizationSettingsBuilder<TType> All<T1, T2, T3, T4, T5, T6, T7, T8>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, bool> authorize)
    {
        return AddRule(_ => true, authorize);
    }

    public AuthorizationSettingsBuilder<TType> All<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool> authorize)
    {
        return AddRule(_ => true, authorize);
    }

    public AuthorizationSettingsBuilder<TType> All<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool> authorize)
    {
        return AddRule(_ => true, authorize);
    }
}