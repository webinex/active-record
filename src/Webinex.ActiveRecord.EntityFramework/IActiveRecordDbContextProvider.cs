using Microsoft.EntityFrameworkCore;

namespace Webinex.ActiveRecord;

public interface IActiveRecordDbContextProvider
{
    DbContext Value { get; }
}