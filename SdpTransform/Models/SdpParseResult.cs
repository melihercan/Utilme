using System.Collections.Generic;

namespace Utilme.SdpTransform;

/// <summary>
/// The outcome of parsing SDP text: the parsed <see cref="Sdp"/> when it succeeded, plus every
/// problem the parser encountered.
/// </summary>
/// <remarks>
/// <para>
/// Produced by <see cref="ModelExtensions.ToSdpResult"/>, which never throws. Use it instead of
/// <see cref="ModelExtensions.ToSdp"/> when you need to know <em>why</em> parsing failed, or which
/// lines were skipped.
/// </para>
/// <para>
/// This type is deliberately local to this package: <c>Utilme.SdpTransform</c> has no dependencies
/// and is not going to take one. It is unrelated to <c>Utilme.Result</c>.
/// </para>
/// </remarks>
public sealed class SdpParseResult
{
    /// <summary>Creates a parse result.</summary>
    /// <param name="sdp">The parsed session description, or <see langword="null"/> on failure.</param>
    /// <param name="diagnostics">Problems encountered while parsing.</param>
    public SdpParseResult(Sdp sdp, IReadOnlyList<SdpDiagnostic> diagnostics)
    {
        Sdp = sdp;
        Diagnostics = diagnostics;
    }

    /// <summary>
    /// The parsed session description, or <see langword="null"/> if parsing could not complete.
    /// </summary>
    public Sdp Sdp { get; }

    /// <summary>Whether an <see cref="Sdp"/> was produced.</summary>
    /// <remarks>
    /// A valid result may still carry <see cref="SdpDiagnosticSeverity.Warning"/> diagnostics for
    /// lines that were skipped.
    /// </remarks>
    public bool IsValid => Sdp is not null;

    /// <summary>Every problem encountered, in the order the lines appear.</summary>
    public IReadOnlyList<SdpDiagnostic> Diagnostics { get; }

    /// <summary>Whether anything at all was reported.</summary>
    public bool HasDiagnostics => Diagnostics.Count > 0;
}
