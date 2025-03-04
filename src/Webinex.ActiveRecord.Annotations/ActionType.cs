namespace Webinex.ActiveRecord.Annotations;

[Flags]
public enum ActionType
{
    GetByKey = 1,
    GetAll = 2,
    Create = 4,
    Update = 8,
    Delete = 16,
}