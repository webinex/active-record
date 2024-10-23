using System.Reflection;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types;
using Humanizer;
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
        builder.AddTypeExtension(typeof(QueryObjectTypeExtension));
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

    private class QueryObjectTypeExtension : ObjectTypeExtension
    {
        private readonly IEnumerable<IActiveRecordSettings> _settings;

        public QueryObjectTypeExtension(IEnumerable<IActiveRecordSettings> settings)
        {
            _settings = settings;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name("Query");

            foreach (var x in _settings)
            {
                typeof(QueryObjectTypeExtension).GetMethod(nameof(AddType), BindingFlags.Instance | BindingFlags.NonPublic)!
                    .MakeGenericMethod(x.Type)
                    .Invoke(this, [descriptor, x]);
            }

            base.Configure(descriptor);
        }

        private void AddType<TType>(IObjectTypeDescriptor descriptor, IActiveRecordSettings settings)
        {
            descriptor
                .Field(settings.Definition.Name.Pluralize(inputIsKnownToBeSingular: false).Camelize())
                .Argument("query", a => a.Type<StringType>())
                .Type<NonNullType<ListType<NonNullType<ActiveRecordGraphQL<TType>>>>>()
                // TODO: s.skalaban, resolve issue with projection
                // .UseProjection()
                .Resolve(
                    async ctx =>
                    {
                        var service = ctx.Service<IActiveRecordService<TType>>();
                        var queryDeserializer = ctx.Service<ActiveRecordQueryDeserializer<TType>>();
                        var queryArg = ctx.ArgumentOptional<string>("query");
                        var query = queryArg.HasValue ? await queryDeserializer.DeserializeAsync(queryArg.Value) : null;
                        return await service.QueryAsync(query);
                    });

            descriptor
                .Field(settings.Definition.Name.Camelize())
                .Argument(settings.Definition.Key.Name.Camelize(), a => a.Type(settings.Definition.Key.PropertyType))
                .Type<ActiveRecordGraphQL<TType>>()
                .Resolve(
                    async ctx =>
                    {
                        var service = ctx.Service<IActiveRecordService<TType>>();
                        return await service.ByKeyAsync(
                            ctx.ArgumentValue<object>(settings.Definition.Key.Name.Camelize()));
                    });
        }
    }
}