# Quick Start Guide

Get up and running with the SEC Edgar Custom Engine Agent in 5 minutes.

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download) installed
- Bing Custom Search API credentials (see below)

## Step 1: Get Bing Custom Search Credentials

1. **Create Azure Account**: Go to [portal.azure.com](https://portal.azure.com)
2. **Create Bing Custom Search Resource**:
   - Search for "Bing Custom Search" in the marketplace
   - Click "Create"
   - Fill in required details
   - Note your API Key from "Keys and Endpoint"
3. **Configure Custom Search Instance**:
   - Go to [www.customsearch.ai](https://www.customsearch.ai)
   - Create a new instance
   - Add `sec.gov/Archives/edgar/*` to Active configuration
   - Note your Custom Configuration ID

## Step 2: Clone and Build

```bash
# Clone the repository
git clone https://github.com/boddev/bing_grounding_api.git
cd bing_grounding_api

# Build the solution
dotnet build
```

## Step 3: Configure Credentials

Choose one of these methods:

### Option A: Environment Variables (Recommended)

```bash
# Linux/macOS
export BING_CUSTOM_SEARCH_API_KEY='your-api-key-here'
export BING_CUSTOM_CONFIG_ID='your-config-id-here'

# Windows (PowerShell)
$env:BING_CUSTOM_SEARCH_API_KEY='your-api-key-here'
$env:BING_CUSTOM_CONFIG_ID='your-config-id-here'
```

### Option B: Configuration File

```bash
# Copy the example config
cp BingGroundingApi.Example/appsettings.example.json BingGroundingApi.Example/appsettings.json

# Edit and add your credentials
# (appsettings.json is in .gitignore for security)
```

## Step 4: Run the Example

```bash
cd BingGroundingApi.Example
dotnet run
```

You should see output like:

```
=================================================
SEC Edgar Custom Engine Agent - Example Usage
=================================================

Example 1: Searching for Apple Inc. (AAPL) documents...

Found 5 document(s):
...
```

## Step 5: Use in Your Code

Create a new file `MyApp.cs`:

```csharp
using BingGroundingApi.SecEdgar;
using BingGroundingApi.SecEdgar.Models;

// Get credentials
var apiKey = Environment.GetEnvironmentVariable("BING_CUSTOM_SEARCH_API_KEY");
var configId = Environment.GetEnvironmentVariable("BING_CUSTOM_CONFIG_ID");

// Create agent
using var agent = new SecEdgarCustomEngineAgent(apiKey, configId);

// Search for documents
var docs = await agent.GetDocumentsByTickerAsync("AAPL", maxResults: 5);

// Display results
foreach (var doc in docs)
{
    Console.WriteLine($"{doc.CompanyName} - {doc.DocumentType}");
    Console.WriteLine($"Filed: {doc.FilingDate:yyyy-MM-dd}");
    Console.WriteLine($"URL: {doc.DocumentUrl}\n");
}
```

## Common Commands

```bash
# Build the solution
dotnet build

# Run the example
cd BingGroundingApi.Example && dotnet run

# Clean build artifacts
dotnet clean

# Run with specific credentials (without setting env vars)
BING_CUSTOM_SEARCH_API_KEY='key' BING_CUSTOM_CONFIG_ID='id' dotnet run
```

## Troubleshooting

### "Missing required environment variables"
Set the environment variables as shown in Step 3.

### "Failed to execute search query"
- Verify your API key is correct
- Check your Custom Configuration ID
- Ensure your Azure subscription is active

### No Results Found
- Verify your Custom Search instance includes `sec.gov/Archives/edgar/*`
- Wait a few minutes for indexing to complete
- Try a broader search query

## What's Next?

- Read the [full README](README.md) for detailed documentation
- Check [USAGE.md](USAGE.md) for advanced examples
- Explore the API methods in `SecEdgarCustomEngineAgent.cs`

## Quick Examples

### Search by Ticker
```csharp
var docs = await agent.GetDocumentsByTickerAsync("MSFT", maxResults: 10);
```

### Get Recent 10-K Filings
```csharp
var reports = await agent.GetRecentFilingsAsync(SecDocumentType.Form10K, maxResults: 10);
```

### Search by Date Range
```csharp
var recent = await agent.SearchByDateRangeAsync(
    DateTime.Now.AddMonths(-3),
    DateTime.Now,
    maxResults: 20
);
```

### Combine Filters
```csharp
var results = await agent.SearchDocumentsAsync(
    query: "Microsoft",
    documentType: SecDocumentType.Form10Q,
    ticker: "MSFT",
    maxResults: 10
);
```

## Support

- **Documentation**: See [README.md](README.md) and [USAGE.md](USAGE.md)
- **Issues**: Open an issue on GitHub
- **Azure Support**: [azure.microsoft.com/support](https://azure.microsoft.com/support/)

Happy coding! 🚀
