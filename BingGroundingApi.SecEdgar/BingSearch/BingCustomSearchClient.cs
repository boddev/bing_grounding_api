using System.Net.Http.Json;
using System.Text.Json;

namespace BingGroundingApi.SecEdgar.BingSearch;

/// <summary>
/// Client for interacting with Bing Custom Search API.
/// </summary>
public class BingCustomSearchClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _customConfigId;
    private const string BaseUrl = "https://api.bing.microsoft.com/v7.0/custom/search";
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the BingCustomSearchClient.
    /// </summary>
    /// <param name="apiKey">Bing Custom Search API key.</param>
    /// <param name="customConfigId">Custom Configuration ID for the search instance.</param>
    /// <param name="httpClient">Optional HttpClient instance. If not provided, a new one will be created.</param>
    public BingCustomSearchClient(string apiKey, string customConfigId, HttpClient? httpClient = null)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be null or empty.", nameof(apiKey));
        
        if (string.IsNullOrWhiteSpace(customConfigId))
            throw new ArgumentException("Custom config ID cannot be null or empty.", nameof(customConfigId));

        _apiKey = apiKey;
        _customConfigId = customConfigId;
        _httpClient = httpClient ?? new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);
    }

    /// <summary>
    /// Performs a custom search query.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <param name="count">Number of results to return (default: 10, max: 50).</param>
    /// <param name="offset">Offset for pagination (default: 0).</param>
    /// <param name="market">Market code (e.g., "en-US"). Default is "en-US".</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The search response.</returns>
    public async Task<BingCustomSearchResponse?> SearchAsync(
        string query,
        int count = 10,
        int offset = 0,
        string market = "en-US",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be null or empty.", nameof(query));

        if (count < 1 || count > 50)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be between 1 and 50.");

        var queryParams = new Dictionary<string, string>
        {
            ["q"] = query,
            ["customconfig"] = _customConfigId,
            ["count"] = count.ToString(),
            ["offset"] = offset.ToString(),
            ["mkt"] = market
        };

        var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        var requestUrl = $"{BaseUrl}?{queryString}";

        try
        {
            var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<BingCustomSearchResponse>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken);

            return result;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to execute search query: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Disposes the HTTP client if it was created internally.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
