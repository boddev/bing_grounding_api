using BingGroundingApi.SecEdgar;
using BingGroundingApi.SecEdgar.Models;

namespace BingGroundingApi.Example;

/// <summary>
/// Example application demonstrating the SEC Edgar Custom Engine Agent.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=================================================");
        Console.WriteLine("SEC Edgar Custom Engine Agent - Example Usage");
        Console.WriteLine("=================================================\n");

        // Configuration - In production, use secure configuration management
        var apiKey = Environment.GetEnvironmentVariable("BING_CUSTOM_SEARCH_API_KEY");
        var customConfigId = Environment.GetEnvironmentVariable("BING_CUSTOM_CONFIG_ID");

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(customConfigId))
        {
            Console.WriteLine("ERROR: Missing required environment variables:");
            Console.WriteLine("  - BING_CUSTOM_SEARCH_API_KEY");
            Console.WriteLine("  - BING_CUSTOM_CONFIG_ID");
            Console.WriteLine("\nPlease set these environment variables before running the application.");
            Console.WriteLine("\nExample setup:");
            Console.WriteLine("  export BING_CUSTOM_SEARCH_API_KEY='your-api-key-here'");
            Console.WriteLine("  export BING_CUSTOM_CONFIG_ID='your-config-id-here'");
            return;
        }

        using var agent = new SecEdgarCustomEngineAgent(apiKey, customConfigId);

        try
        {
            // Example 1: Search for documents by ticker
            Console.WriteLine("Example 1: Searching for Apple Inc. (AAPL) documents...\n");
            var appleDocuments = await agent.GetDocumentsByTickerAsync("AAPL", maxResults: 5);
            DisplayResults(appleDocuments);

            Console.WriteLine("\n" + new string('-', 50) + "\n");

            // Example 2: Search for recent 10-K filings
            Console.WriteLine("Example 2: Searching for recent 10-K filings...\n");
            var recent10KFilings = await agent.GetRecentFilingsAsync(SecDocumentType.Form10K, maxResults: 5);
            DisplayResults(recent10KFilings);

            Console.WriteLine("\n" + new string('-', 50) + "\n");

            // Example 3: Search for specific company and document type
            Console.WriteLine("Example 3: Searching for Microsoft 10-Q reports...\n");
            var msftQuarterlyReports = await agent.SearchDocumentsAsync(
                query: "Microsoft",
                documentType: SecDocumentType.Form10Q,
                ticker: "MSFT",
                maxResults: 5);
            DisplayResults(msftQuarterlyReports);

            Console.WriteLine("\n" + new string('-', 50) + "\n");

            // Example 4: Search by date range
            Console.WriteLine("Example 4: Searching for documents filed in the last 30 days...\n");
            var recentDocuments = await agent.SearchByDateRangeAsync(
                startDate: DateTime.Now.AddDays(-30),
                endDate: DateTime.Now,
                maxResults: 5);
            DisplayResults(recentDocuments);

            Console.WriteLine("\n" + new string('-', 50) + "\n");

            // Example 5: Custom query for 8-K filings
            Console.WriteLine("Example 5: Searching for recent 8-K current report filings...\n");
            var current8KReports = await agent.SearchDocumentsAsync(
                query: "current report",
                documentType: SecDocumentType.Form8K,
                maxResults: 5);
            DisplayResults(current8KReports);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nERROR: {ex.Message}");
            Console.WriteLine("\nPlease ensure:");
            Console.WriteLine("1. Your Bing Custom Search API key is valid");
            Console.WriteLine("2. Your Custom Configuration ID is correct");
            Console.WriteLine("3. Your custom search instance is configured to index SEC Edgar domain");
            Console.WriteLine("4. You have an active internet connection");
        }

        Console.WriteLine("\n=================================================");
        Console.WriteLine("Example completed!");
        Console.WriteLine("=================================================");
    }

    /// <summary>
    /// Displays search results in a formatted manner.
    /// </summary>
    private static void DisplayResults(List<SecDocument> documents)
    {
        if (documents == null || documents.Count == 0)
        {
            Console.WriteLine("No documents found.");
            return;
        }

        Console.WriteLine($"Found {documents.Count} document(s):\n");

        for (int i = 0; i < documents.Count; i++)
        {
            var doc = documents[i];
            Console.WriteLine($"{i + 1}. {doc}");
            Console.WriteLine($"   CIK: {doc.Cik}");
            Console.WriteLine($"   Accession: {doc.AccessionNumber}");
            Console.WriteLine($"   URL: {doc.DocumentUrl}");
            
            if (doc.Metadata.ContainsKey("snippet"))
            {
                var snippet = doc.Metadata["snippet"];
                if (!string.IsNullOrWhiteSpace(snippet))
                {
                    var shortSnippet = snippet.Length > 150 
                        ? snippet.Substring(0, 150) + "..." 
                        : snippet;
                    Console.WriteLine($"   Snippet: {shortSnippet}");
                }
            }
            
            Console.WriteLine();
        }
    }
}
