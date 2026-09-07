using System;

namespace Utilme.SdpTransform;

/// <summary>Helpers shared by the converters.</summary>
public static partial class ModelExtensions
{
    // Utility methods.

    public static byte[] HexadecimalStringToByteArray(String hexadecimalString)
    {
        int length = hexadecimalString.Length;
        byte[] byteArray = new byte[length / 2];
        for (int i = 0; i < length; i += 2)
        {
            byteArray[i / 2] = Convert.ToByte(hexadecimalString.Substring(i, 2), 16);
        }
        return byteArray;
    }

    public static long ToSeconds(this string str)
    {
        // Converts strings ending with the following letters to seconds.
        //  <digits>d - days (86400 seconds)
        //  <digits>h - hours (3600 seconds)
        //  <digits>m - minutes (60 seconds)
        //  <digits>s - seconds 
        if (str.EndsWith("d"))
            return long.Parse(str.TrimEnd('d')) * 86400;
        else if (str.EndsWith("h"))
            return long.Parse(str.TrimEnd('h')) * 3600;
        else if (str.EndsWith("m"))
            return long.Parse(str.TrimEnd('m')) * 60;
        else if (str.EndsWith("s"))
            return long.Parse(str.TrimEnd('s'));
        else
            throw new NotSupportedException();
    }
}
