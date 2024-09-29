namespace Webinex.ActiveRecord.AspNetCore;

internal static class Strings
{
    public static string PascalCaseToKebabCase(string pascalCase)
    {
        var kebabCase = string.Concat(pascalCase.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x : x.ToString()));
        return kebabCase.ToLower();
    }
    
    public static string PascalCaseToCamelCase(string pascalCase)
    {
        var camelCase = char.ToLower(pascalCase[0]) + pascalCase.Substring(1);
        return camelCase;
    }
}