using System.Reflection;

namespace Webinex.ActiveRecord;

public class ActiveRecordTypeAnalyzerSettings
{
    public Func<PropertyInfo, bool> IgnorePropertyPredicate { get; protected set; }
    public Func<MethodInfo, bool> IgnoreMethodPredicate { get; protected set; }
    public Func<Type, object?> AuthorizationConventionFactory { get; protected set; }

    public ActiveRecordTypeAnalyzerSettings()
    {
        IgnorePropertyPredicate = _ => false;
        IgnoreMethodPredicate = _ => false;
        AuthorizationConventionFactory = _ => null;
    }

    public ActiveRecordTypeAnalyzerSettings(ActiveRecordTypeAnalyzerSettings value)
    {
        IgnorePropertyPredicate = value.IgnorePropertyPredicate;
        IgnoreMethodPredicate = value.IgnoreMethodPredicate;
        AuthorizationConventionFactory = value.AuthorizationConventionFactory;
    }

    public ActiveRecordTypeAnalyzerSettings IgnoreProperty(Func<PropertyInfo, bool> predicate)
    {
        var prev = IgnorePropertyPredicate;

        return new ActiveRecordTypeAnalyzerSettings(this)
        {
            IgnorePropertyPredicate = prop => prev(prop) || predicate(prop),
        };
    }

    public ActiveRecordTypeAnalyzerSettings IgnoreMethod(Func<MethodInfo, bool> predicate)
    {
        var prev = IgnoreMethodPredicate;

        return new ActiveRecordTypeAnalyzerSettings(this)
        {
            IgnoreMethodPredicate = method => prev(method) || predicate(method),
        };
    }

    public ActiveRecordTypeAnalyzerSettings UseAuthorizationConvention(Func<Type, object?> factory)
    {
        return new ActiveRecordTypeAnalyzerSettings(this)
        {
            AuthorizationConventionFactory = factory,
        };
    }

    internal AuthorizationSettings? AuthorizationSettings(Type type)
    {
        return (AuthorizationSettings?)GetType().GetMethod(
                nameof(AuthorizationSettingsTyped),
                BindingFlags.Instance | BindingFlags.NonPublic)
            ?.MakeGenericMethod(type)
            .Invoke(this, null);
    }

    private AuthorizationSettings? AuthorizationSettingsTyped<TType>()
    {
        var convention = AuthorizationConvention<TType>();
        if (convention == null) return null;

        var builder = new AuthorizationSettingsBuilder<TType>();
        convention.Configure(builder);
        return builder;
    }

    private IActionAuthorizationConvention<TType>? AuthorizationConvention<TType>()
    {
        return (IActionAuthorizationConvention<TType>?)AuthorizationConventionFactory(typeof(TType));
    }
}