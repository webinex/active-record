using System.Linq.Expressions;
using Webinex.Asky;

namespace Webinex.ActiveRecord.Example.Types;

public class ClientFieldMap : IAskyFieldMap<Client>
{
    public Expression<Func<Client, object>>? this[string fieldId] => fieldId switch
    {
        "id" => x => x.Id,
        "firstName" => x => x.FirstName,
        "lastName" => x => x.LastName,
        "active" => x => x.Active,
        _ => null,
    };
}