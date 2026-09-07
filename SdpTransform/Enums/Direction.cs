using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform;

[JsonConverter(typeof(JsonStringEnumConverter<Direction>))]
public enum Direction
{
    [JsonStringEnumMemberName("sendrecv")]
    [Display(Name = "sendrecv")]
    SendRecv,

    [JsonStringEnumMemberName("sendonly")]
    [Display(Name = "sendonly")]
    SendOnly,

    [JsonStringEnumMemberName("recvonly")]
    [Display(Name = "recvonly")]
    RecvOnly,

    [JsonStringEnumMemberName("inactive")]
    [Display(Name = "inactive")]
    Inactive
}
