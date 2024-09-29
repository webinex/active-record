using Microsoft.AspNetCore.Http;
using Webinex.ActiveRecord.Annotations;

namespace Webinex.ActiveRecord.AspNetCore;

internal static class ActiveRecordMethodTypeExtensions
{
    public static string ToHttpMethod(this ActionType value)
    {
        return value switch
        {
            ActionType.Create => HttpMethods.Post,
            ActionType.Update => HttpMethods.Put,
            ActionType.Delete => HttpMethods.Delete,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }
}