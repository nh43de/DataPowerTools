using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DataPowerTools.Extensions;

// ReSharper disable once CheckNamespace
namespace DataPowerTools.Tests
{
    public static class SampleData
    {
        public static DataTable HeadersOnSecondRow
        {
            get
            {
                var r1 = new
                {
                    Col1 = (string) null,
                    Col2 = (string) null,
                    Col3 = "abc",
                }.AsDataReader();

                var r2 = new
                {
                    Col1 = "Header1",
                    Col2 = "Header2",
                    Col3 = "Header3",
                }.AsDataReader();

                var r3 = new[]
                {
                    new
                    {
                        Col1 = 10,
                        Col2 = 20,
                        Col3 = "abc",
                    }
                }.Repeat(998).ToDataReader();

                var allData = r1.Union(r2).Union(r3);

                var dTable = allData.ToDataTable();

                return dTable;
            }
        }

    }
}
