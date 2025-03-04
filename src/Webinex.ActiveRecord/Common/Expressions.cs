using System.Linq.Expressions;

namespace Webinex.ActiveRecord.Common;

internal static class Expressions
{
    public static Expression<Func<TNew, bool>> ReplaceParameter<TNew>(LambdaExpression expression)
    {
        var previousParam = expression.Parameters[0];
        var newParam = Expression.Parameter(typeof(TNew), previousParam.Name);
        var newBody = new ReplaceParameterVisitor(previousParam, newParam).Visit(expression.Body);
        return Expression.Lambda<Func<TNew, bool>>(newBody, [newParam]);
    }
    
    public class ReplaceParameterVisitor(ParameterExpression previous, ParameterExpression @new) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == previous ? @new : base.VisitParameter(node);
        }
    }
}