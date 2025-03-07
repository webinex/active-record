using FluentAssertions;

namespace Webinex.ActiveRecord.Tests;

public class ActiveRecordTypeAnalyzerTests
{
    [Test]
    public void GetDefinition_WithStaticNewMethods_ShouldReturnBothSyncAndAsync()
    {
        var subject = new ActiveRecordTypeAnalyzer(null!, new ActiveRecordTypeAnalyzerSettings());

        var definition = subject.GetDefinition(typeof(TestClass));

        definition.Methods.Should().BeEquivalentTo(
            new[]
            {
                new
                {
                    Name = nameof(TestClass.New),
                },
                new
                {
                    Name = nameof(TestClass.NewAsync),
                },
                new
                {
                    Name = nameof(TestClass.NewValueAsync),
                }
            });
    }

    private class TestClass
    {
        public Guid Id { get; set; }

        public static TestClass New()
        {
            return new TestClass();
        }

        public static Task<TestClass> NewAsync()
        {
            return Task.FromResult(new TestClass());
        }

        public static ValueTask<TestClass> NewValueAsync()
        {
            return ValueTask.FromResult(new TestClass());
        }
    }
}