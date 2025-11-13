using BingGroundingApi.SecEdgar.BingSearch;
using BingGroundingApi.SecEdgar.Models;
using System.Text.RegularExpressions;

namespace BingGroundingApi.SecEdgar;

/// <summary>
/// Custom engine agent for indexing and retrieving SEC Edgar documents (10-K, 10-Q, 8-K)
/// using Bing Custom Search API for grounding.
/// </summary>
public class SecEdgarCustomEngineAgent : IDisposable
{
    private readonly BingCustomSearchClient _searchClient;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the SecEdgarCustomEngineAgent.
    /// </summary>
    /// <param name="apiKey">Bing Custom Search API key.</param>
    /// <param name="customConfigId">Custom Configuration ID configured to index SEC Edgar domain.</param>
    public SecEdgarCustomEngineAgent(string apiKey, string customConfigId)
    {
        _searchClient = new BingCustomSearchClient(apiKey, customConfigId);
    }

    /// <summary>
    /// Searches for SEC Edgar documents based on the provided criteria.
    /// </summary>
    /// <param name="query">Search query string.</param>
    /// <param name="documentType">Optional filter by document type.</param>
    /// <param name="ticker">Optional filter by company ticker.</param>
    /// <param name="maxResults">Maximum number of results to return (default: 10).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of SEC documents matching the criteria.</returns>
    public async Task<List<SecDocument>> SearchDocumentsAsync(
        string query,
        SecDocumentType? documentType = null,
        string? ticker = null,
        int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        // Build enhanced query with filters
        var enhancedQuery = BuildEnhancedQuery(query, documentType, ticker);

        // Execute search
        var searchResponse = await _searchClient.SearchAsync(
            enhancedQuery,
            count: Math.Min(maxResults, 50),
            cancellationToken: cancellationToken);

        if (searchResponse?.WebPages?.Value == null)
            return new List<SecDocument>();

        // Parse results into SecDocument objects
        var documents = new List<SecDocument>();
        foreach (var webPage in searchResponse.WebPages.Value)
        {
            try
            {
                var document = ParseWebPageToSecDocument(webPage);
                if (document != null)
                    documents.Add(document);
            }
            catch
            {
                // Skip documents that can't be parsed
                continue;
            }
        }

        return documents;
    }

    /// <summary>
    /// Retrieves documents for a specific company by ticker symbol.
    /// </summary>
    /// <param name="ticker">Company ticker symbol.</param>
    /// <param name="documentType">Optional filter by document type.</param>
    /// <param name="maxResults">Maximum number of results to return (default: 10).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of SEC documents for the specified company.</returns>
    public async Task<List<SecDocument>> GetDocumentsByTickerAsync(
        string ticker,
        SecDocumentType? documentType = null,
        int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ticker))
            throw new ArgumentException("Ticker cannot be null or empty.", nameof(ticker));

        return await SearchDocumentsAsync(
            query: ticker,
            documentType: documentType,
            ticker: ticker,
            maxResults: maxResults,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retrieves the most recent filings of a specific document type.
    /// </summary>
    /// <param name="documentType">The type of document to retrieve.</param>
    /// <param name="maxResults">Maximum number of results to return (default: 10).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of the most recent SEC documents of the specified type.</returns>
    public async Task<List<SecDocument>> GetRecentFilingsAsync(
        SecDocumentType documentType,
        int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        var formType = GetFormTypeString(documentType);
        return await SearchDocumentsAsync(
            query: $"{formType} recent filings",
            documentType: documentType,
            maxResults: maxResults,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Searches for documents filed within a specific date range.
    /// </summary>
    /// <param name="startDate">Start date of the range.</param>
    /// <param name="endDate">End date of the range.</param>
    /// <param name="documentType">Optional filter by document type.</param>
    /// <param name="ticker">Optional filter by company ticker.</param>
    /// <param name="maxResults">Maximum number of results to return (default: 10).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of SEC documents filed within the specified date range.</returns>
    public async Task<List<SecDocument>> SearchByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        SecDocumentType? documentType = null,
        string? ticker = null,
        int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        if (startDate > endDate)
            throw new ArgumentException("Start date must be before or equal to end date.");

        var query = $"filed after:{startDate:yyyy-MM-dd} before:{endDate:yyyy-MM-dd}";
        
        return await SearchDocumentsAsync(
            query: query,
            documentType: documentType,
            ticker: ticker,
            maxResults: maxResults,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Builds an enhanced query string with filters.
    /// </summary>
    private string BuildEnhancedQuery(string baseQuery, SecDocumentType? documentType, string? ticker)
    {
        var queryParts = new List<string> { baseQuery };

        // Add site restriction to SEC Edgar
        queryParts.Add("site:sec.gov/Archives/edgar");

        if (documentType.HasValue)
        {
            var formType = GetFormTypeString(documentType.Value);
            queryParts.Add(formType);
        }

        if (!string.IsNullOrWhiteSpace(ticker))
        {
            queryParts.Add($"\"{ticker}\"");
        }

        return string.Join(" ", queryParts);
    }

    /// <summary>
    /// Parses a web page result into a SecDocument object.
    /// </summary>
    private SecDocument? ParseWebPageToSecDocument(WebPage webPage)
    {
        // Extract document type from URL or content
        var docType = ExtractDocumentType(webPage.Url, webPage.Name, webPage.Snippet);
        if (!docType.HasValue)
            return null;

        var document = new SecDocument
        {
            DocumentType = docType.Value,
            DocumentUrl = webPage.Url,
            CompanyName = ExtractCompanyName(webPage.Name, webPage.Snippet),
            Ticker = ExtractTicker(webPage.Name, webPage.Snippet),
            FilingDate = ExtractFilingDate(webPage.DateLastCrawled, webPage.Snippet),
            AccessionNumber = ExtractAccessionNumber(webPage.Url)
        };

        // Add metadata
        document.Metadata["snippet"] = webPage.Snippet;
        document.Metadata["displayUrl"] = webPage.DisplayUrl;
        document.Metadata["id"] = webPage.Id;

        return document;
    }

    /// <summary>
    /// Extracts the document type from URL or content.
    /// </summary>
    private SecDocumentType? ExtractDocumentType(string url, string name, string snippet)
    {
        var content = $"{url} {name} {snippet}".ToLower();

        if (content.Contains("10-k") || content.Contains("10k"))
            return SecDocumentType.Form10K;
        if (content.Contains("10-q") || content.Contains("10q"))
            return SecDocumentType.Form10Q;
        if (content.Contains("8-k") || content.Contains("8k"))
            return SecDocumentType.Form8K;

        return null;
    }

    /// <summary>
    /// Extracts company name from the page title or snippet.
    /// </summary>
    private string ExtractCompanyName(string name, string snippet)
    {
        // Try to extract from page title (usually in format "Company Name - Form Type")
        if (!string.IsNullOrWhiteSpace(name))
        {
            var parts = name.Split(new[] { " - ", " | ", ": " }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
                return parts[0].Trim();
        }

        return "Unknown Company";
    }

    /// <summary>
    /// Extracts ticker symbol from content.
    /// </summary>
    private string ExtractTicker(string name, string snippet)
    {
        // Look for ticker pattern (usually 1-5 uppercase letters in parentheses)
        var tickerPattern = @"\(([A-Z]{1,5})\)";
        var match = Regex.Match($"{name} {snippet}", tickerPattern);
        
        if (match.Success)
            return match.Groups[1].Value;

        return string.Empty;
    }

    /// <summary>
    /// Extracts filing date from content.
    /// </summary>
    private DateTime ExtractFilingDate(string dateLastCrawled, string snippet)
    {
        // Try to parse the last crawled date
        if (DateTime.TryParse(dateLastCrawled, out var crawledDate))
            return crawledDate;

        // Look for date patterns in snippet
        var datePattern = @"\b(\d{4})-(\d{2})-(\d{2})\b";
        var match = Regex.Match(snippet, datePattern);
        
        if (match.Success && DateTime.TryParse(match.Value, out var extractedDate))
            return extractedDate;

        return DateTime.UtcNow;
    }

    /// <summary>
    /// Extracts accession number from URL.
    /// </summary>
    private string ExtractAccessionNumber(string url)
    {
        // SEC Edgar URLs typically contain accession numbers in format: 0000000000-00-000000
        var accessionPattern = @"(\d{10}-\d{2}-\d{6})";
        var match = Regex.Match(url, accessionPattern);
        
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    /// <summary>
    /// Gets the form type string for a document type.
    /// </summary>
    private string GetFormTypeString(SecDocumentType documentType)
    {
        return documentType switch
        {
            SecDocumentType.Form10K => "10-K",
            SecDocumentType.Form10Q => "10-Q",
            SecDocumentType.Form8K => "8-K",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Disposes resources.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _searchClient?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
