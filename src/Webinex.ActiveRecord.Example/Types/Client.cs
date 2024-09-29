using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MediatR;
using Webinex.ActiveRecord.Annotations;
using Webinex.ActiveRecord.Example.Events;
using Webinex.Coded;

namespace Webinex.ActiveRecord.Example.Types;

[ActiveRecord]
public class Client : IAggregateRoot
{
    private readonly List<Contact> _contacts = new();

    [Key] public Guid Id { get; protected init; }
    public Guid CreatedById { get; protected init; }
    public Guid LastModifiedById { get; protected set; }
    public string FirstName { get; protected set; } = null!;
    public string LastName { get; protected set; } = null!;
    public bool Active { get; protected set; }
    public IReadOnlyCollection<Contact> Contacts => _contacts.AsReadOnly();

    [JsonIgnore] public ICollection<INotification> Events { get; } = new List<INotification>();

    protected Client()
    {
    }

    [Action]
    public static Client New(IAuth auth, [Body] NewInput input)
    {
        return new Client
        {
            Id = Guid.NewGuid(),
            CreatedById = auth.Id,
            LastModifiedById = auth.Id,
            FirstName = input.FirstName,
            LastName = input.LastName,
            Active = true,
        };
    }

    [Action]
    public void Update(IAuth auth, [Body] UpdateInput input)
    {
        LastModifiedById = auth.Id;
        FirstName = input.FirstName;
        LastName = input.LastName;
    }

    [Action]
    public void SetActive(IAuth auth, [Body] bool value)
    {
        LastModifiedById = auth.Id;
        Active = value;
    }

    [Action]
    public void Delete()
    {
        Events.Add(new EntityDeletedEvent(this));
    }

    [Action(ActionType.Update)]
    public void UpdateContacts(IAuth auth, [Body] IEnumerable<Contact> contacts)
    {
        if (!Active)
            throw CodedException.Invalid();

        LastModifiedById = auth.Id;
        _contacts.Clear();
        _contacts.AddRange(contacts);
    }

    public record UpdateInput(string FirstName, string LastName);

    public record NewInput(string FirstName, string LastName);
}