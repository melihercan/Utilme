using AJP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UtilmeSdpTransform
{
    class Writer
    {
        readonly Grammer _grammer = new();
        readonly char[] OuterOrders = { 'v', 'o', 's', 'i', 'u', 'e', 'p', 'c', 'b', 't', 'r', 'z', 'a' };
        readonly char[] InnerOrders = { 'i', 'c', 'b', 'a' };
        readonly Regex FormatRegex = new("%[sdv%]");


        public string Write(string session/*, object opts*/)
        {
            var jElement = JsonDocument.Parse(session).RootElement;
            //var y = jElement.ToString();

            if (!jElement.TryGetProperty("version", out _))
                jElement.AddProperty("version", 0);
            if (!jElement.TryGetProperty("name", out _))
                jElement.AddProperty("name", " ");
            var mediaArray = jElement.GetProperty("media").EnumerateArray();
            foreach(var media in mediaArray)
            {
                if (!media.TryGetProperty("payloads", out _))
                    media.AddProperty("payloads", "");
            }
            
            //var mediaText = mediaArray.GetRawText();
            //var xxx = ((JsonElement)session.)
            //var jjEl = JsonDocument.Parse(mediaText).RootElement;

            //var z = jElement.ToString();



            List<string> sdp = new();
            foreach(var type in OuterOrders)
            {
                foreach(var obj in _grammer.Rules[type])
                {
                    if (session.Contains(obj.Name) && 
                        jElement.GetProperty(obj.Name).ValueKind != JsonValueKind.Undefined)
                    {
                        sdp.Add(MakeLine(type, obj, session));
                    }
                    else if (session.Contains(obj.Push) && 
                        jElement.GetProperty(obj.Push).ValueKind != JsonValueKind.Undefined)
                    {
                        foreach (var el  in jElement.GetProperty(obj.Push).EnumerateArray())
                        {
                            sdp.Add(MakeLine(type, obj, el.GetString()));
                        }
                    }
                }
            }




            return string.Join("\r\n", sdp);
        }

        string MakeLine(char type, Grammer.Rule rule, string location)
        {
            var jElement = JsonDocument.Parse(location).RootElement;

            var format = string.IsNullOrEmpty(rule.Format)
                ? rule.FormatFunc(!string.IsNullOrEmpty(rule.Push)
                    ? location
                    :string.IsNullOrEmpty(rule.Name)
                        ? jElement.GetProperty(rule.Name).GetString()
                        : location)
                : rule.Format;


            List<string> args = new();
            args.Add(type + "=" + format);
            if (rule.Names.Count() != 0)
            {

            }
            else
            {
                args.Add(jElement.GetProperty(rule.Name).GetString());
            }






            //List<string> args = new();
            //var itOk = jElement.TryGetProperty(rule.Name, out var it);

            //if (rule.Names.Count() != 0)
            //{
            //    foreach (var name in rule.Names)
            //    {

            //    }
            //}
            //else //if (itOk)
            //{
            //    args.Add();
            //}



            return "";
        }

    }
}
 