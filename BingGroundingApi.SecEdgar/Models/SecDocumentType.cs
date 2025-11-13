namespace BingGroundingApi.SecEdgar.Models;

/// <summary>
/// Represents the types of SEC Edgar documents supported by this agent.
/// </summary>
public enum SecDocumentType
{
    /// <summary>
    /// Form 10-K: Annual report with comprehensive overview of company's business and financial condition.
    /// </summary>
    Form10K,

    /// <summary>
    /// Form 10-Q: Quarterly report with unaudited financial statements and ongoing business updates.
    /// </summary>
    Form10Q,

    /// <summary>
    /// Form 8-K: Current report for major events that shareholders should know about.
    /// </summary>
    Form8K
}
