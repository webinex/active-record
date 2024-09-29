namespace Webinex.ActiveRecord;

public interface IActionAuthorizationConvention<TType>
{
    void Configure(AuthorizationSettingsBuilder<TType> builder);
}