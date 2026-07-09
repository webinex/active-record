using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using Webinex.Coded;
using Webinex.ActiveRecord.Annotations;
using Webinex.Asky;

namespace Webinex.ActiveRecord.Tests;

public class ActiveRecordInteractorTests
{
    private Mock<IServiceProvider> _serviceProviderMock = null!;
    private Mock<IActiveRecordInteractorRepository<TestRecord>> _repositoryMock = null!;
    private Mock<IActiveRecordAuthorizationService<TestRecord>> _authorizationServiceMock = null!;
    private ActiveRecordInteractor<TestRecord> _subject = null!;
    private TestRecord.Settings _settings = null!;

    [SetUp]
    public void SetUp()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _repositoryMock = new Mock<IActiveRecordInteractorRepository<TestRecord>>();
        _authorizationServiceMock = new Mock<IActiveRecordAuthorizationService<TestRecord>>();
        _settings = new TestRecord.Settings();

        _repositoryMock
            .Setup(x => x.WithDefaultPredicate(It.IsAny<Expression<Func<TestRecord, bool>>?>()))
            .Returns(_repositoryMock.Object);
        _repositoryMock
            .Setup(x => x.ListSegmentAsync(It.IsAny<Query?>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new ListSegment<TestRecord>([], -1));
        _repositoryMock
            .Setup(x => x.CountAsync(It.IsAny<FilterRule?>()))
            .ReturnsAsync(0);
        _repositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<FilterRule?>()))
            .ReturnsAsync(false);
        _repositoryMock
            .Setup(x => x.ByKeysAsync<object>(It.IsAny<IEnumerable<object>>(), It.IsAny<bool>()))
            .ReturnsAsync(Array.Empty<TestRecord>());

        _authorizationServiceMock
            .Setup(x => x.ExpressionAsync(It.IsAny<IActionContext<TestRecord>>()))
            .ReturnsAsync((Expression<Func<TestRecord, bool>>?)(x => true));
        _authorizationServiceMock
            .Setup(x => x.InvokeAsync(It.IsAny<IActionContext<TestRecord>>()))
            .ReturnsAsync(true);

        _subject = new ActiveRecordInteractor<TestRecord>(
            _serviceProviderMock.Object,
            NullLogger<ActiveRecordInteractor<TestRecord>>.Instance,
            _repositoryMock.Object,
            _settings,
            _authorizationServiceMock.Object);
    }

    [TestCase(nameof(ActiveRecordInteractor<TestRecord>.ListSegmentAsync), ActionType.GetAll)]
    [TestCase(nameof(ActiveRecordInteractor<TestRecord>.GetAllAsync), ActionType.GetAll)]
    [TestCase(nameof(ActiveRecordInteractor<TestRecord>.CountAsync), ActionType.GetAll)]
    [TestCase(nameof(ActiveRecordInteractor<TestRecord>.AnyAsync), ActionType.GetAll)]
    [TestCase(nameof(ActiveRecordInteractor<TestRecord>.ByKeyAsync), ActionType.GetByKey)]
    public async Task ReadOperations_ShouldRequestExpectedAuthorizationExpression(
        string methodName,
        ActionType expectedActionType)
    {
        IActionContext<TestRecord>? capturedContext = null;

        _authorizationServiceMock
            .Setup(x => x.ExpressionAsync(It.IsAny<IActionContext<TestRecord>>()))
            .Callback<IActionContext<TestRecord>>(context => capturedContext = context)
            .ReturnsAsync((Expression<Func<TestRecord, bool>>?)(x => true));

        switch (methodName)
        {
            case nameof(ActiveRecordInteractor<TestRecord>.ListSegmentAsync):
                await _subject.ListSegmentAsync();
                break;
            case nameof(ActiveRecordInteractor<TestRecord>.GetAllAsync):
                await _subject.GetAllAsync();
                break;
            case nameof(ActiveRecordInteractor<TestRecord>.CountAsync):
                await _subject.CountAsync();
                break;
            case nameof(ActiveRecordInteractor<TestRecord>.AnyAsync):
                await _subject.AnyAsync();
                break;
            case nameof(ActiveRecordInteractor<TestRecord>.ByKeyAsync):
                await _subject.ByKeyAsync(5);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(methodName), methodName, null);
        }

        capturedContext.ShouldNotBeNull();
        capturedContext.Type.ShouldBe(expectedActionType);
        _authorizationServiceMock.Verify(x => x.ExpressionAsync(It.IsAny<IActionContext<TestRecord>>()), Times.Once);
        _repositoryMock.Verify(x => x.WithDefaultPredicate(It.IsAny<Expression<Func<TestRecord, bool>>?>()), Times.Once);
    }

    [Test]
    public void InvokeAsync_WithoutIdForInstanceMethod_ShouldThrowInvalidOperationException()
    {
        var method = CreateMethodDefinition(nameof(TestRecord.Update));

        var exception = Should.Throw<InvalidOperationException>(
            () => _subject.InvokeAsync(method, id: null, body: new TestRecord.UpdateBody()).GetAwaiter().GetResult());

        exception.Message.ShouldBe("No id provided");
        _authorizationServiceMock.Verify(x => x.InvokeAsync(It.IsAny<IActionContext<TestRecord>>()), Times.Never);
        _repositoryMock.Verify(x => x.ByKeysAsync<object>(It.IsAny<IEnumerable<object>>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public async Task InvokeAsync_WithInstanceMethod_ShouldAuthorizeAgainstResolvedEntity()
    {
        var record = new TestRecord { Id = Guid.NewGuid(), Value = "before" };
        var body = new TestRecord.UpdateBody { Value = "after" };
        var method = CreateMethodDefinition(nameof(TestRecord.Update));
        IActionContext<TestRecord>? capturedContext = null;

        _repositoryMock
            .Setup(x => x.ByKeysAsync<object>(It.IsAny<IEnumerable<object>>(), It.IsAny<bool>()))
            .ReturnsAsync([record]);
        _authorizationServiceMock
            .Setup(x => x.InvokeAsync(It.IsAny<IActionContext<TestRecord>>()))
            .Callback<IActionContext<TestRecord>>(context => capturedContext = context)
            .ReturnsAsync(true);

        await _subject.InvokeAsync(method, record.Id, body);

        capturedContext.ShouldNotBeNull();
        var context = capturedContext;
        context.Type.ShouldBe(ActionType.Update);
        context.Instance.ShouldBeSameAs(record);
        context.Body.ShouldBeSameAs(body);
        _authorizationServiceMock.Verify(x => x.InvokeAsync(It.IsAny<IActionContext<TestRecord>>()), Times.Once);
    }

    [Test]
    public void InvokeAsync_WithInstanceMethod_WhenEntityNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var method = CreateMethodDefinition(nameof(TestRecord.Update));

        var exception = Should.Throw<CodedException>(
            () => _subject.InvokeAsync(method, id, new TestRecord.UpdateBody()).GetAwaiter().GetResult());

        exception.Failure.Code.ToString().ShouldBe("NTFND");
        _authorizationServiceMock.Verify(x => x.InvokeAsync(It.IsAny<IActionContext<TestRecord>>()), Times.Never);
    }

    [Test]
    public void InvokeAsync_WithInstanceMethod_WhenAuthorizationFails_ShouldThrowUnauthorizedAccessException()
    {
        var record = new TestRecord { Id = Guid.NewGuid(), Value = "before" };
        var body = new TestRecord.UpdateBody { Value = "after" };
        var method = CreateMethodDefinition(nameof(TestRecord.Update));

        _repositoryMock
            .Setup(x => x.ByKeysAsync<object>(It.IsAny<IEnumerable<object>>(), It.IsAny<bool>()))
            .ReturnsAsync([record]);
        _authorizationServiceMock
            .Setup(x => x.InvokeAsync(It.IsAny<IActionContext<TestRecord>>()))
            .ReturnsAsync(false);

        var exception = Should.Throw<UnauthorizedAccessException>(
            () => _subject.InvokeAsync(method, record.Id, body).GetAwaiter().GetResult());

        exception.ShouldNotBeNull();
        record.Value.ShouldBe("before");
    }

    [Test]
    public async Task InvokeAsync_WithStaticMethod_ShouldAuthorizeAgainstBody()
    {
        var body = new TestRecord.CreateBody { Value = "created" };
        var method = CreateMethodDefinition(nameof(TestRecord.Create));
        IActionContext<TestRecord>? capturedContext = null;

        _authorizationServiceMock
            .Setup(x => x.InvokeAsync(It.IsAny<IActionContext<TestRecord>>()))
            .Callback<IActionContext<TestRecord>>(context => capturedContext = context)
            .ReturnsAsync(true);
        _repositoryMock
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<TestRecord>>()))
            .ReturnsAsync((IEnumerable<TestRecord> entities) => entities.ToArray());

        await _subject.InvokeAsync(method, id: null, body);

        capturedContext.ShouldNotBeNull();
        var context = capturedContext;
        context.Type.ShouldBe(ActionType.Create);
        context.Instance.ShouldBeNull();
        context.Body.ShouldBeSameAs(body);
        _repositoryMock.Verify(
            x => x.AddRangeAsync(It.Is<IEnumerable<TestRecord>>(entities => entities.Single().Value == "created")),
            Times.Once);
    }

    [Test]
    public void InvokeAsync_WithStaticMethod_WhenAuthorizationFails_ShouldThrowUnauthorizedAccessException()
    {
        var body = new TestRecord.CreateBody { Value = "created" };
        var method = CreateMethodDefinition(nameof(TestRecord.Create));

        _authorizationServiceMock
            .Setup(x => x.InvokeAsync(It.IsAny<IActionContext<TestRecord>>()))
            .ReturnsAsync(false);

        var exception = Should.Throw<UnauthorizedAccessException>(
            () => _subject.InvokeAsync(method, id: null, body).GetAwaiter().GetResult());

        exception.ShouldNotBeNull();
        _repositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<TestRecord>>()), Times.Never);
    }

    [Test]
    public async Task InvokeAsync_WithStaticMethod_ShouldReturnAndPersistCreatedEntity()
    {
        var body = new TestRecord.CreateBody { Value = "created" };
        var method = CreateMethodDefinition(nameof(TestRecord.Create));
        TestRecord[]? addedEntities = null;

        _repositoryMock
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<TestRecord>>()))
            .Callback<IEnumerable<TestRecord>>(entities => addedEntities = entities.ToArray())
            .ReturnsAsync((IEnumerable<TestRecord> entities) => entities.ToArray());

        var result = await _subject.InvokeAsync(method, id: null, body);

        result.ShouldBeOfType<TestRecord>();
        var entity = (TestRecord)result;
        entity.Value.ShouldBe("created");
        addedEntities.ShouldNotBeNull();
        addedEntities.Single().ShouldBeSameAs(entity);
    }

    private ActiveRecordMethodDefinition CreateMethodDefinition(string methodName)
    {
        return _settings.Definition.Methods.Single(x => x.MethodInfo.Name == methodName);
    }

}
