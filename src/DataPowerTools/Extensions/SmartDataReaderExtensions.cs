using System;
using System.Data;
using System.Linq;
using DataPowerTools.DataReaderExtensibility.Columns;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;

namespace DataPowerTools.Extensions
{
    public static class SmartDataReaderExtensions
    {
        public static T TryGet<T>(this Func<T> func, out string errorMessage, T defaultVal = default(T))
        {
            errorMessage = null;
            try
            {
                return func();
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return defaultVal;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TDataReader"></typeparam>
        /// <param name="smartDataReader"></param>
        /// <param name="printNonStringDestinations"></param>
        /// <returns></returns>
        public static string PrintDiagnostics<TDataReader>(this SmartDataReader<TDataReader> smartDataReader, bool printNonStringDestinations = false)
            where TDataReader : IDataReader
        {
            if(smartDataReader == null)
                return "";
            
            var sourceOrdinals = !printNonStringDestinations 
                ? smartDataReader
                    .ColumnMappingInfo
                    .NonStringDestinationSourceOrdinals 
                : smartDataReader
                    .ColumnMappingInfo
                    .SourceColumns.Select(c => c.Ordinal)
                    .ToArray();
            
            var rtn = sourceOrdinals
                    .OrderBy(i => i)
                    .Select((sourceOrdinal) =>
                    {
                        var srcCol = smartDataReader.ColumnMappingInfo.SourceColumns[sourceOrdinal];

                        var srcColName = srcCol.ColumnName;
                        var srcColType = srcCol.FieldType?.Name; //TODO: test this with inserting a null into non-null destination
                        var srcColVal = TryGet(() => smartDataReader.DataReader[sourceOrdinal]?.ToString() ?? "<null>", out string e1, "<none>");

                        var sourceStr = $"{sourceOrdinal}. [{srcColName}] ({srcColType}) '{srcColVal}'";

                        var destColOrdinal =
                            smartDataReader.ColumnMappingInfo.SourceOrdinalToDestinationOrdinal[sourceOrdinal];

                        var destinationStr = "<null>";
                        if (destColOrdinal.HasValue)
                        {
                            var destCol = smartDataReader.ColumnMappingInfo.DestinationColumns[destColOrdinal.Value];

                            var destColType = destCol.DataType.Name;
                            var destColName = destCol.ColumnName;
                            var destVal = TryGet(() => smartDataReader[sourceOrdinal]?.ToString() ?? "<null>", out string e2, "<none>");
                            
                            destinationStr = $"{destColOrdinal}. [{destColName}] ({destColType}) '{destVal}'";

                            if (e2 != null)
                                destinationStr += $" (ERROR - {e2})";
                        }

                        return $"{sourceStr} -> {destinationStr}";
                    })
                    .JoinStr("\r\n");

            return rtn;
        }



        public static string GetSmartDataReaderNonStringDestinationsErrorMessage<TDataReader>(this SmartDataReader<TDataReader> smartDataReader)  where TDataReader : IDataReader
        {
            var errmsg = "";
            if (smartDataReader.DataReader != null)
                try
                {
                    var nonStringDestinationNames =
                        smartDataReader
                            .ColumnMappingInfo
                            .SourceOrdinalDestinationIsString
                            .Invert()
                            .Select((d, i) => $"[{smartDataReader.GetName(i)}]: '{smartDataReader[i]}'")
                            .ToArray();

                    errmsg = string.Join("\r\n", nonStringDestinationNames);
                }
                catch (Exception)
                {
                    var nonStringDestinationValues =
                        smartDataReader
                            .ColumnMappingInfo
                            .SourceOrdinalDestinationIsString
                            .Invert()
                            .Select((d, i) => $"Column {i}: '{smartDataReader[i]}'")
                            .ToArray();

                    errmsg = string.Join("\r\n", nonStringDestinationValues);
                }
            return errmsg;
        }


        public static SmartDataReaderDiagnosticInfo GetSmartDataReaderDiagnosticInfo<TDataReader>(
                this SmartDataReader<TDataReader> smartDataReader) where TDataReader : IDataReader
        {
            var d = new SmartDataReaderDiagnosticInfo
            {
                Mappings = smartDataReader.ColumnMappingInfo,
                TransformGroups = PrintTransformGroups(smartDataReader),
                
                Depth = smartDataReader.Depth
            };
            try
            {
                d.NonStringDestinationValues = GetNonStringDestinationFieldValues(smartDataReader);
            }
            catch (Exception e)
            {
                //
            }
            return d;
        }

        public static string PrintColumnMappings<TDataReader>(this SmartDataReader<TDataReader> reader) where TDataReader : IDataReader
            => reader.ColumnMappingInfo.PrintMappings();
        
        public static string[] PrintTransformGroups<TDataReader>(this SmartDataReader<TDataReader> reader) where TDataReader : IDataReader
            => reader.DataTransformsInDestinationOrder.Select((t, i) =>
                        $"{i}. {t.Transform.Method.Name}"
            ).ToArray();

        public static FieldValueInfo[] GetNonStringDestinationFieldValues<TDataReader>(this SmartDataReader<TDataReader> reader) where TDataReader : IDataReader
        {
            return reader.GetFieldValues(reader.ColumnMappingInfo.NonStringDestinationSourceOrdinals);
        }
    }
}