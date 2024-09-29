using MediatR;

namespace Webinex.ActiveRecord.Example.Events;

public record EntityDeletedEvent(object Entity) : INotification;