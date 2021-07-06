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

        public string Parse(string sdp)
        {
            throw new NotImplementedException();
        }

        public string ParseImageAttributes(string str)
        {
            throw new NotImplementedException();
        }

        public string ParseParams(string str)
        {
            throw new NotImplementedException();
        }

        public object[] ParsePayloads(string str)
        {
            throw new NotImplementedException();
        }

        public string ParseSimulcastStreamList(string str)
        {
            throw new NotImplementedException();
        }

        public string Write(string session)
        {
            throw new NotImplementedException();
        }
    }
}
