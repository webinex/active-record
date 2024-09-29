using MediatR;

namespace Webinex.ActiveRecord.Example.Events;

public class EntityDeletedEventHandler : INotificationHandler<EntityDeletedEvent>
{
    private AppDbContext _dbContext;

    public EntityDeletedEventHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task Handle(EntityDeletedEvent notification, CancellationToken cancellationToken)
    {
        _dbContext.Remove(notification.Entity);
        return Task.CompletedTask;
    }
}