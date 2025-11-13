# Bing Grounding API - SEC Edgar Custom Engine Agent

A C# custom engine agent for indexing and retrieving SEC Edgar documents (10-K, 10-Q, and 8-K filings) using Bing Custom Search API for grounding and information retrieval.

## Overview

This project provides a .NET 9.0 library and example application that demonstrates how to use Bing Custom Search API to ground queries with SEC Edgar documents. The agent supports searching, filtering, and retrieving financial documents from the SEC Edgar system.

## Features

- **Document Type Support**: 10-K, 10-Q, and 8-K SEC filings
- **Flexible Search**: Query by ticker symbol, company name, document type, and date ranges
- **Metadata Extraction**: Automatically extracts company information, filing dates, accession numbers
- **Bing Custom Search Integration**: Leverages Bing's powerful search capabilities with custom configuration
- **Easy to Use**: Clean, well-documented API with comprehensive examples

## Project Structure

```
BingGroundingApi/
├── BingGroundingApi.sln                    # Solution file
├── BingGroundingApi.SecEdgar/              # Main library
│   ├── Models/
│   │   ├── SecDocument.cs                  # SEC document model
│   │   └── SecDocumentType.cs              # Document type enum
│   ├── BingSearch/
│   │   ├── BingCustomSearchClient.cs       # Bing API client
│   │   └── BingCustomSearchResponse.cs     # API response models
│   └── SecEdgarCustomEngineAgent.cs        # Main agent class
└── BingGroundingApi.Example/               # Example console application
    └── Program.cs                          # Usage examples
```

## Prerequisites

- .NET 9.0 SDK or later
- Bing Custom Search API subscription
- Bing Custom Search instance configured to index SEC Edgar domain

## Setup

### 1. Install .NET 9.0 SDK

Download and install from [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)

### 2. Get Bing Custom Search API Credentials

1. Go to [Azure Portal](https://portal.azure.com)
2. Create a "Bing Custom Search" resource
3. Note your API key from the resource's "Keys and Endpoint" section
4. Create a Custom Search instance at [https://www.customsearch.ai/](https://www.customsearch.ai/)
5. Configure the instance to include SEC Edgar domain:
   - Add `sec.gov/Archives/edgar/*` to your search instance
   - Note your Custom Configuration ID

### 3. Set Environment Variables

```bash
# Linux/macOS
export BING_CUSTOM_SEARCH_API_KEY='your-api-key-here'
export BING_CUSTOM_CONFIG_ID='your-custom-config-id-here'

# Windows (PowerShell)
$env:BING_CUSTOM_SEARCH_API_KEY='your-api-key-here'
$env:BING_CUSTOM_CONFIG_ID='your-custom-config-id-here'

# Windows (Command Prompt)
set BING_CUSTOM_SEARCH_API_KEY=your-api-key-here
set BING_CUSTOM_CONFIG_ID=your-custom-config-id-here
```

### 4. Build the Solution

```bash
cd bing_grounding_api
dotnet build
```

## Usage

### Running the Example Application

```bash
cd BingGroundingApi.Example
dotnet run
```

The example application demonstrates:
1. Searching for documents by ticker symbol
2. Retrieving recent filings by document type
3. Combining search filters (ticker + document type)
4. Searching by date range
5. Custom queries for specific document types

### Using the Library in Your Code

```csharp
using BingGroundingApi.SecEdgar;
using BingGroundingApi.SecEdgar.Models;

// Initialize the agent
var apiKey = "your-api-key";
var customConfigId = "your-config-id";

using var agent = new SecEdgarCustomEngineAgent(apiKey, customConfigId);

// Example 1: Get documents by ticker
var documents = await agent.GetDocumentsByTickerAsync("AAPL", maxResults: 10);

// Example 2: Get recent 10-K filings
var tenKFilings = await agent.GetRecentFilingsAsync(
    SecDocumentType.Form10K, 
    maxResults: 10
);

// Example 3: Search with multiple filters
var results = await agent.SearchDocumentsAsync(
    query: "Microsoft",
    documentType: SecDocumentType.Form10Q,
    ticker: "MSFT",
    maxResults: 10
);

// Example 4: Search by date range
var recentDocs = await agent.SearchByDateRangeAsync(
    startDate: DateTime.Now.AddMonths(-3),
    endDate: DateTime.Now,
    documentType: SecDocumentType.Form8K,
    maxResults: 20
);

// Process results
foreach (var doc in results)
{
    Console.WriteLine($"Company: {doc.CompanyName}");
    Console.WriteLine($"Ticker: {doc.Ticker}");
    Console.WriteLine($"Type: {doc.DocumentType}");
    Console.WriteLine($"Filed: {doc.FilingDate}");
    Console.WriteLine($"URL: {doc.DocumentUrl}");
    Console.WriteLine();
}
```

## API Reference

### SecEdgarCustomEngineAgent

Main class for interacting with SEC Edgar documents.

#### Methods

- **`SearchDocumentsAsync`**: General-purpose search with optional filters
  - Parameters: `query`, `documentType`, `ticker`, `maxResults`, `cancellationToken`
  - Returns: `List<SecDocument>`

- **`GetDocumentsByTickerAsync`**: Search for documents by company ticker
  - Parameters: `ticker`, `documentType`, `maxResults`, `cancellationToken`
  - Returns: `List<SecDocument>`

- **`GetRecentFilingsAsync`**: Get recent filings of a specific type
  - Parameters: `documentType`, `maxResults`, `cancellationToken`
  - Returns: `List<SecDocument>`

- **`SearchByDateRangeAsync`**: Search within a date range
  - Parameters: `startDate`, `endDate`, `documentType`, `ticker`, `maxResults`, `cancellationToken`
  - Returns: `List<SecDocument>`

### SecDocument

Represents a SEC Edgar document with the following properties:

- `DocumentType`: Type of document (10-K, 10-Q, 8-K)
- `Ticker`: Company ticker symbol
- `CompanyName`: Company name
- `Cik`: Central Index Key
- `FilingDate`: Document filing date
- `PeriodEndDate`: Reporting period end date
- `AccessionNumber`: SEC accession number
- `DocumentUrl`: URL to the document
- `Metadata`: Additional metadata dictionary

### SecDocumentType

Enum representing supported document types:

- `Form10K`: Annual report
- `Form10Q`: Quarterly report
- `Form8K`: Current report

## Configuration

### Custom Search Instance Setup

For optimal results, configure your Bing Custom Search instance with these settings:

1. **Active URLs**: Add `sec.gov/Archives/edgar/*`
2. **Search Configuration**: 
   - Enable "Web Search"
   - Set language to English
   - Set market to United States (en-US)
3. **Refinements**: Consider adding filters for specific document types or date ranges

## Best Practices

1. **API Key Security**: Never hardcode API keys. Use environment variables or secure configuration management.
2. **Rate Limiting**: Be mindful of API rate limits. Implement retry logic with exponential backoff.
3. **Result Pagination**: For large result sets, use the offset parameter (via custom implementation).
4. **Error Handling**: Always wrap API calls in try-catch blocks.
5. **Resource Disposal**: Use `using` statements or manually dispose of the agent when done.

## Troubleshooting

### "API key cannot be null or empty"
- Ensure environment variables are set correctly
- Verify the variable names match exactly

### "Failed to execute search query"
- Check your API key is valid and active
- Verify your Custom Configuration ID is correct
- Ensure your Azure subscription is active
- Check internet connectivity

### No results returned
- Verify your Custom Search instance includes SEC Edgar domain
- Try broader search queries
- Check if the search instance is properly indexed

### HTTP 401 Unauthorized
- API key is invalid or expired
- Check your Azure subscription status

### HTTP 403 Forbidden
- Custom Configuration ID is incorrect
- Search instance not properly configured

## Dependencies

- .NET 9.0
- System.Net.Http.Json
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Configuration.Json

## License

This project is provided as-is for educational and development purposes.

## Contributing

Contributions are welcome! Please ensure:
- Code follows existing style and conventions
- All builds pass without errors
- XML documentation is included for public APIs
- Examples are updated if API changes

## Support

For issues related to:
- **Bing Custom Search API**: [Azure Support](https://azure.microsoft.com/support/)
- **SEC Edgar**: [SEC Support](https://www.sec.gov/edgar/searchedgar/companysearch.html)
- **This Library**: Open an issue on GitHub

## Additional Resources

- [SEC Edgar Documentation](https://www.sec.gov/edgar/searchedgar/accessing-edgar-data.htm)
- [Bing Custom Search API Documentation](https://docs.microsoft.com/azure/cognitive-services/bing-custom-search/)
- [SEC Filing Types](https://www.sec.gov/forms)
- [Understanding 10-K, 10-Q, and 8-K Forms](https://www.investor.gov/introduction-investing/investing-basics/glossary/form-10-k)

## Changelog

### Version 1.0.0 (Initial Release)
- SEC Edgar document models (10-K, 10-Q, 8-K)
- Bing Custom Search API integration
- Search by ticker, document type, date range
- Comprehensive example application
- Full XML documentation
