using System.Linq.Expressions;

namespace Webinex.ActiveRecord;

internal static class Guards
{
    public static void Authorize(Delegate value, Type recordType)
    {
        
        value = value ?? throw new ArgumentNullException(nameof(value));
        var type = value.GetType();

        if (type.Namespace != "System")
            throw Throw(recordType);
            

        if (!type.Name.StartsWith("Func`"))
            throw Throw(recordType);

        var returnType = type.GetMethod("Invoke")!.ReturnType;
        AssertExpectedReturnType(returnType, recordType);
    }

    private static void AssertExpectedReturnType(Type retType, Type recordType)
    {
        if (!retType.IsGenericType || retType.GetGenericTypeDefinition() != typeof(Expression<>))
            throw Throw(recordType);

        var funcType = retType.GetGenericArguments()[0];
        if (!funcType.IsGenericType || funcType.GetGenericTypeDefinition() != typeof(Func<,>))
            throw Throw(recordType);

        var funcArg0 = funcType.GetGenericArguments()[0];
        var funcArg1 = funcType.GetGenericArguments()[1];
        
        if (!funcArg0.IsAssignableFrom(recordType) || funcArg1 != typeof(bool))
            throw Throw(recordType);
    }

    private static Exception Throw(Type recordType)
    {
        throw new InvalidOperationException(
            $"Might be System.Func<TService1, TService2, ..., Expression<Func<{recordType.Name}, bool>>>");
    }
}