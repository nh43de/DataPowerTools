using DataPowerTools.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests.ReaderTests;

[TestClass]
public class RowProjectingDataReaderTests
{
    private static string Data = @"ItemId	Investor	ClientId	Investor
1	A	123	G
2	B	321	F
3	C	147	Z
4	D	158	D";


    [TestMethod]
    public void TestSelectRows()
    {
        var rr = Data
            .ReadCsvString('\t', true)
            .SelectRows(new[]
            {
                "ItemId", "ClientId"
            })
            .AsCsv();

        ;

        var expected = @"""ItemId"",""ClientId""
""1"",""123""
""2"",""321""
""3"",""147""
""4"",""158""
";

        Assert.AreEqual(expected, rr);
    }

//    [TestMethod]
//    public void TestRemoveDuplicates()
//    {
//        var rr = Data
//            .ReadCsvString('\t', true)
//            .ApplyColumnAlias(, "Investor")
//            .RemoveDuplicateColumnNames()
//            .AsCsv();
//        ;

//        var expected = @"""ItemId"",""ClientId""
//""1"",""123""
//""2"",""321""
//""3"",""147""
//""4"",""158""
//";

//        Assert.AreEqual(expected, rr);

//    }
}