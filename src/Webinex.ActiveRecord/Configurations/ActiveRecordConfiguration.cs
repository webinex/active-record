using Microsoft.Extensions.DependencyInjection;

namespace Webinex.ActiveRecord;

internal class ActiveRecordConfiguration<TType> : ActiveRecordConfiguration, IActiveRecordConfiguration<TType>,
    IActiveRecordSettings<TType>
{
    internal ActiveRecordConfiguration(
        ActiveRecordServiceConfiguration @base,
        ActiveRecordDefinition? definition) : base(
        @base,
        typeof(TType),
        definition)
    {
        @base.Services.AddSingleton<IActiveRecordSettings<TType>>(this);
    }
}

internal class ActiveRecordConfiguration : IActiveRecordConfiguration, IActiveRecordSettings
{
    private ActiveRecordDefinition? _definition;
    private BindingBehavior? _bindingBehavior;
    private IActiveRecordTypeAnalyzer? _typeAnalyzer;

    public IDictionary<string, object> Data { get; } = new Dictionary<string, object>();
    public IServiceCollection Services => Base.Services;
    public Type Type { get; }
    public ActiveRecordServiceConfiguration Base { get; }
    public BindingBehavior BindingBehavior => _bindingBehavior ?? Base.BindingBehavior;
    public IActiveRecordTypeAnalyzer TypeAnalyzer => _typeAnalyzer ?? Base.TypeAnalyzer;
    public ActiveRecordDefinition? Definition { get; }
    public Func<ActiveRecordDefinition, ActiveRecordDefinition>? PostConfigureDefinitionDelegate { get; private set; }

    ActiveRecordDefinition IActiveRecordSettings.Definition => GetOrCreateDefinition();

    internal ActiveRecordConfiguration(
        ActiveRecordServiceConfiguration @base,
        Type type,
        ActiveRecordDefinition? definition)
    {
        Base = @base;
        Type = type;
        Definition = definition;
        @base.Services.AddSingleton<IActiveRecordSettings>(this);
    }

    public IActiveRecordConfiguration UseBinding(BindingBehavior value)
    {
        ResetDefinition();

        if (!Enum.IsDefined(typeof(BindingBehavior), value))
            throw new ArgumentOutOfRangeException(nameof(value), value, "Invalid binding behavior.");

        _bindingBehavior = value;
        return this;
    }

    public IActiveRecordConfiguration UseTypeAnalyzer(IActiveRecordTypeAnalyzer typeAnalyzer)
    {
        ResetDefinition();

        _typeAnalyzer = typeAnalyzer ?? throw new ArgumentNullException(nameof(typeAnalyzer));
        return this;
    }

    public IActiveRecordConfiguration PostConfigureDefinition(
        Func<ActiveRecordDefinition, ActiveRecordDefinition> configure)
    {
        ResetDefinition();

        PostConfigureDefinitionDelegate = configure ?? throw new ArgumentNullException(nameof(configure));
        return this;
    }

    private ActiveRecordDefinition GetOrCreateDefinition()
    {
        if (_definition != null)
            return _definition;

        var definition = TypeAnalyzer.GetDefinition(Type);
        definition = PostConfigureDefinitionDelegate != null ? PostConfigureDefinitionDelegate(definition) : definition;

        return _definition = definition;
    }

    private void ResetDefinition()
    {
        _definition = null;
    }
}