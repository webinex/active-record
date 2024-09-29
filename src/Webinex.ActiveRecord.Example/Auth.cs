namespace Webinex.ActiveRecord.Example;

public interface IAuth
{
    Guid Id { get; }
    Guid OrganizationId { get; }
}

public class Auth : IAuth
{
    public Guid Id => Guid.Parse("684a5aab-bfda-433c-b368-bf586808a05b");
    public Guid OrganizationId => Guid.Parse("ff0bab4a-bf6a-434e-9245-e54c2749c0c7");
}