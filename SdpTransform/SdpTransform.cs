using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilmeSdpTransform;

namespace Utilme
{
    public class SdpTransform : ISdpTransform
    {
        readonly Parser _parser = new();
        readonly Writer _writer = new();

        public string Parse(string sdp) => _parser.Parse(sdp);

        public string ParseImageAttributes(string str) => _parser.ParseImageAttributes(str);

        public string ParseParams(string str) => _parser.ParseParams(str);

        public object[] ParsePayloads(string str) => _parser.ParsePayloads(str);

        public string ParseSimulcastStreamList(string str) => _parser.ParseSimulcastStreamList(str);

        public string Write(string session) => _writer.Write(session);
    }
}
