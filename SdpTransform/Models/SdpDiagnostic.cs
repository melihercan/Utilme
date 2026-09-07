namespace Utilme.SdpTransform;

/// <summary>
/// Something the parser could not handle, reported against the line that caused it.
/// </summary>
public sealed class SdpDiagnostic
{
    /// <summary>Creates a diagnostic.</summary>
    /// <param name="severity">Whether parsing continued.</param>
    /// <param name="lineNumber">1-based line number in the original text.</param>
    /// <param name="line">The offending line, trimmed.</param>
    /// <param name="message">What went wrong.</param>
    public SdpDiagnostic(SdpDiagnosticSeverity severity, int lineNumber, string line, string message)
    {
        Severity = severity;
        LineNumber = lineNumber;
        Line = line;
        Message = message;
    }

    /// <summary>Whether parsing continued past this problem.</summary>
    public SdpDiagnosticSeverity Severity { get; }

    /// <summary>
    /// 1-based line number in the original text, counting blank lines, or 0 when the problem is not
    /// attributable to a single line.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>The offending line, trimmed. Empty when not attributable to a single line.</summary>
    public string Line { get; }

    /// <summary>What went wrong.</summary>
    public string Message { get; }

    /// <inheritdoc/>
    public override string ToString() =>
        LineNumber > 0
            ? $"{Severity} (line {LineNumber}): {Message}"
            : $"{Severity}: {Message}";
}
