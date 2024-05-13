using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DataPowerTools.Extensions;
using DataPowerTools.Extensions.Objects;
using DataPowerTools.PowerTools;
using SimpleCSV;

namespace DataPowerTools.Connectivity.Json
{
    public static class DataReaderCsharpExtensions
    {
        public static string ReadToCSharpArray(this IDataReader reader, bool useAnonymousType = false)
        {
            var props = reader.GetFieldNames();
            
            var instances = reader.SelectRows<string>(dr =>
                {
                    var properties = new List<string>();
                    
                    foreach (var prop in props)
                    {
                        var val = dr[prop];

                        string s;
                        if (val == null)
                        {
                            s = $"{prop} = null".Indent(1);
                        }
                        else if (val.IsNumeric())
                        {
                            s = $"{prop} = {val}".Indent(1);
                        }
                        else
                        {
                            s = $"{prop} = \"{val}\"".Indent(1);
                        }

                        properties.Add(s);
                    }

                    var propsString = properties.JoinStr(",\r\n");

                    var instanceDeclaration = useAnonymousType
                        ? @$"new {{ 
{propsString}
}}".Indent(1)
                        : @$"new() {{ 
{propsString}
}}".Indent(1);

                    return instanceDeclaration;
                })
                .ToArray();

            var sBuilder = new StringBuilder();

            if (useAnonymousType)
            {
                sBuilder.AppendLine(@"new[] {");
            }
            else
            {
                sBuilder.AppendLine(@"new MyClass[] {");
            }

            var instancesString = instances.JoinStr(",\r\n");

            sBuilder.AppendLine(instancesString);

            sBuilder.Append(@"}");

            return sBuilder.ToString();
        }
    }
}
