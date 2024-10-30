using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Webinex.ActiveRecord.HotChocolate;

public static class ActiveRecordHotChocolateExtensions
{
    /// <summary>
    ///     Add Active Record types to the GraphQL schema.
    ///     This method should be called after active record configuration completed.
    /// </summary>
    /// <param name="builder"><see cref="IRequestExecutorBuilder"/></param>
    /// <returns><see cref="IRequestExecutorBuilder"/></returns>
    /// <exception cref="InvalidOperationException">When called before any IServiceCollection.AddActiveRecordService calls</exception>
    public static IRequestExecutorBuilder AddActiveRecordTypes(this IRequestExecutorBuilder builder)
    {
        builder.AddTypeExtension(typeof(ActiveRecordQueryObjectTypeExtension));
        builder.Services.AddScoped(typeof(ActiveRecordBatchDataLoader<>));
        builder.Services.TryAddSingleton<IActiveRecordGraphQLDataLock, NoLockActiveRecordGraphQLDataLock>();
        builder.Services.TryAddSingleton(typeof(ActiveRecordQueryDeserializer<>), typeof(ActiveRecordQueryDeserializer<>));

        var configuration = (ActiveRecordServiceConfiguration?)builder.Services
            .FirstOrDefault(x => x.ServiceType == typeof(ActiveRecordServiceConfiguration))?.ImplementationInstance;
        
        if (configuration == null)
            throw new InvalidOperationException(
                "ActiveRecordsConfiguration is not registered in the service collection. Please call IServiceCollection.AddActiveRecords() before GraphQL configuration.");

        foreach (var record in configuration.Records)
        {
            builder.AddType(sp => ActiveRecordGraphQL.New(record.Type, sp));
        }

        return builder;
    }
}