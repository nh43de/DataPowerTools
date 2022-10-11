//using System;

//namespace ReportViewerLib
//{
//    public static class ReportOutputExtensions
//    {
//        public static string GetFileExtension(this ReportOutputFormat reportOutputFormat)
//        {
//            switch (reportOutputFormat)
//            {
//                case ReportOutputFormat.PDF:
//                    return ".pdf";
//                case ReportOutputFormat.Image:
//                    return ".jpg";
//                case ReportOutputFormat.ExcelOpenXML:
//                    return ".xlsx";
//                case ReportOutputFormat.Excel:
//                    return ".xls";
//                default:
//                    throw new ArgumentOutOfRangeException(nameof(reportOutputFormat), reportOutputFormat, null);
//            }
//        }

//        public static string GetReportFormatCode(this ReportOutputFormat reportOutputFormat)
//        {
//            switch (reportOutputFormat)
//            {
//                case ReportOutputFormat.PDF:
//                    return "PDF";
//                case ReportOutputFormat.Image:
//                    return "IMAGE";
//                case ReportOutputFormat.ExcelOpenXML:
//                    return "EXCELOPENXML";
//                case ReportOutputFormat.Excel:
//                    return "EXCEL";
//                default:
//                    throw new ArgumentOutOfRangeException(nameof(reportOutputFormat), reportOutputFormat, null);
//            }
//        }
//    }
//}