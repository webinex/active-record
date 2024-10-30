using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Webinex.Asky;

namespace Webinex.ActiveRecord.AspNetCore;

public static class ActiveRecordEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapActiveRecords(this IEndpointRouteBuilder endpoints,
        Action<ActiveRecordServiceRouteConfiguration> configure)
    {
        var configuration = new ActiveRecordServiceRouteConfiguration(endpoints.ServiceProvider);
        configure(configuration);

        foreach (var route in configuration.Routes)
        {
            MapActiveRecordRoute(endpoints, route);
        }

        return endpoints;
    }

    private static IEndpointRouteBuilder MapActiveRecordRoute(
        this IEndpointRouteBuilder endpoints,
        ActiveRecordRouteConfiguration routeConfiguration)
    {
        var mapType =
            typeof(Map<,>).MakeGenericType(routeConfiguration.Type, routeConfiguration.Definition.Key.PropertyType);
        var map = (IMap)Activator.CreateInstance(mapType, endpoints, routeConfiguration)!;
        return map.Bind();
    }

    private interface IMap
    {
        IEndpointRouteBuilder Bind();
    }

    private class Map<TType, TId> : IMap
        where TType : class
    {
        private readonly ActiveRecordRouteConfiguration _configuration;
        private readonly IEndpointRouteBuilder _endpoints;

        public Map(IEndpointRouteBuilder endpoints, ActiveRecordRouteConfiguration configuration)
        {
            _endpoints = endpoints;
            _configuration = configuration;
        }

        public IEndpointRouteBuilder Bind()
        {
            var basePath = _configuration.Route;
            var name = _configuration.Definition.Name;

            ConfigureRoute(
                _endpoints.MapGet(
                        basePath,
                        async (
                            [FromServices] IActiveRecordService<TType> service,
                            [FromServices] IOptions<JsonOptions> jsonOptions,
                            [FromQuery] string? query,
                            [FromServices] IAskyFieldMap<TType>? fieldMap = null) =>
                        {
                            var deserializer = new ActiveRecordQueryDeserializer<TType>(fieldMap, jsonOptions);
                            var queryObject = query != null ? await deserializer.DeserializeAsync(query) : null;
                            return await service.QueryAsync(queryObject);
                        })
                    .WithName($"GetAll{name}")
                    .WithTags(name));

            ConfigureRoute(
                _endpoints.MapGet(
                        basePath + "/{id}",
                        async (
                                [FromRoute(Name = "id")] TId id,
                                [FromServices] IActiveRecordService<TType> repository) =>
                            await repository.ByKeyAsync(id!))
                    .WithName($"Get{name}")
                    .WithTags(name));

            foreach (var method in _configuration.Definition.Methods)
            {
                var factory = ActiveRecordDynamicRouteHandlerBuilderFactory.New(_endpoints, _configuration, method);
                ConfigureRoute(factory.Create(), method);
            }

            return _endpoints;
        }

        private void ConfigureRoute(
            RouteHandlerBuilder handler,
            ActiveRecordMethodDefinition? method = null)
        {
            _configuration.ConfigureRoute(handler, _configuration.Definition, method);
        }
    }
}