namespace Webinex.ActiveRecord.Example;

public class DbContextSaveChangesFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = await next(context);

        if (!IsMutateMethod(context.HttpContext.Request.Method))
        {
            return result;
        }
        
        
        var dbContextProvider = context.HttpContext.RequestServices.GetRequiredService<IActiveRecordDbContextProvider>();
        await dbContextProvider.Value.SaveChangesAsync();
        return result;
    }

    private bool IsMutateMethod(string httpMethod)
    {
        return httpMethod.Equals(HttpMethods.Post, StringComparison.InvariantCultureIgnoreCase)
            || httpMethod.Equals(HttpMethods.Put, StringComparison.InvariantCultureIgnoreCase)
            || httpMethod.Equals(HttpMethods.Patch, StringComparison.InvariantCultureIgnoreCase)
            || httpMethod.Equals(HttpMethods.Delete, StringComparison.InvariantCultureIgnoreCase);
    }
}