namespace Webinex.ActiveRecord.Annotations;

[AttributeUsage(AttributeTargets.Parameter)]
public class ParameterAttribute : Attribute
{
    public ParameterSource Source { get; }
    public string? Name { get; }

    public ParameterAttribute(ParameterSource source, string? name = null)
    {
        Source = source;
        Name = name;
    }
}