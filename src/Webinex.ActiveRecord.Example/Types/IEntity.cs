using MediatR;

namespace Webinex.ActiveRecord.Example.Types;

public interface IEntity
{
    ICollection<INotification> Events { get; }
}