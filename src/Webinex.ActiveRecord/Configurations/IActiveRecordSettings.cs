using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Webinex.ActiveRecord;

public interface IActiveRecordSettings<TType> : IActiveRecordSettings;

public interface IActiveRecordSettings
{
    IDictionary<string, object> Data { get; }
    Type Type { get; }
    ActiveRecordDefinition Definition { get; }
}

public static class ActiveRecordSettingsExtension
{
    private static readonly ConditionalWeakTable<Tuple<IActiveRecordSettings, Type>, Expression> KeyExpressionCache = new();
    private static readonly ConditionalWeakTable<Tuple<IActiveRecordSettings, Type>, Delegate> KeyFuncCache = new();
    
    public static Expression<Func<TType, TKey>> GetKeyExpression<TType, TKey>(this IActiveRecordSettings settings)
    {
        var cacheKey = Tuple.Create(settings, typeof(TKey));
        
        if (KeyExpressionCache.TryGetValue(cacheKey, out var cachedExpression))
            return (Expression<Func<TType, TKey>>)cachedExpression;
        
        var result = settings.NewKeyExpression<TType, TKey>();
        KeyExpressionCache.AddOrUpdate(cacheKey, result);
        return result;
    }
    
    private static Expression<Func<TType, TKey>> NewKeyExpression<TType, TKey>(this IActiveRecordSettings settings)
    {
        var key = settings.Definition.Key;
        var parameter = Expression.Parameter(typeof(TType), "x");
        var propertyAccess = Expression.MakeMemberAccess(parameter, key);
        Expression expression = key.PropertyType != typeof(TKey) ? Expression.Convert(propertyAccess, typeof(TKey)) : propertyAccess;
        return Expression.Lambda<Func<TType, TKey>>(expression, parameter);
    }
    
    public static Func<TType, TKey> GetKeyFunc<TType, TKey>(this IActiveRecordSettings settings)
    {
        var cacheKey = Tuple.Create(settings, typeof(TKey));
        
        if (KeyFuncCache.TryGetValue(cacheKey, out var cachedFunc))
            return (Func<TType, TKey>)cachedFunc;
        
        var result = settings.NewKeyFunc<TType, TKey>();
        KeyFuncCache.AddOrUpdate(cacheKey, result);
        return result;
    }
    
    private static Func<TType, TKey> NewKeyFunc<TType, TKey>(this IActiveRecordSettings settings)
    {
        var keyExpression = settings.NewKeyExpression<TType, TKey>();
        return keyExpression.Compile();
    }
}