using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform;

[JsonConverter(typeof(JsonStringEnumConverter<NetType>))]
public enum NetType
{
    [JsonStringEnumMemberName("IN")]
    [Display(Name="IN")]
    Internet
}
