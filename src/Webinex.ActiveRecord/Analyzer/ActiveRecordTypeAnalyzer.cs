using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Webinex.ActiveRecord.Annotations;

namespace Webinex.ActiveRecord;

public interface IActiveRecordTypeAnalyzer
{
    ActiveRecordDefinition GetDefinition(Type type);
}

public class ActiveRecordTypeAnalyzer : IActiveRecordTypeAnalyzer
{
    protected readonly IServiceCollection Services;
    protected readonly ActiveRecordTypeAnalyzerSettings Settings;

    public ActiveRecordTypeAnalyzer(IServiceCollection services, ActiveRecordTypeAnalyzerSettings settings)
    {
        Services = services;
        Settings = settings;
    }

    public ActiveRecordDefinition GetDefinition(Type type)
    {
        var name = GetName(type);
        var id = GetId(Services, type);
        var properties = GetPropertyDefinitions(type);
        var methods = GetMethodDefinitions(type);
        var authorize = GetAuthorize(type);

        return new ActiveRecordDefinition(
            type,
            id,
            properties,
            methods,
            name,
            authorize);
    }

    protected virtual Delegate? GetAuthorize(Type type)
    {
        var settings = Settings.AuthorizationSettings(type);
        return settings?.AccessExpressionFactory;
    }

    protected virtual string GetName(Type type)
    {
        return type.Name;
    }

    protected virtual PropertyInfo GetId(IServiceCollection _, Type type)
    {
        var id = type.GetProperties().FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
        id ??= type.GetProperties()
            .FirstOrDefault(x => "id".Equals(x.Name, StringComparison.InvariantCultureIgnoreCase));

        return id ?? throw new InvalidOperationException(
            "Unable to determine the primary key property. [Key] attribute might be specified or property name might be 'Id'.");
    }

    protected virtual ActiveRecordPropertyDefinition[] GetPropertyDefinitions(Type type)
    {
        var properties = type.GetProperties()
            .Where(x => x.CanRead && !x.IsSpecialName && x.DeclaringType != typeof(object))
            .Where(x => !Settings.IgnorePropertyPredicate(x));
        return properties.Select(ResolvePropertyDefinition).ToArray();
    }

    protected virtual ActiveRecordPropertyDefinition ResolvePropertyDefinition(
        PropertyInfo propertyInfo)
    {
        return new ActiveRecordPropertyDefinition(propertyInfo);
    }

    protected virtual ActiveRecordMethodDefinition[] GetMethodDefinitions(Type type)
    {
        var methods = GetMethods(type);
        return methods.Select(x => ResolveMethodDefinition(type, x)).ToArray();
    }

    protected virtual MethodInfo[] GetMethods(Type type)
    {
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Where(x => x is { IsAbstract: false, IsGenericMethod: false, IsSpecialName: false })
            .Where(x => x.DeclaringType != typeof(object))
            .Where(x => !x.IsStatic || x.ReturnType == type)
            .Where(x => !Settings.IgnoreMethodPredicate(x))
            .ToArray();

        return methods;
    }

    protected virtual ActiveRecordMethodDefinition ResolveMethodDefinition(
        Type type,
        MethodInfo methodInfo)
    {
        var name = GetName(methodInfo);
        var actionType = ResolveActionType(methodInfo);
        var parameters = GetMethodParameterDefinitions(methodInfo);
        var accessFilterFactory = GetAuthorize(type, methodInfo, actionType);
        return new ActiveRecordMethodDefinition(actionType, methodInfo, parameters, accessFilterFactory, name);
    }

    protected virtual string GetName(MethodInfo methodInfo)
    {
        return methodInfo.Name;
    }

    protected virtual Delegate? GetAuthorize(Type type, MethodInfo methodInfo, ActionType actionType)
    {
        var settings = Settings.AuthorizationSettings(type);
        return settings?.Rules.FirstOrDefault(rule => rule.MethodPredicate(methodInfo, actionType))?.Authorize;
    }

    protected virtual ActionType ResolveActionType(MethodInfo methodInfo)
    {
        if (CreateMethodRegex().IsMatch(methodInfo.Name))
        {
            return ActionType.Create;
        }

        if (DeleteMethodRegex().IsMatch(methodInfo.Name))
        {
            return ActionType.Delete;
        }

        return ActionType.Update;
    }

    protected virtual ActiveRecordParameterDefinition[] GetMethodParameterDefinitions(
        MethodInfo methodInfo)
    {
        var parameters = methodInfo.GetParameters();
        return parameters.Select(ResolveParameterDefinition).ToArray();
    }

    protected virtual ActiveRecordParameterDefinition ResolveParameterDefinition(
        ParameterInfo parameterInfo)
    {
        var source = ResolveParameterSource(parameterInfo);
        return new ActiveRecordParameterDefinition(source, parameterInfo);
    }

    protected virtual ParameterSource ResolveParameterSource(ParameterInfo parameterInfo)
    {
        var type = parameterInfo.ParameterType;
        var isService = Services.Any(
            x => x.ServiceType == type || (type.IsGenericType && x.ServiceType == type.GetGenericTypeDefinition()));
        return isService ? ParameterSource.DependencyInjection : ParameterSource.Body;
    }

    protected virtual Regex CreateMethodRegex() => new("^(Create|Add|New).*");
    protected virtual Regex DeleteMethodRegex() => new("^(Remove|Delete).*");
}