using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform;

[JsonConverter(typeof(JsonStringEnumConverter<EncryptionKeyMethod>))]
public enum EncryptionKeyMethod
{
    [JsonStringEnumMemberName("clear")]
    [Display(Name= "clear")]
    Clear,

    [JsonStringEnumMemberName("base64")]
    [Display(Name = "base64")]
    Base64,

    [JsonStringEnumMemberName("uri")]
    [Display(Name = "uri")]
    Uri,

    [JsonStringEnumMemberName("prompt")]
    [Display(Name = "prompt")]
    Prompt
}
