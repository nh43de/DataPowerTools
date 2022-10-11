//using System;
//using System.Collections.Generic;
//using System.IO;
//using Microsoft.Reporting.WinForms;

//namespace ReportViewerLib
//{
//    public class ReportFunctions
//    {
//        public static void GetReport(string reportServerUri, string reportPath, string exportPath,
//            List<ReportParameter> reportParameters)
//        {
//            var rview = new ReportViewer();
//            //Web Address of your report server (ex: http://rserver/reportserver)

//            rview.ServerReport.ReportServerUrl = new Uri(reportServerUri);

//            rview.ServerReport.ReportPath = reportPath;

//            rview.ServerReport.SetParameters(reportParameters);

//            string mimeType, encoding, extension;
//            string[] streamids;
//            Warning[] warnings;

//            var deviceInfo = "<DeviceInfo>201D<SimplePageHeaders>True</SimplePageHeaders></DeviceInfo>";

//            var format = "EXCELOPENXML"; //Desired format goes here (PDF, Excel, Image, or EXCELOPENXML)

//            var bytes = rview.ServerReport.Render(format, deviceInfo, out mimeType, out encoding, out extension,
//                out streamids, out warnings);

//            File.WriteAllBytes(exportPath, bytes);
//        }
//    }
//}