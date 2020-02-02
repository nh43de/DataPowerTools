using System;
using System.Linq;
using DataPowerTools.Extensions;
using Newtonsoft.Json;

namespace DataPowerTools.Serialization.Extensions
{
    public class SerializableAttributeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            var isSerializable = objectType.GetCustomAttributes(typeof(SerializableAttribute), false).Any();

            if (objectType.IsAnonymousType2())
                return false;

            return !(isSerializable || objectType.IsSimpleType() || objectType.IsEnumerable() );
        }
    }
}