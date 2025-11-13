# Usage Guide - SEC Edgar Custom Engine Agent

This guide provides detailed instructions and examples for using the SEC Edgar Custom Engine Agent.

## Table of Contents

1. [Quick Start](#quick-start)
2. [Configuration](#configuration)
3. [Basic Examples](#basic-examples)
4. [Advanced Usage](#advanced-usage)
5. [API Details](#api-details)
6. [Error Handling](#error-handling)

## Quick Start

### 1. Set Up Environment Variables

```bash
export BING_CUSTOM_SEARCH_API_KEY='your-api-key'
export BING_CUSTOM_CONFIG_ID='your-config-id'
```

### 2. Run the Example Application

```bash
cd BingGroundingApi.Example
dotnet run
```

### 3. Basic Code Example

```csharp
using BingGroundingApi.SecEdgar;
using BingGroundingApi.SecEdgar.Models;

var apiKey = Environment.GetEnvironmentVariable("BING_CUSTOM_SEARCH_API_KEY");
var configId = Environment.GetEnvironmentVariable("BING_CUSTOM_CONFIG_ID");

using var agent = new SecEdgarCustomEngineAgent(apiKey, configId);

// Search for Apple's 10-K filings
var results = await agent.GetDocumentsByTickerAsync("AAPL", maxResults: 5);

foreach (var doc in results)
{
    Console.WriteLine($"{doc.CompanyName} - {doc.DocumentType}");
    Console.WriteLine($"Filed: {doc.FilingDate:yyyy-MM-dd}");
    Console.WriteLine($"URL: {doc.DocumentUrl}\n");
}
```

## Configuration

### Environment Variables (Recommended)

The most secure way to manage credentials:

```bash
# Linux/macOS
export BING_CUSTOM_SEARCH_API_KEY='abc123...'
export BING_CUSTOM_CONFIG_ID='12345678'

# Windows PowerShell
$env:BING_CUSTOM_SEARCH_API_KEY='abc123...'
$env:BING_CUSTOM_CONFIG_ID='12345678'
```

### Configuration File (Alternative)

Copy `appsettings.example.json` to `appsettings.json` and fill in your credentials:

```json
{
  "BingCustomSearch": {
    "ApiKey": "your-actual-api-key",
    "CustomConfigId": "your-actual-config-id"
  }
}
```

**Important**: `appsettings.json` is in `.gitignore` to prevent committing secrets.

## Basic Examples

### Example 1: Search by Company Ticker

Find all SEC documents for a specific company:

```csharp
var documents = await agent.GetDocumentsByTickerAsync(
    ticker: "MSFT",
    maxResults: 10
);
```

### Example 2: Search by Document Type

Get recent 10-K annual reports:

```csharp
var annualReports = await agent.GetRecentFilingsAsync(
    documentType: SecDocumentType.Form10K,
    maxResults: 20
);
```

### Example 3: Combine Filters

Search for Microsoft's 10-Q quarterly reports:

```csharp
var quarterlyReports = await agent.SearchDocumentsAsync(
    query: "Microsoft",
    documentType: SecDocumentType.Form10Q,
    ticker: "MSFT",
    maxResults: 10
);
```

### Example 4: Search by Date Range

Find documents filed in the last 90 days:

```csharp
var recentDocs = await agent.SearchByDateRangeAsync(
    startDate: DateTime.Now.AddDays(-90),
    endDate: DateTime.Now,
    maxResults: 15
);
```

### Example 5: Search Current Reports (8-K)

Find recent 8-K current reports for any company:

```csharp
var currentReports = await agent.SearchDocumentsAsync(
    query: "current report",
    documentType: SecDocumentType.Form8K,
    maxResults: 10
);
```

## Advanced Usage

### Custom Query Construction

You can build complex queries for specific needs:

```csharp
// Search for Tesla's 8-K filings related to earnings
var results = await agent.SearchDocumentsAsync(
    query: "Tesla earnings",
    documentType: SecDocumentType.Form8K,
    ticker: "TSLA",
    maxResults: 10
);

// Search for technology companies' 10-K filings
var techReports = await agent.SearchDocumentsAsync(
    query: "technology software",
    documentType: SecDocumentType.Form10K,
    maxResults: 20
);
```

### Processing Document Metadata

Access rich metadata from search results:

```csharp
var documents = await agent.GetDocumentsByTickerAsync("AAPL", maxResults: 5);

foreach (var doc in documents)
{
    Console.WriteLine($"Company: {doc.CompanyName}");
    Console.WriteLine($"Ticker: {doc.Ticker}");
    Console.WriteLine($"CIK: {doc.Cik}");
    Console.WriteLine($"Type: {doc.DocumentType}");
    Console.WriteLine($"Filed: {doc.FilingDate:yyyy-MM-dd}");
    Console.WriteLine($"Period End: {doc.PeriodEndDate?.ToString("yyyy-MM-dd") ?? "N/A"}");
    Console.WriteLine($"Accession: {doc.AccessionNumber}");
    Console.WriteLine($"URL: {doc.DocumentUrl}");
    
    // Access additional metadata
    if (doc.Metadata.ContainsKey("snippet"))
    {
        Console.WriteLine($"Snippet: {doc.Metadata["snippet"]}");
    }
    
    Console.WriteLine();
}
```

### Filtering and Post-Processing

Apply additional filtering after retrieval:

```csharp
var allDocs = await agent.GetDocumentsByTickerAsync("GOOGL", maxResults: 50);

// Filter for documents from 2023
var docs2023 = allDocs
    .Where(d => d.FilingDate.Year == 2023)
    .OrderByDescending(d => d.FilingDate)
    .ToList();

// Group by document type
var groupedByType = allDocs
    .GroupBy(d => d.DocumentType)
    .ToDictionary(g => g.Key, g => g.ToList());

Console.WriteLine($"10-K Reports: {groupedByType[SecDocumentType.Form10K].Count}");
Console.WriteLine($"10-Q Reports: {groupedByType[SecDocumentType.Form10Q].Count}");
Console.WriteLine($"8-K Reports: {groupedByType[SecDocumentType.Form8K].Count}");
```

### Using Cancellation Tokens

Support for async cancellation:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    var results = await agent.SearchDocumentsAsync(
        query: "financial",
        maxResults: 50,
        cancellationToken: cts.Token
    );
}
catch (OperationCanceledException)
{
    Console.WriteLine("Search operation was cancelled or timed out.");
}
```

### Multiple Search Operations

Perform multiple searches efficiently:

```csharp
using var agent = new SecEdgarCustomEngineAgent(apiKey, configId);

// Search multiple companies
var tickers = new[] { "AAPL", "MSFT", "GOOGL", "AMZN", "META" };
var allResults = new Dictionary<string, List<SecDocument>>();

foreach (var ticker in tickers)
{
    var docs = await agent.GetDocumentsByTickerAsync(
        ticker, 
        documentType: SecDocumentType.Form10K,
        maxResults: 5
    );
    allResults[ticker] = docs;
}

// Display results
foreach (var (ticker, docs) in allResults)
{
    Console.WriteLine($"\n{ticker}: {docs.Count} documents found");
    foreach (var doc in docs.Take(3))
    {
        Console.WriteLine($"  - {doc.FilingDate:yyyy-MM-dd}: {doc.DocumentUrl}");
    }
}
```

## API Details

### SecEdgarCustomEngineAgent Methods

#### SearchDocumentsAsync

General-purpose search with full filtering capabilities.

```csharp
Task<List<SecDocument>> SearchDocumentsAsync(
    string query,                    // Required: search query
    SecDocumentType? documentType,   // Optional: filter by type
    string? ticker,                  // Optional: filter by ticker
    int maxResults = 10,             // Optional: max results (1-50)
    CancellationToken cancellationToken = default
)
```

#### GetDocumentsByTickerAsync

Convenience method for ticker-based searches.

```csharp
Task<List<SecDocument>> GetDocumentsByTickerAsync(
    string ticker,                   // Required: company ticker
    SecDocumentType? documentType,   // Optional: filter by type
    int maxResults = 10,             // Optional: max results
    CancellationToken cancellationToken = default
)
```

#### GetRecentFilingsAsync

Retrieve recent filings of a specific type.

```csharp
Task<List<SecDocument>> GetRecentFilingsAsync(
    SecDocumentType documentType,    // Required: document type
    int maxResults = 10,             // Optional: max results
    CancellationToken cancellationToken = default
)
```

#### SearchByDateRangeAsync

Search within a specific date range.

```csharp
Task<List<SecDocument>> SearchByDateRangeAsync(
    DateTime startDate,              // Required: range start
    DateTime endDate,                // Required: range end
    SecDocumentType? documentType,   // Optional: filter by type
    string? ticker,                  // Optional: filter by ticker
    int maxResults = 10,             // Optional: max results
    CancellationToken cancellationToken = default
)
```

### SecDocument Properties

| Property | Type | Description |
|----------|------|-------------|
| `DocumentType` | `SecDocumentType` | Type of document (10-K, 10-Q, 8-K) |
| `Ticker` | `string` | Company ticker symbol |
| `CompanyName` | `string` | Company name |
| `Cik` | `string` | SEC Central Index Key |
| `FilingDate` | `DateTime` | Document filing date |
| `PeriodEndDate` | `DateTime?` | Reporting period end date |
| `AccessionNumber` | `string` | SEC accession number |
| `DocumentUrl` | `string` | URL to the document |
| `Metadata` | `Dictionary<string,string>` | Additional metadata |

### SecDocumentType Enum

```csharp
public enum SecDocumentType
{
    Form10K,  // Annual report
    Form10Q,  // Quarterly report
    Form8K    // Current report
}
```

## Error Handling

### Common Exceptions

```csharp
try
{
    var results = await agent.SearchDocumentsAsync("query", maxResults: 100);
}
catch (ArgumentException ex)
{
    // Invalid arguments (e.g., null query, invalid max results)
    Console.WriteLine($"Invalid argument: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    // API call failed (network, auth, etc.)
    Console.WriteLine($"API error: {ex.Message}");
}
catch (Exception ex)
{
    // Unexpected error
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

### Best Practices for Error Handling

```csharp
public async Task<List<SecDocument>> SafeSearchAsync(
    SecEdgarCustomEngineAgent agent,
    string query,
    int maxRetries = 3)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            return await agent.SearchDocumentsAsync(query, maxResults: 10);
        }
        catch (InvalidOperationException ex) when (attempt < maxRetries)
        {
            Console.WriteLine($"Attempt {attempt} failed: {ex.Message}");
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt))); // Exponential backoff
        }
    }
    
    throw new Exception($"Failed after {maxRetries} attempts");
}
```

### Validation

Always validate inputs before calling API:

```csharp
// Validate ticker format
if (!Regex.IsMatch(ticker, @"^[A-Z]{1,5}$"))
{
    throw new ArgumentException("Invalid ticker format");
}

// Validate date range
if (startDate > endDate)
{
    throw new ArgumentException("Start date must be before end date");
}

// Validate max results
if (maxResults < 1 || maxResults > 50)
{
    throw new ArgumentOutOfRangeException(nameof(maxResults), 
        "Must be between 1 and 50");
}
```

## Performance Tips

1. **Limit Results**: Only request what you need (default: 10, max: 50)
2. **Use Specific Queries**: More specific queries return faster results
3. **Reuse Agent Instance**: Create once, use multiple times within the same scope
4. **Implement Caching**: Cache frequently accessed documents
5. **Parallel Searches**: Use `Task.WhenAll` for multiple independent searches

```csharp
// Parallel searches example
var tasks = new[]
{
    agent.GetDocumentsByTickerAsync("AAPL", maxResults: 5),
    agent.GetDocumentsByTickerAsync("MSFT", maxResults: 5),
    agent.GetDocumentsByTickerAsync("GOOGL", maxResults: 5)
};

var results = await Task.WhenAll(tasks);
```

## Additional Resources

- [SEC Edgar Search](https://www.sec.gov/edgar/searchedgar/companysearch.html)
- [Understanding SEC Filings](https://www.investor.gov/introduction-investing/investing-basics/glossary)
- [Bing Custom Search Documentation](https://docs.microsoft.com/azure/cognitive-services/bing-custom-search/)
