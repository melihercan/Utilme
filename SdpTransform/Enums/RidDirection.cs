using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform;

[JsonConverter(typeof(JsonStringEnumConverter<RidDirection>))]
public enum RidDirection
{
    [JsonStringEnumMemberName("recv")]
    [Display(Name = "recv")]
    Recv,

    [JsonStringEnumMemberName("send")]
    [Display(Name = "send")]
    Send,
}
