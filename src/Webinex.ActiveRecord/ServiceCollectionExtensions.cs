using Microsoft.Extensions.DependencyInjection;

namespace Webinex.ActiveRecord;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddActiveRecordService(this IServiceCollection services, Action<ActiveRecordServiceConfiguration> configure)
    {
        var configuration = ActiveRecordServiceConfiguration.GetOrCreate(services);
        configure(configuration);
        return services;
    }
}