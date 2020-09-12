using System.Text.Json;

namespace DataPowerTools.Connectivity.Json
{
    public static class JsonSerializationExtensions
    {
        public static TObject ToObject<TObject>(this string jsonString)
        {
            var r = JsonSerializer.Deserialize<TObject>(jsonString);

            return r;
        }

        public static string ToJson(this object serializableObject, bool indent = false, bool ignoreNonSerializableClassReferences = false)
        {
            var o = new JsonSerializerOptions
            {
                WriteIndented = indent
            };

            if (ignoreNonSerializableClassReferences)
            {
                o.Converters.Clear();
                o.Converters.Add(new SerializableAttributeConverter());
            }

            return JsonSerializer.Serialize(serializableObject, o);
        }
    }
}
