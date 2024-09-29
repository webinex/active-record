namespace Webinex.ActiveRecord.AspNetCore;

internal static class TypeConvert
{
    public static bool TryConvert(string value, Type type, out object? result)
    {
        if (type == typeof(string))
        {
            result = value;
            return true;
        }
        
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (type is IConvertible)
        {
            result = Convert.ChangeType(value, type);
            return true;
        }

        if (type == typeof(Guid))
        {
            result = Guid.Parse(value);
            return true;
        }

        if (type == typeof(DateTimeOffset))
        {
            result = DateTimeOffset.Parse(value);
            return true;
        }

        if (type == typeof(TimeSpan))
        {
            result = TimeSpan.Parse(value);
            return true;
        }

        if (type == typeof(DateOnly))
        {
            result = DateOnly.Parse(value);
            return true;
        }

        if (type == typeof(TimeOnly))
        {
            result = TimeOnly.Parse(value);
            return true;
        }

        result = default;
        return false;
    }
}