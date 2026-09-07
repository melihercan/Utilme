using System.Collections.Generic;

namespace Utilme.SdpTransform;

public class MediaDescription
{
    public MediaType Media { get; set; }

    public int Port { get; set; }

    public string Proto { get; set; }

    public IList<string> Fmts { get; set; }

    // Session overrides.

    /// <summary>
    /// i=<session description>
    /// Optional.
    /// </summary>
    public string Information { get; set; }

    /// <summary>
    /// c=<nettype> <addrtype> <connection-address>
    /// Either here or in media descriptions, so it is optional here.
    /// </summary>
    public ConnectionData ConnectionData { get; set; }

    /// <summary>
    /// b=<bwtype>:<bandwidth>
    /// Optional.
    /// </summary>
    public IList<Bandwidth> Bandwidths { get; set; }

    /// <summary>
    /// k=<method>
    /// k=<method>:<encryption key>
    /// Optional.
    /// Not recommended, new work is in progress.
    /// </summary>
    public EncryptionKey EncryptionKey { get; set; }



    // Attributes.
    public Attributes Attributes { get; set; }
}
