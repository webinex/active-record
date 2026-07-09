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
                            [FromServices] IActiveRecordInteractor<TType> interactor,
                            [FromServices] IOptions<JsonOptions> jsonOptions,
                            [FromQuery] string? query,
                            [FromServices] IAskyFieldMap<TType>? fieldMap = null) =>
                        {
                            if (!string.IsNullOrWhiteSpace(query) && fieldMap == null)
                                throw new InvalidOperationException(
                                    $"`query` parameter is provided but {nameof(IAskyFieldMap<>)} is not registered for type {typeof(TType).Name}. Please register it in DI or remove query parameter.");

                            var q = !string.IsNullOrWhiteSpace(query) ? Query.FromJson(query, fieldMap!, jsonOptions.Value.JsonSerializerOptions) : null;
                            return await interactor.GetAllAsync(q);
                        })
                    .WithName($"{name}_GetAll")
                    .WithTags(name));

            ConfigureRoute(
                _endpoints.MapGet(
                        basePath + "/list-segment",
                        async (
                            [FromServices] IActiveRecordInteractor<TType> interactor,
                            [FromServices] IOptions<JsonOptions> jsonOptions,
                            [FromQuery] string? query,
                            [FromQuery] bool includeTotal = true,
                            [FromServices] IAskyFieldMap<TType>? fieldMap = null) =>
                        {
                            if (!string.IsNullOrWhiteSpace(query) && fieldMap == null)
                                throw new InvalidOperationException(
                                    $"`query` parameter is provided but {nameof(IAskyFieldMap<>)} is not registered for type {typeof(TType).Name}. Please register it in DI or remove query parameter.");

                            var q = !string.IsNullOrWhiteSpace(query) ? Query.FromJson(query, fieldMap!, jsonOptions.Value.JsonSerializerOptions) : null;
                            return await interactor.ListSegmentAsync(q, includeTotal);
                        })
                    .WithName($"{name}_ListSegment")
                    .WithTags(name));
            
            ConfigureRoute(
                _endpoints.MapGet(
                        basePath + "/count",
                        async (
                            [FromServices] IActiveRecordInteractor<TType> interactor,
                            [FromServices] IOptions<JsonOptions> jsonOptions,
                            [FromQuery] string? filter,
                            [FromServices] IAskyFieldMap<TType>? fieldMap = null) =>
                        {
                            if (!string.IsNullOrWhiteSpace(filter) && fieldMap == null)
                                throw new InvalidOperationException(
                                    $"`filter` parameter is provided but {nameof(IAskyFieldMap<>)} is not registered for type {typeof(TType).Name}. Please register it in DI or remove filter parameter.");

                            var filterRule = !string.IsNullOrWhiteSpace(filter) ? FilterRule.FromJson(filter, fieldMap!) : null;
                            return await interactor.CountAsync(filterRule);
                        })
                    .WithName($"{name}_Count")
                    .WithTags(name));
            
            ConfigureRoute(
                _endpoints.MapGet(
                        basePath + "/any",
                        async (
                            [FromServices] IActiveRecordInteractor<TType> interactor,
                            [FromServices] IOptions<JsonOptions> jsonOptions,
                            [FromQuery] string? filter,
                            [FromServices] IAskyFieldMap<TType>? fieldMap = null) =>
                        {
                            if (!string.IsNullOrWhiteSpace(filter) && fieldMap == null)
                                throw new InvalidOperationException(
                                    $"`filter` parameter is provided but {nameof(IAskyFieldMap<>)} is not registered for type {typeof(TType).Name}. Please register it in DI or remove filter parameter.");

                            var filterRule = !string.IsNullOrWhiteSpace(filter) ? FilterRule.FromJson(filter, fieldMap!) : null;
                            return await interactor.AnyAsync(filterRule);
                        })
                    .WithName($"{name}_Any")
                    .WithTags(name));

            ConfigureRoute(
                _endpoints.MapGet(
                        basePath + "/{id}",
                        async (
                                [FromRoute(Name = "id")] TId id,
                                [FromServices] IActiveRecordInteractor<TType> repository) =>
                            await repository.ByKeyAsync(id!))
                    .WithName($"{name}_Get")
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