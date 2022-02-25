﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform;

public class JsonCamelCaseStringEnumConverter : JsonConverterFactory
{
    private static readonly JsonStringEnumConverter _jsonStringEnumConverter = new(JsonNamingPolicy.CamelCase);

    public override bool CanConvert(Type typeToConvert) =>
        _jsonStringEnumConverter.CanConvert(typeToConvert);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        _jsonStringEnumConverter.CreateConverter(typeToConvert, options);
}
