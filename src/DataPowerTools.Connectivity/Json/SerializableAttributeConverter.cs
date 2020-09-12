using System;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using DataPowerTools.Extensions;

namespace DataPowerTools.Connectivity.Json
{
    public class SerializableAttributeConverter : JsonConverter<object>
    {
        public override bool CanConvert(Type objectType)
        {
            var isSerializable = objectType.GetCustomAttributes(typeof(SerializableAttribute), false).Any();

            if (objectType.IsAnonymousType2())
                return false;

            return !(isSerializable || objectType.IsSimpleType() || objectType.IsEnumerable() );
        }

        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return null;
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToString());
        }
    }
}