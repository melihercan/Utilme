using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform;

[JsonConverter(typeof(JsonStringEnumConverter<CandidateTransport>))]
public enum CandidateTransport
{
    [JsonStringEnumMemberName("udp")]
    [Display(Name = "udp")]
    Udp,

    [JsonStringEnumMemberName("tcp")]
    [Display(Name = "tcp")]
    Tcp
}
