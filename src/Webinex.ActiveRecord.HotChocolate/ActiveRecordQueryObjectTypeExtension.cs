using System.Reflection;
using System.Text.Json;
using HotChocolate.Types;
using Humanizer;
using Webinex.Asky;

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
            .Argument("query", a => a.Type<AnyType>())
            .Type<NonNullType<ListType<NonNullType<ActiveRecordGraphQL<TType>>>>>()
            // Disabled due to The field `Query.XXX` declares the data middleware `UseProjection` more than once.
            // .UseProjection<TType>()
            .Resolve(async ctx =>
            {
                var interactor = ctx.Service<IActiveRecordInteractor<TType>>();
                var queryArg = ctx.ArgumentValue<JsonElement?>("query");
                var query = queryArg != null ? Query.FromJson(queryArg, ctx.Service<IAskyFieldMap<TType>>()) : null;
                return await interactor.GetAllAsync(query);
            });

        descriptor
            .Field($"{settings.Definition.Name.Camelize()}ListSegment")
            .Argument("query", a => a.Type<AnyType>())
            .Argument("includeTotal", a => a.Type<BooleanType>().DefaultValue(true))
            .Type<NonNullType<ObjectType<ListSegment<TType>>>>()
            .Resolve(async ctx =>
            {
                var interactor = ctx.Service<IActiveRecordInteractor<TType>>();
                var queryArg = ctx.ArgumentValue<JsonElement?>("query");
                var includeTotal = ctx.ArgumentValue<bool?>("includeTotal") ?? true;
                var query = queryArg != null ? Query.FromJson(queryArg, ctx.Service<IAskyFieldMap<TType>>()) : null;
                return await interactor.ListSegmentAsync(query, includeTotal);
            });

        descriptor
            .Field(settings.Definition.Name.Camelize())
            .Argument(settings.Definition.Key.Name.Camelize(), a => a.Type(settings.Definition.Key.PropertyType))
            .Type<ActiveRecordGraphQL<TType>>()
            .Resolve(async ctx =>
            {
                var interactor = ctx.Service<IActiveRecordInteractor<TType>>();
                var key = ctx.ArgumentValue<object>(settings.Definition.Key.Name.Camelize());
                
                return await interactor.ByKeyAsync(key);
            });

        descriptor.Field($"{settings.Definition.Name.Camelize()}Count")
            .Argument("filterRule", a => a.Type<AnyType>())
            .Type<NonNullType<IntType>>()
            .Resolve(async ctx =>
            {
                var interactor = ctx.Service<IActiveRecordInteractor<TType>>();
                var filterRuleArg = ctx.ArgumentValue<JsonElement?>("filterRule");
                var filterRule = filterRuleArg != null ? FilterRule.FromJson(filterRuleArg, ctx.Service<IAskyFieldMap<TType>>()) : null;
                return await interactor.CountAsync(filterRule);
            });

        descriptor.Field($"{settings.Definition.Name.Camelize()}Any")
            .Argument("filterRule", a => a.Type<AnyType>())
            .Type<NonNullType<BooleanType>>()
            .Resolve(async ctx =>
            {
                var interactor = ctx.Service<IActiveRecordInteractor<TType>>();
                var filterRuleArg = ctx.ArgumentValue<JsonElement?>("filterRule");
                var filterRule = filterRuleArg != null ? FilterRule.FromJson(filterRuleArg, ctx.Service<IAskyFieldMap<TType>>()) : null;
                return await interactor.AnyAsync(filterRule);
            });
    }
}