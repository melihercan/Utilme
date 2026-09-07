using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform;

[JsonConverter(typeof(JsonStringEnumConverter<CandidateType>))]
public enum CandidateType
{
    [JsonStringEnumMemberName("host")]
    [Display(Name="host")]
    Host,

    [JsonStringEnumMemberName("srflx")]
    [Display(Name = "srflx")]
    Srflx,

    [JsonStringEnumMemberName("prlfx")]
    [Display(Name = "prlfx")]
    Prflx,

    [JsonStringEnumMemberName("relay")]
    [Display(Name = "relay")]
    Relay
}
