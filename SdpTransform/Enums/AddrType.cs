using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform;

[JsonConverter(typeof(JsonStringEnumConverter<AddrType>))]
public enum AddrType
{
    [JsonStringEnumMemberName("IP4")]
    [Display(Name = "IP4")]
    Ip4,

    [JsonStringEnumMemberName("IP6")]
    [Display(Name = "IP6")]
    Ip6

}
