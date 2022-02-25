using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform
{
    [JsonConverter(typeof(JsonCamelCaseStringEnumConverter))]
    public enum RTCIceCandidateType
    {
        Host,
        Srflx,
        Prflx,
        Relay
    }
}
