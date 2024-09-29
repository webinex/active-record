using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Webinex.ActiveRecord.AspNetCore;

public class ActiveRecordServiceRouteConfiguration
{
    public string Route { get; private set; }
    public IReadOnlyCollection<ActiveRecordRouteConfiguration> Routes { get; }

    public Action<
        RouteHandlerBuilder,
        ActiveRecordDefinition,
        ActiveRecordMethodDefinition?>? ConfigureRoute { get; private set; }

    internal ActiveRecordServiceRouteConfiguration(IServiceProvider services)
    {
        Route = "/api";

        var settings = services.GetServices<IActiveRecordSettings>().ToArray();
        Routes = settings.Select(x => new ActiveRecordRouteConfiguration(this, x)).ToArray();
    }

    public ActiveRecordServiceRouteConfiguration UseRoute(string route)
    {
        route = route ?? throw new ArgumentNullException(nameof(route));
        if (!route.StartsWith("/"))
            throw new ArgumentException($"Might start with \"/\". Value = {route}", nameof(route));

        Route = route.TrimEnd('/');
        return this;
    }

    public ActiveRecordServiceRouteConfiguration UseConfigureRoute(
        Action<
            RouteHandlerBuilder,
            ActiveRecordDefinition,
            ActiveRecordMethodDefinition?> configure)
    {
        ConfigureRoute = configure ?? throw new ArgumentNullException(nameof(configure));
        return this;
    }

    public ActiveRecordServiceRouteConfiguration UseConfigureRoute(
        Action<RouteHandlerBuilder, ActiveRecordDefinition> configure)
    {
        configure = configure ?? throw new ArgumentNullException(nameof(configure));
        ConfigureRoute = (route, def, _) => configure(route, def);
        return this;
    }

    public ActiveRecordServiceRouteConfiguration UseConfigureRoute(
        Action<RouteHandlerBuilder> configure)
    {
        configure = configure ?? throw new ArgumentNullException(nameof(configure));
        ConfigureRoute = (route, _, _) => configure(route);
        return this;
    }
}