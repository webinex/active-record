using Microsoft.EntityFrameworkCore;

namespace Webinex.ActiveRecord;

internal class ActiveRecordDbContextProvider<TDbContext> : IActiveRecordDbContextProvider where TDbContext : DbContext
{
    public DbContext Value { get; }

    public ActiveRecordDbContextProvider(TDbContext value)
    {
        Value = value;
    }
}