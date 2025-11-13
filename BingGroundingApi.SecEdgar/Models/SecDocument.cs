namespace BingGroundingApi.SecEdgar.Models;

/// <summary>
/// Represents a SEC Edgar document with metadata.
/// </summary>
public class SecDocument
{
    /// <summary>
    /// Gets or sets the type of SEC document (10-K, 10-Q, or 8-K).
    /// </summary>
    public SecDocumentType DocumentType { get; set; }

    /// <summary>
    /// Gets or sets the company ticker symbol.
    /// </summary>
    public string Ticker { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the company name.
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Central Index Key (CIK) assigned by the SEC.
    /// </summary>
    public string Cik { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the filing date of the document.
    /// </summary>
    public DateTime FilingDate { get; set; }

    /// <summary>
    /// Gets or sets the reporting period end date.
    /// </summary>
    public DateTime? PeriodEndDate { get; set; }

    /// <summary>
    /// Gets or sets the accession number (unique identifier for SEC filings).
    /// </summary>
    public string AccessionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL to the document on SEC Edgar.
    /// </summary>
    public string DocumentUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional metadata as key-value pairs.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Returns a string representation of the document.
    /// </summary>
    public override string ToString()
    {
        return $"{CompanyName} ({Ticker}) - {DocumentType} filed on {FilingDate:yyyy-MM-dd}";
    }
}
