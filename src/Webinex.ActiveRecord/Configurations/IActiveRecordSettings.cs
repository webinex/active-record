namespace Webinex.ActiveRecord;

public interface IActiveRecordSettings<TType> : IActiveRecordSettings;

public interface IActiveRecordSettings
{
    IDictionary<string, object> Data { get; }
    Type Type { get; }
    ActiveRecordDefinition Definition { get; }
}