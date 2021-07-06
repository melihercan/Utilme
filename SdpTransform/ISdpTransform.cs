using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilme
{
    public interface ISdpTransform
    {
        string Parse(string sdp);
        string ParseParams(string str);

        object[] ParsePayloads(string str);

        string ParseImageAttributes(string str);

        string ParseSimulcastStreamList(string str);

        string Write(string session/*, object opts*/);
    }
}
