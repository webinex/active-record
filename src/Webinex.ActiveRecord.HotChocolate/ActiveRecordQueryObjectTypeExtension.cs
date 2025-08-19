using System.Reflection;
using System.Text.Json;
using HotChocolate.Types;
using Humanizer;

namespace Webinex.ActiveRecord.HotChocolate;

internal class ActiveRecordQueryObjectTypeExtension : ObjectTypeExtension
{
    private readonly IEnumerable<IActiveRecordSettings> _settings;

    public ActiveRecordQueryObjectTypeExtension(IEnumerable<IActiveRecordSettings> settings)
    {
        _settings = settings;
    }

    protected override void Configure(IObjectTypeDescriptor descriptor)
    {
        descriptor.Name("Query");

        foreach (var x in _settings)
        {
            typeof(ActiveRecordQueryObjectTypeExtension).GetMethod(nameof(AddType),
                    BindingFlags.Instance | BindingFlags.NonPublic)!
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
            // Disabled due to The field `Query.XXX` declares the data middleware `UseProjection` more than once.
            // .UseProjection<TType>()
            .Resolve(async ctx =>
            {
                var service = ctx.Service<IActiveRecordService<TType>>();
                var queryDeserializer = ctx.Service<ActiveRecordQueryDeserializer<TType>>();
                var queryArg = ctx.ArgumentValue<string?>("query");
                var query = queryArg != null ? await queryDeserializer.DeserializeAsync(queryArg) : null;
                return await service.QueryAsync(query);
            });

        descriptor
            .Field(settings.Definition.Name.Camelize())
            .Argument(settings.Definition.Key.Name.Camelize(), a => a.Type(settings.Definition.Key.PropertyType))
            .Type<ActiveRecordGraphQL<TType>>()
            .Resolve(async ctx =>
            {
                var service = ctx.Service<IActiveRecordService<TType>>();
                return await service.ByKeyAsync(
                    ctx.ArgumentValue<object>(settings.Definition.Key.Name.Camelize()));
            });

        descriptor.Field($"{settings.Definition.Name.Pascalize()}Count")
            .Argument("filterRule", a => a.Type<JsonType>())
            .Type<NonNullType<IntType>>()
            .Resolve(async ctx =>
            {
                var service = ctx.Service<IActiveRecordService<TType>>();
                var queryDeserializer = ctx.Service<ActiveRecordQueryDeserializer<TType>>();
                return await service.CountAsync(
                    queryDeserializer.DeserializeFilterRule(ctx.ArgumentValue<JsonElement?>("filterRule")));
            });
    }
}