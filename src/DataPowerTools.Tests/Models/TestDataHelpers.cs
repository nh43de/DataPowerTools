using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataPowerTools.Extensions;

namespace DataPowerTools.Tests.ReaderTests
{
    public static class TestDataHelpers
    {
        public static IEnumerable<object[]> GetDataReaderSourceTypes()
        {
            return new[]
            {
                new object[] { DataReaderSource.Csv },
                new object[] { DataReaderSource.DataTable },
                new object[] { DataReaderSource.ObjectReader }
            };
        }

        
        public static IDataReader GetSampleDataReader(DataReaderSource source, int count)
        {
            var r =
                Enumerable.Range(1, count).Select(i => new
                    {
                        Col1 = i,
                        Col2 = i * 10,
                        Col3 = $"{i}-abc",
                    })
                    .ToArray();

            var rr = source switch
            {
                DataReaderSource.Csv => (IDataReader) Csv.ReadString(r.ToCsvString()),
                DataReaderSource.DataTable => (IDataReader) r.ToDataTable().ToDataReader(),
                DataReaderSource.ObjectReader => (IDataReader) r.ToDataReader()
            };

            return rr;
        }

    }
}