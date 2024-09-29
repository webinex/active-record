namespace Webinex.ActiveRecord.HotChocolate;

internal class NoLockActiveRecordGraphQLDataLock : IActiveRecordGraphQLDataLock
{
    public Task<IDisposable> LockAsync()
    {
        return Task.FromResult<IDisposable>(new EmptyDisposable());
    }

    public Task WaitAsync()
    {
        return Task.CompletedTask;
    }

    public void Release()
    {
    }

    private class EmptyDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}