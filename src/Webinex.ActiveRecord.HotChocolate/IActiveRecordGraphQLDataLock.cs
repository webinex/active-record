namespace Webinex.ActiveRecord.HotChocolate;

public interface IActiveRecordGraphQLDataLock
{
    Task<IDisposable> LockAsync();
    Task WaitAsync();
    void Release();
}