using System.Text.Json.Serialization;
using Webinex.ActiveRecord;
using Webinex.ActiveRecord.Annotations;
using Webinex.ActiveRecord.AspNetCore;
using Webinex.ActiveRecord.Example;
using Webinex.ActiveRecord.Example.Types;
using Webinex.ActiveRecord.HotChocolate;
using Webinex.Asky;
using BindingBehavior = Webinex.ActiveRecord.BindingBehavior;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddMediatR(x => x.RegisterServicesFromAssembly(typeof(Program).Assembly))
    .AddSingleton<IClock, Clock>()
    .AddSingleton<IAuth, Auth>()
    .AddDbContext<AppDbContext>()
    .AddSingleton<IAskyFieldMap<Client>, ClientFieldMap>();

builder.Services
    .AddActiveRecordService(o => o
        .UseBinding(BindingBehavior.Implicit)
        .UseDbContext<AppDbContext>()
        .ConfigureTypeAnalyzer(settings => settings.IgnoreProperty(x => x.Name == nameof(IEntity.Events)))
        .Add<Client>());

builder.Services
    .AddGraphQLServer()
    .AddQueryType()
    .AddProjections()
    .AddActiveRecordTypes()
    .AddDiagnosticEventListener<GraphQLExceptionLogger.Execution>()
    .AddDiagnosticEventListener<GraphQLExceptionLogger.Server>()
    .AddDiagnosticEventListener<GraphQLExceptionLogger.DataLoader>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var dbContext = sp.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureDeleted();
    dbContext.Database.EnsureCreated();
    
    dbContext.Set<Client>().AddRange([
        Client.NewDefault("James", "Doe"),
        Client.NewDefault("Diego", "Alves"),
        Client.NewDefault("Jane", "Doe"),
    ]);

    dbContext.SaveChanges();
}

app
    .UseSwagger()
    .UseSwaggerUI();

app
    .MapActiveRecords(x => x
        .UseRoute("/api")
        .UseConfigureRoute(routes => routes
            .AddEndpointFilter<DbContextSaveChangesFilter>()));

app.MapGraphQL();

app.Run();

Action<Client> f = x => x.Update(From.Service<IAuth>(), From.Body<Client.UpdateInput>());
Action<Client> f2 = x => Client.New(From.Service<IAuth>(), From.Body<Client.NewInput>());