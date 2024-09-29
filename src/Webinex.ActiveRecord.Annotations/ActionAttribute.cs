namespace Webinex.ActiveRecord.Annotations;

[AttributeUsage(AttributeTargets.Method)]
public class ActionAttribute : Attribute
{
    public string? Name { get; }
    public ActionType? Type { get; }

    public ActionAttribute()
    {
    }

    public ActionAttribute(ActionType type)
    {
        Type = type;
    }

    public ActionAttribute(ActionType type, string? name = null)
    {
        Name = name;
        Type = type;
    }

    public ActionAttribute(string name)
    {
        Name = name;
    }
}