using Microsoft.Extensions.DependencyInjection;

namespace Webinex.ActiveRecord;

public interface IActiveRecordConfiguration<TType> : IActiveRecordConfiguration;

public interface IActiveRecordConfiguration
{
    IDictionary<string, object> Data { get; }
    IServiceCollection Services { get; }

    /// <summary>
    ///     The type of the active record.
    /// </summary>
    Type Type { get; }

    /// <summary>
    ///     Binding behavior. When Implicit, the active record will be bound to the route automatically.
    ///     When Explicit, the active record will not be bound to the route automatically and must be bound manually.
    /// </summary>
    BindingBehavior BindingBehavior { get; }

    /// <summary>
    ///     The type analyzer to perform implicit binding.
    /// </summary>
    IActiveRecordTypeAnalyzer TypeAnalyzer { get; }

    /// <summary>
    ///     The definition of the active record.
    ///     Not null when active record registered with definition instead of type.
    /// </summary>
    ActiveRecordDefinition? Definition { get; }

    /// <summary>
    ///     Specifies the binding behavior of the active record.
    ///     When Implicit, the active record will be bound to the route automatically.
    ///     When Explicit, the active record will not be bound to the route automatically and must be bound manually.
    /// </summary>
    IActiveRecordConfiguration UseBinding(BindingBehavior value);

    /// <summary>
    ///     Specifies the type analyzer to perform implicit binding.
    /// </summary>
    IActiveRecordConfiguration UseTypeAnalyzer(IActiveRecordTypeAnalyzer typeAnalyzer);

    /// <summary>
    ///     Allows to perform custom configuration of the active record definition.
    /// </summary>
    IActiveRecordConfiguration PostConfigureDefinition(Func<ActiveRecordDefinition, ActiveRecordDefinition> configure);
}

public static class ActiveRecordConfigurationExtensions
{
    public static IActiveRecordConfiguration UseImplicitBinding(this IActiveRecordConfiguration configuration)
    {
        configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        return configuration.UseBinding(BindingBehavior.Implicit);
    }

    public static IActiveRecordConfiguration UseExplicitBinding(this IActiveRecordConfiguration configuration)
    {
        configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        return configuration.UseBinding(BindingBehavior.Explicit);
    }
}