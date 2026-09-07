using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform;

[JsonConverter(typeof(JsonStringEnumConverter<GroupSemantics>))]
public enum GroupSemantics
{
    [JsonStringEnumMemberName("LS")]
    [Display(Name ="LS")]
    LipSynchronization,

    [JsonStringEnumMemberName("FID")]
    [Display(Name = "FID")]
    FlowIdentification,

    [JsonStringEnumMemberName("BUNDLE")]
    [Display(Name = "BUNDLE")]
    Bundle
}
