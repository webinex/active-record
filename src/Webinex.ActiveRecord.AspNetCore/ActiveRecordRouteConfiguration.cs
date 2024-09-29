using Microsoft.AspNetCore.Builder;

namespace Webinex.ActiveRecord.AspNetCore;

public class ActiveRecordRouteConfiguration
{
    private string? _route;
    private Action<RouteHandlerBuilder, ActiveRecordDefinition, ActiveRecordMethodDefinition?>? _configureRouteDelegate;

    public string Route => _route ?? Base.Route + "/" + Strings.PascalCaseToKebabCase(Settings.Definition.Name);

    public ActiveRecordRouteConfiguration(ActiveRecordServiceRouteConfiguration @base, IActiveRecordSettings settings)
    {
        Base = @base;
        Settings = settings;
    }

    public ActiveRecordServiceRouteConfiguration Base { get; }
    public IActiveRecordSettings Settings { get; }
    public ActiveRecordDefinition Definition => Settings.Definition;
    public Type Type => Settings.Type;

    public ActiveRecordRouteConfiguration UseRoute(string route)
    {
        route = route ?? throw new ArgumentNullException(nameof(route));

        if (!route.StartsWith("/"))
            throw new ArgumentException($"Might start with \"/\". Value = {route}", nameof(route));

        _route = route.TrimEnd('/');
        return this;
    }

    public ActiveRecordRouteConfiguration UseConfigureRoute(
        Action<RouteHandlerBuilder, ActiveRecordDefinition, ActiveRecordMethodDefinition?> configure)
    {
        _configureRouteDelegate = configure ?? throw new ArgumentNullException(nameof(configure));
        return this;
    }

    public ActiveRecordRouteConfiguration UseConfigureRoute(
        Action<RouteHandlerBuilder, ActiveRecordDefinition> configure)
    {
        configure = configure ?? throw new ArgumentNullException(nameof(configure));
        _configureRouteDelegate = (route, def, _) => configure(route, def);
        return this;
    }

    public ActiveRecordRouteConfiguration UseConfigureRoute(
        Action<RouteHandlerBuilder> configure)
    {
        configure = configure ?? throw new ArgumentNullException(nameof(configure));
        _configureRouteDelegate = (route, _, _) => configure(route);
        return this;
    }

    internal RouteHandlerBuilder ConfigureRoute(
        RouteHandlerBuilder route,
        ActiveRecordDefinition definition,
        ActiveRecordMethodDefinition? method)
    {
        var configure = _configureRouteDelegate ?? Base.ConfigureRoute;
        configure?.Invoke(route, definition, method);
        return route;
    }
}