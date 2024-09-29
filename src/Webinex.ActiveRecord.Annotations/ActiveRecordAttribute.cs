namespace Webinex.ActiveRecord.Annotations;

[AttributeUsage(AttributeTargets.Class)]
public class ActiveRecordAttribute : Attribute
{
    public string? Name { get; }

    public ActiveRecordAttribute(string? name = null)
    {
        Name = name;
    }
}