namespace Webinex.ActiveRecord.Annotations;

[AttributeUsage(AttributeTargets.Parameter)]
public class BodyAttribute : ParameterAttribute
{
    public BodyAttribute(string? name = null) : base(ParameterSource.Body, name)
    {
    }
}