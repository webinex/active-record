using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Webinex.ActiveRecord;

public class ActiveRecordServiceConfiguration
{
    private IActiveRecordTypeAnalyzer? _typeAnalyzer;
    private readonly List<ActiveRecordConfiguration> _records = new();

    public IReadOnlyCollection<IActiveRecordConfiguration> Records => _records.AsReadOnly();
    public BindingBehavior BindingBehavior { get; private set; } = BindingBehavior.Explicit;

    public IActiveRecordTypeAnalyzer TypeAnalyzer
    {
        get => _typeAnalyzer ?? new ActiveRecordTypeAnalyzer(Services, TypeAnalyzerSettings);
        private set => _typeAnalyzer = value;
    }

    public ActiveRecordTypeAnalyzerSettings TypeAnalyzerSettings { get; private set; } = new();
    public IServiceCollection Services { get; }

    private ActiveRecordServiceConfiguration(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
        Services.TryAddTransient(typeof(IActiveRecordService<>), typeof(ActiveRecordService<>));
        Services.TryAddTransient(typeof(IActiveRecordAuthorizationService<>), typeof(ActiveRecordAuthorizationService<>));
    }

    public ActiveRecordServiceConfiguration UseBinding(BindingBehavior value)
    {
        if (!Enum.IsDefined(typeof(BindingBehavior), value))
            throw new ArgumentOutOfRangeException(nameof(value), value, "Invalid binding behavior.");

        BindingBehavior = value;
        return this;
    }

    public ActiveRecordServiceConfiguration UseTypeAnalyzer(IActiveRecordTypeAnalyzer typeAnalyzer)
    {
        TypeAnalyzer = typeAnalyzer ?? throw new ArgumentNullException(nameof(typeAnalyzer));
        return this;
    }

    public ActiveRecordServiceConfiguration ConfigureTypeAnalyzer(
        Func<ActiveRecordTypeAnalyzerSettings, ActiveRecordTypeAnalyzerSettings> configure)
    {
        configure = configure ?? throw new ArgumentNullException(nameof(configure));
        TypeAnalyzerSettings = configure(TypeAnalyzerSettings) ?? throw new InvalidOperationException();
        return this;
    }

    public ActiveRecordServiceConfiguration Add<TType>(Action<IActiveRecordConfiguration<TType>>? configure = null)
        where TType : class
    {
        if (Records.Any(x => x.Type == typeof(TType)))
            throw new InvalidOperationException($"Type {typeof(TType).Name} is already registered.");

        var configuration = new ActiveRecordConfiguration<TType>(this, null);
        configure?.Invoke(configuration);
        _records.Add(configuration);
        return this;
    }

    internal static ActiveRecordServiceConfiguration GetOrCreate(IServiceCollection services)
    {
        var instance = (ActiveRecordServiceConfiguration?)services.FirstOrDefault(
                x =>
                    x.ImplementationInstance is ActiveRecordServiceConfiguration)
            ?.ImplementationInstance;

        if (instance != null)
            return instance;

        instance = new ActiveRecordServiceConfiguration(services);
        services.AddSingleton(instance);
        return instance;
    }
}