using MediatR;
using Microsoft.EntityFrameworkCore;
using Webinex.ActiveRecord.Example.Types;

namespace Webinex.ActiveRecord.Example;

public class AppDbContext : DbContext
{
    private readonly IMediator _mediator;
    
    public AppDbContext(IMediator mediator) : base(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(
                "Data Source=localhost;Initial Catalog=wx-active-record;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options)
    {
        _mediator = mediator;
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        NotifyEventHandlersAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new())
    {
        await NotifyEventHandlersAsync();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Client>(client =>
        {
            client.ToTable("Clients");
            client.HasKey(x => x.Id);
            client.OwnsMany(x => x.Contacts, contacts =>
            {
                contacts.ToTable("ClientContacts");
            });
        });
    }

    private async Task NotifyEventHandlersAsync()
    {
        int iteration = 0;

        while (true)
        {
            var entities = GetEventSourceEntities();
        
            if (entities.Length == 0)
                return;
            
            if (iteration > 10)
                throw new InvalidOperationException("Too many iterations");

            await PublishEventsAsync(entities);
            iteration++;
        }
    }

    private async Task PublishEventsAsync(IEntity[] entities)
    {
        foreach (var entity in entities)
        {
            var events = entity.Events.ToArray();
            entity.Events.Clear();
            
            foreach (var @event in events)
            {
                await _mediator.Publish(@event);
            }
        }
    }

    private IEntity[] GetEventSourceEntities()
    {
        ChangeTracker.DetectChanges();
        return ChangeTracker
            .Entries()
            .Where(x => x.Entity is IEntity)
            .Select(x => (IEntity)x.Entity)
            .Where(x => x.Events.Count != 0).ToArray();
    }
}