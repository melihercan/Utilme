using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UtilmeSdpTransform
{
    class Writer
    {
        readonly char[] OuterOrder = { 'v', 'o', 's', 'i', 'u', 'e', 'p', 'c', 'b', 't', 'r', 'z', 'a' };
        readonly char[] InnerOrder = { 'i', 'c', 'b', 'a' };

        public string Write(string session/*, object opts*/)
        {
            var jsonDocument = JsonDocument.Parse(session);
            //            if (!jsonDocument.RootElement.TryGetProperty("version", out var x))


            return "";
        }

    }
}
