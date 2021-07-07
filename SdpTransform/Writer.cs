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
        readonly Grammar _grammer = new();
        readonly char[] _outerOrders = { 'v', 'o', 's', 'i', 'u', 'e', 'p', 'c', 'b', 't', 'r', 'z', 'a' };
        readonly char[] _innerOrders = { 'i', 'c', 'b', 'a' };
        readonly Regex _formatRegExp = new("/%[sdv%]/g");

        public string Write(string session/*, object opts*/)
        {
            var jElement = JsonDocument.Parse(session).RootElement;
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
            
            List<string> sdp = new();

            foreach(var type in _outerOrders)
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

            foreach (var mlineJElement in mediaArray)
            {
                var mline = mlineJElement.GetRawText();
                sdp.Add(MakeLine('m', _grammer.Rules['m'][0], mline));

                foreach(var type in _innerOrders)
                {
                    foreach (var obj in _grammer.Rules[type])
                    {
                        if (mline.Contains(obj.Name) &&
                            mlineJElement.GetProperty(obj.Name).ValueKind != JsonValueKind.Undefined)
                        {
                            sdp.Add(MakeLine(type, obj, mline));
                        }
                        else if (mline.Contains(obj.Push) &&
                            mlineJElement.GetProperty(obj.Push).ValueKind != JsonValueKind.Undefined)
                        {
                            foreach (var el in mlineJElement.GetProperty(obj.Push).EnumerateArray())
                            {
                                sdp.Add(MakeLine(type, obj, el.GetString()));
                            }
                        }
                    }
                }
            }

            return string.Join("\r\n", sdp);
        }

        string Format(params string[] formatStr)
        {

            return "";
        }


        string MakeLine(char type, Grammar.Rule obj, string location)
        {
            var jElement = JsonDocument.Parse(location).RootElement;

            var str = obj.FormatFunc is not null ? 
                obj.FormatFunc(obj.Push is not null ? location : jElement.GetPropertyAsString(obj.Name)) : 
                obj.Format; 

            List<string> args = new();
            args.Add(type + "=" + str);
            if (obj.Names.Count() != 0)
            {
                for (var i=0; i< obj.Names.Length; i++)
                {
                    var n = obj.Names[i];
                    if (obj.Name is not null)
                        //// TODO: CHECK THIS 
                        args.Add(jElement.GetPropertyAsString(obj.Name) + n);
                    else
                        args.Add(jElement.GetPropertyAsString(n));
                }
            }
            else
            {
                args.Add(jElement.GetPropertyAsString(obj.Name));
            }

            return "";
        }
    }
}
 