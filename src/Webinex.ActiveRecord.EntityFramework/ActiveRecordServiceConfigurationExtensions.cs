using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Webinex.ActiveRecord;

public static class ActiveRecordServiceConfigurationExtensions
{
    public static ActiveRecordServiceConfiguration UseDbContext<TDbContext>(this ActiveRecordServiceConfiguration @this)
        where TDbContext : DbContext
    {
        @this.Services.TryAddTransient(typeof(IActiveRecordRepository<>), typeof(ActiveRecordRepository<>));
        @this.Services.TryAddTransient(typeof(IActiveRecordServiceRepository<>), typeof(ActiveRecordRepository<>));
        @this.Services.AddTransient<IActiveRecordDbContextProvider, ActiveRecordDbContextProvider<TDbContext>>();
        return @this;
    }
}