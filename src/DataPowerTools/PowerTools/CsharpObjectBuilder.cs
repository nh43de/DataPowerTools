using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataPowerTools.Extensions;

namespace DataPowerTools.PowerTools
{
    public class CSharpObjectBuilder
    {
        private readonly List<CSharpObjectInitDef> _defs = [];

        //private readonly StringBuilder _s = new StringBuilder();

        public CSharpObjectBuilder()
        {
            
        }

        public void AddCSharpObjectDef(IEnumerable<CSharpObjectInitDef> defs)
        {
            _defs.AddRange(defs);
        }

        public void AddCSharpObjectDef(CSharpObjectInitDef def)
        {
            _defs.Add(def);
        }

        public override string ToString()
        {
            var s = new StringBuilder();

            switch (_defs.Count)
            {
                case 0:
                    return string.Empty;
                case 1:
                    return BuildDefinition(_defs.First());
            }

            return BuildDefinition(_defs.ToArray());
        }

        public static string BuildDefinition(string val, CSharpObjInitType initType)
        {
            switch (initType)
            {
                case CSharpObjInitType.Default:
                case CSharpObjInitType.String:
                    return $@"""{val}""";
                    break;
                case CSharpObjInitType.Numeric:
                    return val;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(initType), initType, null);
            }
        }

        public static string BuildDefinition(CSharpObjectInitDef[] initsDefs)
        {
            var subItems = initsDefs
                .Select(BuildDefinition)
                .JoinStr(",\r\n");

            var template = $@"new[] {{
{subItems.Indent(1)}
}}";

            return template;
        }

        public static string BuildDefinition(CSharpObjectInitDef init)
        {
            var subItems = init
                .Inits
                .Select(def => $@"{def.Name.Replace(" ", "")} = {BuildDefinition(def.Value, def.DataType)}")
                .JoinStr(",\r\n");

            var template = $@"new() {{
{subItems.Indent(1)}
}}";

            return template;
        }

        public class CSharpObjectInitDef
        {
            //public string? TypeName
            public CSharpObjectInit[] Inits { get; set; }
        } 

        public class CSharpObjectInit
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public CSharpObjInitType DataType { get; set; }
        }
        
        public enum CSharpObjInitType
        {
            Default,
            String,
            Numeric
        }
    }
}
