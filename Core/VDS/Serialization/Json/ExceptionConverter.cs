﻿using System;
using System.Reflection;
using Newtonsoft.Json;

namespace VDS.Serialization.Json
{
    /// <summary>
    /// Represents an implementation of <see cref="JsonConverter"/> for dealing with <see cref="Exception"/>
    /// </summary>
    public class ExceptionConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(Exception).GetTypeInfo().IsAssignableFrom(objectType);
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return new Exception();
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var exception = value as Exception;
            writer.WriteStartObject();
            writer.WritePropertyName("message");
            writer.WriteValue(exception.Message);
            writer.WritePropertyName("stackTrace");
            writer.WriteValue(exception.StackTrace);
            writer.WriteEndObject();
        }
    }
}
