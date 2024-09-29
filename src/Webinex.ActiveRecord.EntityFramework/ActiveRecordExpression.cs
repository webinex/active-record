using System.Collections;
using System.Linq.Expressions;

namespace Webinex.ActiveRecord;

internal class ActiveRecordExpression
{
    public static Expression<Func<TType, bool>> KeyIn<TType>(IActiveRecordSettings settings, IEnumerable<object> keys)
    {
        var key = settings.Definition.Key;
        var parameter = Expression.Parameter(typeof(TType), "x");
        var propertyAccess = Expression.MakeMemberAccess(parameter, key);

        var idList = Cast(settings.Definition.Key.PropertyType, keys);
        var constantValue = Expression.Constant(idList);

        var containsMethod = typeof(List<>).MakeGenericType(key.PropertyType).GetMethod(nameof(List<object>.Contains), [key.PropertyType])!;
        var containsExpression = Expression.Call(constantValue, containsMethod, Expression.Convert(propertyAccess, key.PropertyType));

        return Expression.Lambda<Func<TType, bool>>(containsExpression, parameter);
    }

    public static Expression<Func<TType, TKey>> GetKey<TType, TKey>(IActiveRecordSettings settings)
    {
        var key = settings.Definition.Key;
        var parameter = Expression.Parameter(typeof(TType), "x");
        var propertyAccess = Expression.MakeMemberAccess(parameter, key);
        Expression expression = key.PropertyType != typeof(TKey) ? Expression.Convert(propertyAccess, typeof(TKey)) : propertyAccess;
        return Expression.Lambda<Func<TType, TKey>>(expression, parameter);
    }
    
    private static IList Cast(Type type, IEnumerable<object> values)
    {
        var listType = typeof(List<>).MakeGenericType(type);
        var list = (IList)Activator.CreateInstance(listType)!;

        foreach (var value in values)
        {
            list.Add(Convert.ChangeType(value, type));
        }

        return list;
    }
}