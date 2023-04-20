using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using DataPowerTools.Extensions;

namespace DataPowerTools.Connectivity.Json
{
    public static class DataReaderJsonExtensions
    {
        public static string ToJson(this IDataReader reader, bool indent = false)
        {
            var props = reader.GetFieldNames();

            var objectArray = reader.SelectRows<JsonObject>(dr =>
                {
                    var jsonObject = new JsonObject();

                    foreach (var prop in props)
                    {
                        jsonObject[prop] = dr[prop].ToString();
                    }

                    return jsonObject;
                })
                .ToArray();

            return objectArray.ToJson(indent);
        }
        
    }
}
