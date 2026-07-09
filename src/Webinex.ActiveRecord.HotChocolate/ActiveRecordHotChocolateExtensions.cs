using HotChocolate.Execution.Configuration;
using HotChocolate.Types;
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
        builder.Services.AddScoped(typeof(ActiveRecordBatchDataLoader<,>));
        builder.Services.TryAddSingleton<IActiveRecordGraphQLDataLock, NoLockActiveRecordGraphQLDataLock>();

        var configuration = (ActiveRecordServiceConfiguration?)builder.Services
            .FirstOrDefault(x => x.ServiceType == typeof(ActiveRecordServiceConfiguration))?.ImplementationInstance;
        
        if (configuration == null)
            throw new InvalidOperationException(
                "ActiveRecordsConfiguration is not registered in the service collection. Please call IServiceCollection.AddActiveRecords() before GraphQL configuration.");

        foreach (var record in configuration.Records)
        {
            var settingsDescriptor = builder.Services.FirstOrDefault(x =>
                x.ServiceType == typeof(IActiveRecordSettings<>).MakeGenericType(record.Type));
            
            if (settingsDescriptor?.ImplementationInstance == null)
                throw new InvalidOperationException(
                    $"IActiveRecordSettings<{record.Type.Name}> is not registered in the service collection. Please call services.AddActiveRecordService(o => o.Add<{record.Type.Name}>() before GraphQL configuration.");

            var settings = settingsDescriptor.ImplementationInstance;
            
            var graphQLType = (ITypeDefinition)Activator.CreateInstance(
                typeof(ActiveRecordGraphQL<>).MakeGenericType(record.Type),
                settings)!;
            
            builder.AddType(graphQLType);
        }

        return builder;
    }
}