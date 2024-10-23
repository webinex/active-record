using Webinex.Asky;

namespace Webinex.ActiveRecord;

public class ActiveRecordQuery
{
    public FilterRule? FilterRule { get; }
    public SortRule[]? SortRules { get; }
    public PagingRule? PagingRule { get; }

    public ActiveRecordQuery(FilterRule? filterRule = null, SortRule[]? sortRules = null, PagingRule? pagingRule = null)
    {
        FilterRule = filterRule;
        SortRules = sortRules;
        PagingRule = pagingRule;
    }
}