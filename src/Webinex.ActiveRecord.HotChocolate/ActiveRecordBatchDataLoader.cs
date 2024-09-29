using GreenDonut;

namespace Webinex.ActiveRecord.HotChocolate;

public class ActiveRecordBatchDataLoader<TType> : BatchDataLoader<object, TType>
{
    private readonly IActiveRecordRepository<TType> _repository;
    private readonly IActiveRecordGraphQLDataLock _lock;

    public ActiveRecordBatchDataLoader(
        IBatchScheduler batchScheduler,
        IActiveRecordRepository<TType> repository,
        IActiveRecordGraphQLDataLock @lock,
        DataLoaderOptions? options = null) : base(
        batchScheduler,
        options)
    {
        _repository = repository;
        _lock = @lock;
    }

    protected override async Task<IReadOnlyDictionary<object, TType>> LoadBatchAsync(
        IReadOnlyList<object> keys,
        CancellationToken cancellationToken)
    {
        using var _ = await _lock.LockAsync();
        return await _repository.MapAsync(keys);
    }
}