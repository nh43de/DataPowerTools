using System;
using System.Data;
using System.Globalization;
using System.Linq;
using DataPowerTools.DataReaderExtensibility.Columns;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;
using DataPowerTools.Extensions;
using DataPowerTools.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests
{
    [TestClass]
    public class SmartDataReaderTests
    {
        [TestInitialize]
        public void TestInitialize()
        {

        }

        [TestMethod]
        public void TestMapping()
        {
            var r = new[]
            {
                new
                {
                    Col301 = "abc",
                    Col200 = 20,
                    Col201 = 20,
                    Col302 = "abc",
                    Col1 = 10,
                    Col2 = 20,
                    Col202 = 20,
                    Col203 = 20,
                    Col3 = "abc",
                }
            }.ToDataReader();

            var sr = new SmartDataReader<IDataReader>(r, new[]
            {
                new TypedDataColumnInfo
                {
                    ColumnName = "Col3",
                    DataType = typeof(string),
                    FieldType = typeof(string),
                    Ordinal = 0
                },
                new TypedDataColumnInfo
                {
                    ColumnName = "Col1",
                    DataType = typeof(int),
                    FieldType = typeof(int),
                    Ordinal = 2
                },
                new TypedDataColumnInfo
                {
                    ColumnName = "Col2",
                    DataType = typeof(int),
                    FieldType = typeof(int),
                    Ordinal = 1
                }
            });

            Assert.IsTrue(sr.ColumnMappingInfo.NonStringDestinationSourceOrdinals.ToHashSet().IsSubsetOf(new[] { 4, 5 }));

            Assert.IsTrue(sr.ColumnMappingInfo.DestinationColumns.Length == 3);

            var col1 = sr.ColumnMappingInfo.Mappings.First(m => m.SourceField.ColumnName == "Col1" &&
                                                                m.DestinationField.ColumnName == "Col1");

            Assert.IsTrue(col1.SourceField.Ordinal == 4 && col1.DestinationField.Ordinal == 2);

            Assert.IsTrue(sr.ColumnMappingInfo.SourceColumnNameToDestinationOrdinal["Col1"] == 2);

            Assert.IsTrue(sr.ColumnMappingInfo.SourceColumns[0].ColumnName == "Col301");

            Assert.IsTrue(sr.ColumnMappingInfo.SourceOrdinalDestinationIsString[0]);
            Assert.IsTrue(sr.ColumnMappingInfo.SourceOrdinalDestinationIsString[4] == false);
            Assert.IsTrue(sr.ColumnMappingInfo.SourceOrdinalDestinationIsString[5] == false);

            Assert.IsTrue(sr.ColumnMappingInfo.SourceOrdinalToDestinationOrdinal[4] == 2);
            Assert.IsTrue(sr.ColumnMappingInfo.SourceOrdinalToDestinationOrdinal[5] == 1);
            Assert.IsTrue(sr.ColumnMappingInfo.SourceOrdinalToDestinationOrdinal[8] == 0);

            sr.Read();

            Assert.AreEqual(sr["Col1"], 10);
            Assert.AreEqual(sr["Col3"], "abc");
            Assert.AreEqual(sr["Col2"], 20);
        }

        [TestMethod]
        public void TestSmartDataReaderConversions()
        {
            var d = Test123.GetTest123s2();

            var clientId = Guid.NewGuid();
            var dt = DateTime.Now;
            var dc = "";
            var ga = "0.02";

            var dr = d.ToDataReader()
                .AddColumn("ClientId", row => clientId)
                .AddColumn("Rd", row => dt)
                .AddColumn("Dc", row => dc)
                .AddColumn("Ga", row => ga);
            
            var destinationColumns = new TypedDataColumnInfo[]
            {
                new TypedDataColumnInfo
                {
                    DataType = typeof(DateTime),
                    Ordinal = 15,
                    ColumnName = "Rd"
                },
                new TypedDataColumnInfo
                {
                    DataType = typeof(Guid),
                    Ordinal = 4,
                    ColumnName = "ClientId"
                },
                new TypedDataColumnInfo
                {
                    DataType = typeof(decimal),
                    Ordinal = 8,
                    ColumnName = "Dc"
                },
                new TypedDataColumnInfo
                {
                    DataType = typeof(decimal),
                    Ordinal = 3,
                    ColumnName = "Ga"
                }
            };
            
            var s = new SmartDataReader<IDataReader>(dr, destinationColumns, DataTransformGroups.Default);

            var r = s.CountRows();

            r.ReadToEnd();

            Assert.AreEqual(clientId.ToString(), r["ClientId"].ToString());
            Assert.AreEqual(dt.ToString(), r["Rd"].ToString());
            Assert.AreEqual(null, r["Dc"]);
            Assert.AreEqual(0.02m,  Convert.ToDecimal(r["Ga"].ToString()));
        }
    }
}