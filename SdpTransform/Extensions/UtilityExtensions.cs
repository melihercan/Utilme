using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace UtilmeSdpTransform;

internal static class UtilityExtensions
{
    /// <summary>
    /// Removes <paramref name="prefix"/> only when the string actually starts with it.
    /// </summary>
    /// <remarks>
    /// SDP field indicators and attribute labels ("o=", "candidate:") must be stripped from the
    /// front only. string.Replace is global and would also strip the token out of the middle of a
    /// value — corrupting, for example, a session name or a URI query that happens to contain it.
    /// </remarks>
    public static string StripPrefix(this string str, string prefix) =>
        str.StartsWith(prefix, StringComparison.Ordinal) ? str.Substring(prefix.Length) : str;

    public static string DisplayName(this Enum enumValue)
    {
        string displayName;
        displayName = enumValue.GetType()
            .GetMember(enumValue.ToString())
            .FirstOrDefault()
            .GetCustomAttribute<DisplayAttribute>()?
            .GetName();
        if (String.IsNullOrEmpty(displayName))
        {
            displayName = enumValue.ToString();
        }
        return displayName;
    }

    public static T EnumFromDisplayName<T>(this string name)
    {
        var type = typeof(T);
        if (!type.IsEnum) throw new InvalidOperationException();

        foreach (var field in type.GetFields())
        {
            var attribute = Attribute.GetCustomAttribute(field,
                typeof(DisplayAttribute)) as DisplayAttribute;
            if (attribute != null)
            {
                // SDP tokens appear in either case in the wild: RFC 5245 spells the candidate
                // transport "UDP" while RFC 8839 uses "udp". Accept both.
                if (string.Equals(attribute.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return (T)field.GetValue(null);
                }
            }
            else
            {
                if (string.Equals(field.Name, name, StringComparison.OrdinalIgnoreCase))
                    return (T)field.GetValue(null);
            }
        }

        throw new ArgumentOutOfRangeException("name");
    }
}
