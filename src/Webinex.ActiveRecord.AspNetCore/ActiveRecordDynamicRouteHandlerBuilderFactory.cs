using System.Net.Mime;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Webinex.ActiveRecord.Annotations;

namespace Webinex.ActiveRecord.AspNetCore;

public abstract class ActiveRecordDynamicRouteHandlerBuilderFactory
{
    public abstract RouteHandlerBuilder Create();

    public static ActiveRecordDynamicRouteHandlerBuilderFactory New(
        IEndpointRouteBuilder endpoint,
        ActiveRecordRouteConfiguration configuration,
        ActiveRecordMethodDefinition method)
    {
        return (ActiveRecordDynamicRouteHandlerBuilderFactory)Activator.CreateInstance(
            typeof(ActiveRecordDynamicRouteHandlerBuilderFactory<>).MakeGenericType(configuration.Definition.Type),
            endpoint,
            configuration,
            method)!;
    }
}

public class ActiveRecordDynamicRouteHandlerBuilderFactory<TType> : ActiveRecordDynamicRouteHandlerBuilderFactory
{
    private readonly IEndpointRouteBuilder _endpoint;
    private readonly ActiveRecordRouteConfiguration _configuration;
    private readonly ActiveRecordMethodDefinition _method;

    public ActiveRecordDynamicRouteHandlerBuilderFactory(
        IEndpointRouteBuilder endpoint,
        ActiveRecordRouteConfiguration configuration,
        ActiveRecordMethodDefinition method)
    {
        _endpoint = endpoint;
        _configuration = configuration;
        _method = method;
    }

    private string BasePath => _configuration.Route;
    private string Path => Paths.FromMethod(BasePath, _method.MethodInfo);
    private string HttpMethod => _method.Type.ToHttpMethod();
    private PropertyInfo Key => _configuration.Definition.Key;

    private ActiveRecordParameterDefinition? BodyParam =>
        _method.Parameters.FirstOrDefault(x => x.ParameterSource == ParameterSource.Body);


    public override RouteHandlerBuilder Create()
    {
        var endpoint = CreateBase()
            .WithTags(_configuration.Definition.Name);

        if (BodyParam != null)
            endpoint = endpoint.Accepts(BodyParam.ParameterInfo.ParameterType, MediaTypeNames.Application.Json);

        var routeTemplate = TemplateParser.Parse(Path);

        endpoint = endpoint.WithOpenApi(
            x =>
            {
                foreach (var parameter in routeTemplate.Parameters)
                {
                    x.Parameters.Add(
                        new OpenApiParameter
                        {
                            Name = parameter.Name,
                            In = ParameterLocation.Path,
                            Required = true,
                        });
                }

                return x;
            });

        _configuration.ConfigureRoute(endpoint, _configuration.Definition, _method);
        return endpoint;
    }

    private RouteHandlerBuilder CreateBase()
    {
        return _endpoint.MapMethods(
            Path,
            [HttpMethod],
            async (HttpContext context) =>
            {
                var id = ResolveKey(context);
                var service = ResolveService(context);
                var body = await ResolveBodyAsync(context);
                var result = await service.InvokeAsync(_method, id, body);

                if (result is Type type && type == typeof(void))
                    return Results.Ok();

                return Results.Ok(result);
            });
    }

    private IActiveRecordService<TType> ResolveService(HttpContext context)
    {
        return context.RequestServices.GetRequiredService<IActiveRecordService<TType>>();
    }

    private object? ResolveKey(HttpContext context)
    {
        if (_method.MethodInfo.IsStatic)
            return null;

        var value = context.GetRouteValue("id")?.ToString();
        if (value == null)
            throw new InvalidOperationException("No id provided");

        if (!TypeConvert.TryConvert(value, Key.PropertyType, out var result))
        {
            throw new InvalidOperationException($"Unable to convert {value} to type {Key.PropertyType.Name}");
        }

        return result;
    }

    private async Task<object?> ResolveBodyAsync(HttpContext context)
    {
        if (BodyParam == null)
            return null;

        try
        {
            var options = context.RequestServices.GetRequiredService<IOptions<JsonOptions>>();
            var result = await context.Request.ReadFromJsonAsync(
                BodyParam!.ParameterInfo.ParameterType,
                options.Value.JsonSerializerOptions);
            return result;
        }
        catch (Exception ex)
        {
            Logger(context).LogError("Failed to read body: {0}", ex.Message);
            throw;
        }
    }

    private ILogger Logger(HttpContext context)
    {
        return context.RequestServices
            .GetRequiredService<ILogger<ActiveRecordDynamicRouteHandlerBuilderFactory<TType>>>();
    }
}