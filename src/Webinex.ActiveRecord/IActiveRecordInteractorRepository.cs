using System.Linq.Expressions;

namespace Webinex.ActiveRecord;

public interface IActiveRecordInteractorRepository<T> : IActiveRecordRepository<T>
{
    IActiveRecordInteractorRepository<T> WithDefaultPredicate(Expression<Func<T, bool>>? predicate);
}