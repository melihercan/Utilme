using System;

namespace Utilme.SdpTransform;

// a=extmap:<value>["/"<direction>] <URI> <extensionattributes>
public class Extmap
{
    public const string Label = "extmap:";

    public int Value { get; set; }
    public Direction? Direction { get; set; }
    public Uri Uri { get; set; }

    // Optional.        
    public string ExtensionAttributes { get; set; }
}
