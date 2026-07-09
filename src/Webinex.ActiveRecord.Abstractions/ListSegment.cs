namespace Webinex.ActiveRecord;

public class ListSegment<T>
{
    public IReadOnlyCollection<T> Items { get; protected set; }
    public int Total { get; protected set; }

    /// <summary>
    ///     Creates a new list segment.
    /// </summary>
    /// <param name="items">The items in the current segment.</param>
    /// <param name="total">
    ///     Total number of items matching the search criteria.
    ///     A value of <c>-1</c> indicates that the total count is unspecified.
    /// </param>
    public ListSegment(IEnumerable<T> items, int total)
    {
        Items = items?.ToArray() ?? throw new ArgumentNullException(nameof(items));
        Total = total < -1
            ? throw new ArgumentOutOfRangeException(nameof(total), "Might be >= 0 or -1")
            : total;
    }
}