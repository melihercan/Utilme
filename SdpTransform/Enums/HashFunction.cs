using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform;

[JsonConverter(typeof(JsonStringEnumConverter<HashFunction>))]
public enum HashFunction
{
    [JsonStringEnumMemberName("sha-1")]
    [Display(Name = "sha-1")]
    Sha1,

    [JsonStringEnumMemberName("sha-224")]
    [Display(Name = "sha-224")]
    Sha224,

    [JsonStringEnumMemberName("sha-256")]
    [Display(Name = "sha-256")]
    Sha256,

    [JsonStringEnumMemberName("sha-384")]
    [Display(Name = "sha-384")]
    Sha384,

    [JsonStringEnumMemberName("sha-512")]
    [Display(Name = "sha-512")]
    Sha512,

    [JsonStringEnumMemberName("md2")]
    [Display(Name = "md2")]
    Md2,

    [JsonStringEnumMemberName("md5")]
    [Display(Name = "md5")]
    Md5
}
