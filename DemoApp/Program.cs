using System;
using System.Text.RegularExpressions;
using Utilme;

namespace DemoApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var res = @"
{ 
    ""version"": 0,
    ""origin"": { 
       ""username"": ""-"",
       ""sessionId"": 20518,
       ""sessionVersion"": 0,
       ""netType"": ""IN"",
       ""ipVer"": 4,
       ""address"": ""203.0.113.1""
    },
    ""name"": """",
    ""timing"": { ""start"": 0, ""stop"": 0
    },
    ""connection"": { ""version"": 4, ""ip"": ""203.0.113.1""
    },
    ""iceUfrag"": ""F7gI"",
    ""icePwd"": ""x9cml/YzichV2+XlhiMu8g"",
    ""fingerprint"": { ""type"": ""sha-1"",
       ""hash"": ""42: 89:c5:c6: 55: 9d:6e:c8:e8: 83: 55: 2a: 39:f9:b6:eb:e9:a3:a9:e7""
    },
    ""media"": [
        {
         ""type"": ""audio"",
         ""port"": 54400,
         ""protocol"": ""RTP/SAVPF"",
         ""payloads"": ""0 96"",
         ""ptime"": 20,
         ""direction"": ""sendrecv""
        },
        {
         ""type"": ""video"",
         ""port"": 55400,
         ""protocol"": ""RTP/SAVPF"",
         ""payloads"": ""97 98"",
         ""direction"": ""sendrecv""
        }
    ]
}
".Replace(" ","");

            Console.WriteLine("Hello World!");

            ////SdpTransform sdpTransform = new();

            ////var ret = sdpTransform.Write(res);


        }
    }
}
