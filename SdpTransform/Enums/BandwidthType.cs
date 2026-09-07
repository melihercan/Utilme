using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform;

[JsonConverter(typeof(JsonStringEnumConverter<BandwidthType>))]
public enum BandwidthType
{
    [JsonStringEnumMemberName("AS")]
    [Display(Name="AS")]
    ApplicationSpecific,

    [JsonStringEnumMemberName("CT")]
    [Display(Name = "CT")]
    ConferenceTotal,

    [JsonStringEnumMemberName("RS")]
    [Display(Name = "RS")]
    RtcpSender,

    [JsonStringEnumMemberName("RR")]
    [Display(Name = "RR")]
    RtcpReceiver,

    [JsonStringEnumMemberName("TIAS")]
    [Display(Name = "TIAS")]
    TransportIndependentMaximumBandwidth,
}
