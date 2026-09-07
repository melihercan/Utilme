using System;

namespace Utilme.SdpTransform;

public class TimeZone
{
    public DateTime AdjustmentTime { get; set; }
    public TimeSpan Offset { get; set; }
}
