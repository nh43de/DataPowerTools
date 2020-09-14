using System.Data;
using System.Linq;
using DataPowerTools.DataReaderExtensibility.Columns;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;
using DataPowerTools.Extensions;
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

            Assert.IsTrue(sr.ColumnMappingInfo.NonStringDestinationSourceOrdinals.ToHashSet().IsSubsetOf(new[] {4, 5}));

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

            Assert.AreEqual(sr[0], "abc");
            Assert.AreEqual(sr[1], 20);
            Assert.AreEqual(sr[2], 20);
        }
    }
}