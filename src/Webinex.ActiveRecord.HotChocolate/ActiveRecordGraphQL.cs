using System.Linq.Expressions;
using System.Reflection;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Webinex.ActiveRecord.HotChocolate;

public static class ActiveRecordGraphQL
{
    public static INamedType New(Type type, IServiceProvider sp)
    {
        return (INamedType)Activator.CreateInstance(
            typeof(ActiveRecordGraphQL<>).MakeGenericType(type),
            sp.GetRequiredService(typeof(IActiveRecordSettings<>).MakeGenericType(type)))!;
    }
}

public class ActiveRecordGraphQL<TType> : ObjectType<TType>
{
    private readonly IActiveRecordSettings<TType> _settings;

    public ActiveRecordGraphQL(IActiveRecordSettings<TType> settings)
    {
        _settings = settings;
    }

    protected override void Configure(IObjectTypeDescriptor<TType> descriptor)
    {
        descriptor.BindFieldsExplicitly();
        descriptor.Name(typeof(TType).Name);

        var addFieldMethodInfo =
            typeof(ActiveRecordGraphQL<>).MakeGenericType(typeof(TType))
                .GetMethod(nameof(AddField), BindingFlags.Instance | BindingFlags.NonPublic)!;

        var properties = _settings.Definition.Properties;

        foreach (var property in properties)
        {
            addFieldMethodInfo.MakeGenericMethod(property.PropertyInfo.PropertyType)
                .Invoke(this, [descriptor, property.PropertyInfo]);
        }

        base.Configure(descriptor);
    }

    private void AddField<TProp>(IObjectTypeDescriptor<TType> descriptor, PropertyInfo property)
    {
        var paramExpression = Expression.Parameter(typeof(TType), "x");
        var memberAccessExpression = Expression.PropertyOrField(paramExpression, property.Name);
        var lambdaExpression = Expression.Lambda<Func<TType, TProp?>>(memberAccessExpression, paramExpression);

        descriptor.Field(lambdaExpression);
    }
}