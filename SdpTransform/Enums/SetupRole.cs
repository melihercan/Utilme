using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform;

[JsonConverter(typeof(JsonStringEnumConverter<SetupRole>))]
public enum SetupRole
{
    [JsonStringEnumMemberName("active")]
    [Display(Name = "active")]
    Active,

    [JsonStringEnumMemberName("passive")]
    [Display(Name = "passive")]
    Passive,

    [JsonStringEnumMemberName("actpass")]
    [Display(Name = "actpass")]
    ActPass,

    [JsonStringEnumMemberName("holdconn")]
    [Display(Name = "holdconn")]
    HoldConn
}
