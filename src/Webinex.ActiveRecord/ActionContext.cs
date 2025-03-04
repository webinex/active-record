using Webinex.ActiveRecord.Annotations;

namespace Webinex.ActiveRecord;

public class ActionContext<TType> : IActionContext<TType>
{
    public IServiceProvider Services { get; }
    public ActionType Type { get; }
    public TType? Instance { get; }
    public object? Body { get; }
    public ActiveRecordDefinition Definition { get; }
    public ActiveRecordMethodDefinition? MethodDefinition { get; }
    object? IActionContext.Instance => Instance;

    public ActionContext(
        IServiceProvider services,
        ActionType type,
        ActiveRecordDefinition definition,
        ActiveRecordMethodDefinition? methodDefinition,
        TType? instance,
        object? body)
    {
        Type = type;
        Instance = instance;
        Body = body;
        Services = services;
        Definition = definition;
        MethodDefinition = methodDefinition;
    }
}

public interface IActionContext
{
    ActionType Type { get; }
    object? Instance { get; }
    object? Body { get; }
    IServiceProvider Services { get; }
    ActiveRecordDefinition Definition { get; }
    ActiveRecordMethodDefinition? MethodDefinition { get; }
}

public interface IActionContext<TType> : IActionContext
{
    new TType? Instance { get; }
}