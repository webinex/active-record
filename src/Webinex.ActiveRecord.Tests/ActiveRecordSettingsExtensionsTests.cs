using Microsoft.Extensions.DependencyInjection;

namespace Webinex.ActiveRecord.Tests;

public class ActiveRecordExpressionTests
{
    [Test]
    public void GetKey_WhenKeyTypeMatch_ShouldNotThrow()
    {
        new ActiveRecordSettings().GetKeyExpression<TestClass, Guid>();
    }

    private class ActiveRecordSettings : IActiveRecordSettings
    {
        public IDictionary<string, object> Data { get; } = new Dictionary<string, object>();
        public Type Type => typeof(TestClass);
        public ActiveRecordDefinition Definition => TestClass.Definition;
    }
    
    private class TestClass
    {
        public Guid Id { get; } = Guid.NewGuid();

        public static ActiveRecordDefinition Definition =>
            new ActiveRecordTypeAnalyzer(new ServiceCollection(), new ActiveRecordTypeAnalyzerSettings()).GetDefinition(
                typeof(TestClass));
    }
}