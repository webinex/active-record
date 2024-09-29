using System.ComponentModel.DataAnnotations;

namespace Webinex.ActiveRecord.Example.Types;

public class Contact
{
    [Key]
    public Guid Id { get; }
    
    [Required]
    public string Phone { get; protected init; } = null!;
    
    [Required]
    public string Email { get; protected init; } = null!;

    protected Contact()
    {
    }

    public Contact(string phone, string email)
    {
        Phone = phone;
        Email = email;
    }
}