namespace Webinex.ActiveRecord.Annotations;

[AttributeUsage(AttributeTargets.Parameter)]
public class ServiceAttribute : ParameterAttribute
{
    public ServiceAttribute(string? name = null) : base(ParameterSource.DependencyInjection, name)
    {
    }
}