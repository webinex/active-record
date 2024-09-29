using System.Reflection;

namespace Webinex.ActiveRecord.AspNetCore;

internal static class Paths
{
    private static readonly string[] TRIM_PREFIXES =
        ["Update", "Set", "Patch", "Put", "Push", "Insert", "Add", "Create", "New", "Remove", "Delete"];

    private static readonly string[] TRIM_SUFFIXES = ["Async"];

    public static string FromMethod(string basePath, MethodInfo methodInfo)
    {
        var path = Format(methodInfo.Name);
        var id = methodInfo.IsStatic ? null : "{id}";
        var parts = new[] { basePath, id, path }.Where(x => !string.IsNullOrWhiteSpace(x));
        return string.Join("/", parts);
    }

    private static string Format(string methodName)
    {
        foreach (var prefix in TRIM_PREFIXES)
        {
            if (methodName.StartsWith(prefix) || methodName.StartsWith(Strings.PascalCaseToCamelCase(prefix)))
            {
                methodName = methodName.Substring(prefix.Length);
                break;
            }
        }

        foreach (var suffix in TRIM_SUFFIXES)
        {
            if (methodName.EndsWith(suffix))
            {
                methodName = methodName.Substring(0, methodName.Length - suffix.Length);
            }
        }

        return Strings.PascalCaseToKebabCase(methodName);
    }
}