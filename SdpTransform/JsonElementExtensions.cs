using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UtilmeSdpTransform
{
    static class JsonElementExtensions
    {
        public static string GetPropertyAsString(this JsonElement jElement, string propertyName)
        {
            var property = jElement.GetProperty(propertyName);
            return property.ValueKind switch
            {
                // Assuming string or number is SDP values.
                JsonValueKind.String => property.GetString(),
                JsonValueKind.Number => property.GetInt32().ToString(),
                _ => throw new NotSupportedException()
            };
        }

    }
}
