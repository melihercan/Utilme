namespace Utilme.SdpTransform;

/// <summary>
/// How serious a <see cref="SdpDiagnostic"/> is.
/// </summary>
public enum SdpDiagnosticSeverity
{
    /// <summary>
    /// A line was not understood and was skipped. Parsing continued and produced an
    /// <see cref="Sdp"/>, but that line's information was dropped.
    /// </summary>
    Warning,

    /// <summary>
    /// Parsing could not complete. No <see cref="Sdp"/> is produced.
    /// </summary>
    Error,
}
