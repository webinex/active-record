using System.Reflection;
using Webinex.ActiveRecord.Annotations;

namespace Webinex.ActiveRecord;

public class ActiveRecordMethodDefinition
{
    public ActionType Type { get; private set; }
    public string Name { get; }
    public MethodInfo MethodInfo { get; }
    public IReadOnlyCollection<ActiveRecordParameterDefinition> Parameters { get; }
    public Delegate? Authorize { get; }

    public ActiveRecordMethodDefinition(
        ActionType type,
        MethodInfo methodInfo,
        IEnumerable<ActiveRecordParameterDefinition> parameters,
        Delegate? authorize = null,
        string? name = null)
    {
        parameters = parameters?.ToArray() ?? throw new ArgumentNullException(nameof(parameters));

        if (!Enum.IsDefined(typeof(ActionType), type))
            throw new ArgumentOutOfRangeException(nameof(type), type, null);

        if (methodInfo.IsGenericMethod)
            throw new InvalidOperationException(
                $"Generic methods are not supported. {methodInfo.DeclaringType?.Name}.{methodInfo.Name}");

        var bodyCount = parameters.Count(p => p.ParameterSource == ParameterSource.Body);
        if (bodyCount > 1)
            throw new InvalidOperationException(
                $"Only one parameter can be with body source. {methodInfo.DeclaringType?.Name}.{methodInfo.Name}");

        Type = type;
        MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
        Parameters = parameters.ToArray();
        Name = name ?? methodInfo.Name;
        Authorize = authorize;
    }
}