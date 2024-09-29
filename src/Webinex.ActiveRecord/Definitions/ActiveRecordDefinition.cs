using System.Reflection;

namespace Webinex.ActiveRecord;

public class ActiveRecordDefinition
{
    public Type Type { get; }
    public string Name { get; private set; }
    public PropertyInfo Key { get; private set; }
    public IReadOnlyCollection<ActiveRecordMethodDefinition> Methods { get; private set; }
    public IReadOnlyCollection<ActiveRecordPropertyDefinition> Properties { get; private set; }
    public Delegate? Authorize { get; private set; }

    public ActiveRecordDefinition(
        Type type,
        PropertyInfo key,
        IEnumerable<ActiveRecordPropertyDefinition> properties,
        IEnumerable<ActiveRecordMethodDefinition> methods,
        string? name = null,
        Delegate? authorize = null)
    {
        type = type ?? throw new ArgumentNullException(nameof(type));
        methods = methods?.ToArray() ?? throw new ArgumentNullException(nameof(methods));
        properties = properties?.ToArray() ?? throw new ArgumentNullException(nameof(properties));

        if (authorize != null)
            Guards.Authorize(authorize, type);

        Type = type;
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Properties = properties.ToArray();
        Methods = methods.ToArray();
        Name = name ?? type.Name;
        Authorize = authorize;
    }

    public ActiveRecordDefinition(ActiveRecordDefinition value)
    {
        Type = value.Type;
        Name = value.Name;
        Key = value.Key;
        Methods = value.Methods;
        Authorize = value.Authorize;
        Properties = value.Properties;
    }

    public ActiveRecordDefinition WithName(string name)
    {
        name = name ?? throw new ArgumentNullException(nameof(name));
        return new ActiveRecordDefinition(this)
        {
            Name = name
        };
    }

    public ActiveRecordDefinition WithKey(PropertyInfo key)
    {
        key = key ?? throw new ArgumentNullException(nameof(key));
        return new ActiveRecordDefinition(this)
        {
            Key = key
        };
    }

    public ActiveRecordDefinition WithMethods(IEnumerable<ActiveRecordMethodDefinition> methods)
    {
        methods = methods?.ToArray() ?? throw new ArgumentNullException(nameof(methods));
        return new ActiveRecordDefinition(this)
        {
            Methods = methods.ToArray(),
        };
    }

    public ActiveRecordDefinition WithProperties(IEnumerable<ActiveRecordPropertyDefinition> properties)
    {
        properties = properties?.ToArray() ?? throw new ArgumentNullException(nameof(properties));
        return new ActiveRecordDefinition(this)
        {
            Properties = properties.ToArray(),
        };
    }

    public ActiveRecordDefinition WithAuthorize(Delegate? value)
    {
        if (value != null)
            Guards.Authorize(value, Type);

        return new ActiveRecordDefinition(this)
        {
            Authorize = value
        };
    }
}