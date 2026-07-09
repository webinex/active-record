using System.Linq.Expressions;
using Moq;
using Shouldly;
using Webinex.ActiveRecord.Annotations;

namespace Webinex.ActiveRecord.Tests;

public class ActiveRecordAuthorizationServiceTests
{
    private Mock<IServiceProvider> _serviceProviderMock = null!;
    private ActiveRecordAuthorizationService<TestRecord> _subject = null!;
    private TestDependency _dependency = null!;
    private TestRecord.Settings _settings = null!;

    [SetUp]
    public void SetUp()
    {
        _dependency = new TestDependency();
        _settings = new TestRecord.Settings();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceProviderMock
            .Setup(x => x.GetService(typeof(TestDependency)))
            .Returns(_dependency);
        _subject = new ActiveRecordAuthorizationService<TestRecord>(_serviceProviderMock.Object);
    }

    [Test]
    public async Task ExpressionAsync_WithActionContextAndDependency_ShouldBuildPredicate()
    {
        _dependency.ExpectedOwnerId = 42;
        var definition = CreateDefinition(
            (TestDependency dependency, IActionContext<TestRecord> context) =>
                (Expression<Func<TestRecord, bool>>)(x =>
                    x.OwnerId == dependency.ExpectedOwnerId && context.Type == ActionType.GetByKey));
        var context = new ActionContext<TestRecord>(
            _serviceProviderMock.Object,
            ActionType.GetByKey,
            definition,
            methodDefinition: null,
            instance: null,
            body: null);

        var predicate = await _subject.ExpressionAsync(context);

        predicate.ShouldNotBeNull();
        predicate.Compile()(new TestRecord { OwnerId = 42 }).ShouldBeTrue();
        predicate.Compile()(new TestRecord { OwnerId = 10 }).ShouldBeFalse();
    }

    [Test]
    public async Task InvokeAsync_WithEntityAndDependency_ShouldUseExpectedArguments()
    {
        _dependency.ExpectedOwnerId = 7;
        var methodDefinition = CreateMethodDefinition(
            nameof(TestRecord.Update),
            (TestDependency dependency, TestRecord record, IActionContext<TestRecord> context) =>
                dependency.ExpectedOwnerId == record.OwnerId &&
                context.Type == ActionType.Update);
        var context = new ActionContext<TestRecord>(
            _serviceProviderMock.Object,
            ActionType.Update,
            CreateDefinition(),
            methodDefinition,
            new TestRecord { OwnerId = 7 },
            body: new TestRecord.UpdateBody { OwnerId = 7 });

        var allowed = await _subject.InvokeAsync(context);

        allowed.ShouldBeTrue();
    }

    [Test]
    public async Task InvokeAsync_WithBodyParameter_ShouldUseContextBody()
    {
        var body = new TestRecord.UpdateBody { OwnerId = 15 };
        var methodDefinition = CreateMethodDefinition(
            nameof(TestRecord.Update),
            (TestRecord.UpdateBody request) => request.OwnerId == 15);
        var context = new ActionContext<TestRecord>(
            _serviceProviderMock.Object,
            ActionType.Update,
            CreateDefinition(),
            methodDefinition,
            new TestRecord { OwnerId = 15 },
            body);

        var allowed = await _subject.InvokeAsync(context);

        allowed.ShouldBeTrue();
    }

    private static ActiveRecordDefinition CreateDefinition(Delegate? authorize = null)
    {
        return TestRecord.Definition.WithAuthorize(authorize);
    }

    private ActiveRecordMethodDefinition CreateMethodDefinition(
        string methodName,
        Delegate authorize)
    {
        var method = _settings.Definition.Methods.Single(x => x.MethodInfo.Name == methodName);
        return new ActiveRecordMethodDefinition(method.Type, method.MethodInfo, method.Parameters, authorize, method.Name);
    }

    private sealed class TestDependency
    {
        public int ExpectedOwnerId { get; set; }
    }
}
