using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Webinex.Asky;

namespace Webinex.ActiveRecord.AspNetCore;

internal class ActiveRecordQueryDeserializer<T>
{
    private readonly IAskyFieldMap<T>? _fieldMap;
    private readonly IOptions<JsonOptions> _jsonOptions;

    public ActiveRecordQueryDeserializer(IAskyFieldMap<T>? fieldMap, IOptions<JsonOptions> jsonOptions)
    {
        _fieldMap = fieldMap;
        _jsonOptions = jsonOptions;
    }

    public Task<ActiveRecordQuery?> DeserializeAsync(string queryString)
    {
        var jObject = JsonNode.Parse(queryString)!.AsObject();
        var filterRuleJNode = GetJNode(jObject, "filterRule");
        var sortRulesJNode = GetJNode(jObject, "sortRules");
        var pagingRuleJNode = GetJNode(jObject, "pagingRule");

        var filterRule = filterRuleJNode is not null ? DeserializeFilterRule(filterRuleJNode) : null;
        var sortRule = sortRulesJNode is not null ? DeserializeSortRule(sortRulesJNode) : null;
        var pagingRule = pagingRuleJNode is not null ? DeserializePagingRule(pagingRuleJNode) : null;

        if (filterRule is null && (sortRule is null || sortRule.Length == 0) && pagingRule is null)
            return Task.FromResult<ActiveRecordQuery?>(null);
        
        var query = new ActiveRecordQuery(filterRule, sortRule, pagingRule);
        return Task.FromResult<ActiveRecordQuery?>(query);
    }

    private PagingRule DeserializePagingRule(JsonNode pagingRuleJNode)
    {
        var jsonString = pagingRuleJNode.ToJsonString();
        return PagingRule.FromJson(jsonString, _jsonOptions.Value.JsonSerializerOptions)!;
    }

    private SortRule[] DeserializeSortRule(JsonNode sortRuleJNode)
    {
        var jsonString = sortRuleJNode.ToJsonString();
        return SortRule.FromJsonArray(jsonString, _jsonOptions.Value.JsonSerializerOptions)!;
    }

    private FilterRule DeserializeFilterRule(JsonNode filterRuleJNode)
    {
        var jsonString = filterRuleJNode.ToJsonString();
        return _fieldMap is not null
            ? FilterRule.FromJson(jsonString, _fieldMap)!
            : FilterRule.FromJson(jsonString, _jsonOptions.Value.JsonSerializerOptions)!;
    }

    private JsonNode? GetJNode(JsonObject jObject, string key)
    {
        return jObject.FirstOrDefault(x => x.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)).Value;
    }
}