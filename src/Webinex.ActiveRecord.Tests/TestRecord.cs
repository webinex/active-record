using Microsoft.Extensions.DependencyInjection;

namespace Webinex.ActiveRecord.Tests;

public sealed class TestRecord
{
    public Guid Id { get; set; }
    public int OwnerId { get; set; }
    public string? Value { get; set; }

    public void Update(UpdateBody body)
    {
        OwnerId = body.OwnerId ?? OwnerId;
        Value = body.Value ?? Value;
    }

    public static TestRecord Create(CreateBody body)
    {
        return new TestRecord { Value = body.Value };
    }

    public static ActiveRecordDefinition Definition { get; } =
        new ActiveRecordTypeAnalyzer(new ServiceCollection(), new ActiveRecordTypeAnalyzerSettings())
            .GetDefinition(typeof(TestRecord));

    public sealed class UpdateBody
    {
        public int? OwnerId { get; init; }
        public string? Value { get; init; }
    }

    public sealed class CreateBody
    {
        public string? Value { get; init; }
    }

    public sealed class Settings : IActiveRecordSettings<TestRecord>
    {
        public IDictionary<string, object> Data { get; } = new Dictionary<string, object>();
        public Type Type => typeof(TestRecord);
        public ActiveRecordDefinition Definition => TestRecord.Definition;
    }
}
