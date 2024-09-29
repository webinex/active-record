using System.Reflection;
using Webinex.ActiveRecord.Annotations;

namespace Webinex.ActiveRecord;

public class ActiveRecordParameterDefinition
{
    public ActiveRecordParameterDefinition(ParameterSource parameterSource, ParameterInfo parameterInfo)
    {
        if (!Enum.IsDefined(typeof(ParameterSource), parameterSource))
            throw new ArgumentOutOfRangeException(nameof(parameterSource), parameterSource, null);
        
        ParameterSource = parameterSource;
        ParameterInfo = parameterInfo ?? throw new ArgumentNullException(nameof(parameterInfo));
    }

    public ParameterSource ParameterSource { get; }
    public ParameterInfo ParameterInfo { get; }
}