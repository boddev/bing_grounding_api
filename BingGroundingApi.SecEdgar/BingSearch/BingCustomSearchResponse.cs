using System.Text.Json.Serialization;

namespace BingGroundingApi.SecEdgar.BingSearch;

/// <summary>
/// Represents the response from Bing Custom Search API.
/// </summary>
public class BingCustomSearchResponse
{
    [JsonPropertyName("_type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("webPages")]
    public WebPages? WebPages { get; set; }

    [JsonPropertyName("queryContext")]
    public QueryContext? QueryContext { get; set; }
}

/// <summary>
/// Represents web pages in the search response.
/// </summary>
public class WebPages
{
    [JsonPropertyName("webSearchUrl")]
    public string WebSearchUrl { get; set; } = string.Empty;

    [JsonPropertyName("totalEstimatedMatches")]
    public long TotalEstimatedMatches { get; set; }

    [JsonPropertyName("value")]
    public List<WebPage> Value { get; set; } = new();
}

/// <summary>
/// Represents a single web page result.
/// </summary>
public class WebPage
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("snippet")]
    public string Snippet { get; set; } = string.Empty;

    [JsonPropertyName("dateLastCrawled")]
    public string DateLastCrawled { get; set; } = string.Empty;

    [JsonPropertyName("displayUrl")]
    public string DisplayUrl { get; set; } = string.Empty;
}

/// <summary>
/// Represents the query context in the search response.
/// </summary>
public class QueryContext
{
    [JsonPropertyName("originalQuery")]
    public string OriginalQuery { get; set; } = string.Empty;
}
