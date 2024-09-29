using System.Reflection;

namespace Webinex.ActiveRecord;

public class ActiveRecordPropertyDefinition
{
    public ActiveRecordPropertyDefinition(PropertyInfo propertyInfo)
    {
        PropertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));
    }

    public PropertyInfo PropertyInfo { get; }
}