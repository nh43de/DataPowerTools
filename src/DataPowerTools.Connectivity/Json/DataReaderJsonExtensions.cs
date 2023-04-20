﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DataPowerTools.Extensions;
using DataPowerTools.PowerTools;
using SimpleCSV;

namespace DataPowerTools.Connectivity.Json
{
    public static class DataReaderJsonExtensions
    {
        public static string ReadToJson(this IDataReader reader, bool indent = false)
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

        /// <summary>
        /// Creates insert statements from an array of json objects.
        /// </summary>
        public static string FromJsonToCsv(this string jsonString, bool writeHeaders = true, char delimiter = ',')
        {
            var sb = new StringBuilder();
            
            var el = JsonDocument.Parse(jsonString).RootElement;

            var sw = new StringWriter(sb);

            using var csvWriter = new CSVWriter(sw, delimiter, CSVWriter.DefaultQuoteCharacter, CSVWriter.DefaultEscapeCharacter, "\r\n");
            
            var hasWrittenHeaders = !writeHeaders;
            void WriteHeaders(JsonElement jsonElement)
            {
                hasWrittenHeaders = true;

                var headers = jsonElement.EnumerateObject().Select(p => p.Name).ToArray();

                csvWriter.WriteNext(headers);
            }

            using (csvWriter)
            using (sw)
            {
                
                //enumerate array
                foreach (var jsonElement in el.EnumerateArray())
                {
                    if (jsonElement.ValueKind != JsonValueKind.Object)
                        continue;
                    
                    if(hasWrittenHeaders == false)
                        WriteHeaders(jsonElement);

                    //write values
                    var values = jsonElement.EnumerateObject()
                        .Select(p => p.Value.ToString())
                        .ToArray();

                    csvWriter.WriteNext(values);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates insert statements from an array of json objects.
        /// </summary>
        public static string FromJsonToSqlInsertStatements(this string jsonString, string tableName, DatabaseEngine engine = DatabaseEngine.SqlServer)
        {
            var sb = new StringBuilder();

            var isb = new InsertSqlBuilder(engine);
            
            //var el = ParseJsonAsArray(jsonString);
            var el = JsonDocument.Parse(jsonString).RootElement;
            

            //enumerate array
            foreach (var jsonElement in el.EnumerateArray())
            {
                //enumerate object
                if (jsonElement.ValueKind != JsonValueKind.Object)
                    continue;


                var objEnumerator = jsonElement.EnumerateObject();

                var dd = objEnumerator
                    .ToDictionary<JsonProperty, string, object>(jsonProperty => jsonProperty.Name,
                        jsonProperty => jsonProperty.Value.ToString());

                isb.AppendFromValuesDictionary(sb, dd, tableName);
            }
            
            return sb.ToString();
        }

        ///// <summary>
        ///// Parses json and wraps in array if not already.
        ///// </summary>
        ///// <param name="json"></param>
        ///// <returns></returns>
        ///// <exception cref="ArgumentNullException"></exception>
        //private static JsonElement ParseJsonAsArray(string json)
        //{
        //    if (json == null)
        //    {
        //        throw new ArgumentNullException(nameof(json));
        //    }

        //    var rootElement = JsonDocument.Parse(json).RootElement;

        //    if (rootElement.ValueKind == JsonValueKind.Array)
        //    {
        //        // The JSON string is already an array
        //        return rootElement;
        //    }
        //    else
        //    {
        //        // The JSON string is not an array, so we need to wrap it in a new array
        //        var jsonArray = new JsonArray
        //        {
        //            rootElement
        //        };

        //        return jsonArray.AsArray();
        //    }
        //}
    }
}
