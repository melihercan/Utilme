using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform;

[JsonConverter(typeof(JsonStringEnumConverter<MediaType>))]
public enum MediaType
{
    [JsonStringEnumMemberName("audio")]
    [Display(Name = "audio")]
    Audio,

    [JsonStringEnumMemberName("video")]
    [Display(Name = "video")]
    Video,

    [JsonStringEnumMemberName("text")]
    [Display(Name = "text")]
    Text,

    [JsonStringEnumMemberName("application")]
    [Display(Name = "application")]
    Application,

    [JsonStringEnumMemberName("message")]
    [Display(Name = "message")]
    Message,
}
