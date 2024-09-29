using System.Reflection;

namespace Webinex.ActiveRecord;

internal class Result
{
    /// <summary>
    ///     Check if a method returns void or non-generic Task
    /// </summary>
    /// <param name="methodInfo"><see cref="MethodInfo"/></param>
    /// <returns>True if method returns void or non-generic Task. False otherwise.</returns>
    public static bool IsVoidOrTask(MethodInfo methodInfo)
    {
        return methodInfo.ReturnType == typeof(void) || methodInfo.ReturnType == typeof(Task);
    }

    /// <summary>
    ///     Unwrap the result of a method invocation if it is a Task or Task&lt;T&gt;
    /// </summary>
    /// <param name="value">Result or task containing result</param>
    /// <returns><paramref name="value"/> or <see cref="Task{TResult}.Result"/></returns>
    public static async Task<object?> UnwrapAsync(object? value)
    {
        if (value is Task task)
        {
            await task;

            if (task.GetType().IsGenericType)
            {
                value = task.GetType().GetProperty(nameof(Task<object>.Result))?.GetValue(task);
            }
        }

        return value;
    }
}