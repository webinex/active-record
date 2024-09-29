using System.Linq.Expressions;

namespace Webinex.ActiveRecord;

internal static class Guards
{
    public static void Authorize(Delegate value, Type recordType)
    {
        Exception Throw()
        {
            throw new InvalidOperationException(
                $"Might be System.Func<TService1, TService2, ..., Expression<Func<{recordType.Name}, bool>>>");
        }
        
        value = value ?? throw new ArgumentNullException(nameof(value));
        var type = value.GetType();

        if (type.Namespace != "System")
            throw Throw();
            

        if (!type.Name.StartsWith("Func`"))
            throw Throw();

        var returnType = type.GetMethod("Invoke")!.ReturnType;
        var expectedReturnType = typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(recordType, typeof(bool)));
        if (returnType != expectedReturnType)
            throw Throw();
    }
}