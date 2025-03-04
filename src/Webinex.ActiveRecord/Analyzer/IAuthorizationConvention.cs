namespace Webinex.ActiveRecord;

public interface IAuthorizationConvention
{
    AuthorizationSettings Create(Type type);
}

public interface IAuthorizationConvention<in T> : IAuthorizationConvention;