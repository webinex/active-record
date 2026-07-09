using GreenDonut;

namespace Webinex.ActiveRecord.HotChocolate;

public class ActiveRecordBatchDataLoader<TType, TKey> : BatchDataLoader<TKey, TType>
    where TKey : notnull
{
    private readonly IActiveRecordRepository<TType> _repository;
    private readonly IActiveRecordSettings<TType> _settings;
    private readonly IActiveRecordGraphQLDataLock _lock;

    public ActiveRecordBatchDataLoader(
        IBatchScheduler batchScheduler,
        IActiveRecordRepository<TType> repository,
        IActiveRecordGraphQLDataLock @lock,
        DataLoaderOptions options,
        IActiveRecordSettings<TType> settings) : base(
        batchScheduler,
        options)
    {
        _repository = repository;
        _lock = @lock;
        _settings = settings;
    }

    protected override async Task<IReadOnlyDictionary<TKey, TType>> LoadBatchAsync(
        IReadOnlyList<TKey> keys,
        CancellationToken cancellationToken)
    {
        using var _ = await _lock.LockAsync();
        var result = await _repository.ByKeysAsync(keys);
        var keyBy = _settings.GetKeyFunc<TType, TKey>();
        return result.ToDictionary(keyBy);
    }
}