//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.Reporting.WinForms;

//namespace ReportViewerLib
//{
//    public class ReportParameterProcessTask
//    {
//        public string FileExportPath { get; set; }

//        public List<ReportParameter> ReportParameters { get; set; } = new List<ReportParameter>();

//        private string GetParam(string paramName)
//        {
//            var param = ReportParameters
//                .FirstOrDefault(p => string.Equals(p.Name, paramName, StringComparison.CurrentCultureIgnoreCase));

//            var values = param?.Values;

//            if (values == null || values.Count <= 0)
//                throw new Exception($"No value for parameter {paramName}");

//            return values[0];
//        }
//    }
//}