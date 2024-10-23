using HotChocolate.AspNetCore.Instrumentation;
using HotChocolate.Execution;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Resolvers;

namespace Webinex.ActiveRecord.Example;

public abstract class GraphQLExceptionLogger
{
    public class Server : ServerDiagnosticEventListener
    {
        private readonly ILogger<GraphQLExceptionLogger> _logger;

        public Server(ILogger<GraphQLExceptionLogger> logger)
        {
            _logger = logger;
        }

        public override void HttpRequestError(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "GraphQL HTTP request error");
            base.HttpRequestError(context, exception);
        }
    }

    public class DataLoader : DataLoaderDiagnosticEventListener
    {
        private readonly ILogger<GraphQLExceptionLogger> _logger;

        public DataLoader(ILogger<GraphQLExceptionLogger> logger)
        {
            _logger = logger;
        }

        public override void BatchError<TKey>(IReadOnlyList<TKey> keys, Exception error)
        {
            _logger.LogError(error, "GraphQL data loader batch error");
            base.BatchError(keys, error);
        }
    }
    
    public class Execution : ExecutionDiagnosticEventListener
    {
        private readonly ILogger<GraphQLExceptionLogger> _logger;

        public Execution(ILogger<GraphQLExceptionLogger> logger)
        {
            _logger = logger;
        }

        public override void RequestError(IRequestContext context, Exception exception)
        {
            _logger.LogError(exception, "GraphQL request error");
        }

        public override void ResolverError(IMiddlewareContext context, IError error)
        {
            _logger.LogError(error.Exception, $"GraphQL resolver error: {error.Code}\n\t{error.Path}\n\t{error.Message}");
            base.ResolverError(context, error);
        }

        public override void SyntaxError(IRequestContext context, IError error)
        {
            _logger.LogError(error.Exception, $"GraphQL syntax error: {error.Code}\n\t{error.Path}\n\t{error.Message}");
            base.SyntaxError(context, error);
        }

        public override void TaskError(IExecutionTask task, IError error)
        {
            _logger.LogError(error.Exception, $"GraphQL task error: {error.Code}\n\t{error.Path}\n\t{error.Message}");
            base.TaskError(task, error);
        }
    }
}