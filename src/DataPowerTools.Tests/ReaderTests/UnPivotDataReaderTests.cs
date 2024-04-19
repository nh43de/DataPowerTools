using DataPowerTools.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests.ReaderTests;



[TestClass]
public class UnPivotDataReaderTests
{
    [TestMethod]
    public void TestUnPivot()
    {
        var csv = @"xxx	0.01	0.03	0.05
10	1	2	3
20	4	5	6
30	7	8	9
40	10	11	12";

        var dr = csv.ReadCsvString('\t', true);

        var ddr = dr.UnPivot();

        var rr = ddr.AsCsv(true);

        var checkCsv = @"""Dimension1"",""Dimension2"",""Value""
""10"",""0.01"",""1""
""10"",""0.03"",""2""
""10"",""0.05"",""3""
""20"",""0.01"",""4""
""20"",""0.03"",""5""
""20"",""0.05"",""6""
""30"",""0.01"",""7""
""30"",""0.03"",""8""
""30"",""0.05"",""9""
""40"",""0.01"",""10""
""40"",""0.03"",""11""
""40"",""0.05"",""12""
";

        Assert.AreEqual(checkCsv.Trim(), rr.Trim());
    }




    [TestMethod]
    public void TestUnPivotMulti()
    {
        var csv = @"sku	item	count	t096	t144	t192
sk1	sk	1	x	x	x
sk2	sk	2	x	x	x
sk3	sk	3		x	x";

        var dr = csv.ReadCsvString('\t', true);

        var ddr = dr.UnPivot(3);

        var rr = ddr.AsCsv(true);

        var checkCsv = @"""Dimension1"",""Dimension2"",""Dimension3"",""Dimension4"",""Value""
""sk1"",""sk"",""1"",""t096"",""x""
""sk1"",""sk"",""1"",""t144"",""x""
""sk1"",""sk"",""1"",""t192"",""x""
""sk2"",""sk"",""2"",""t096"",""x""
""sk2"",""sk"",""2"",""t144"",""x""
""sk2"",""sk"",""2"",""t192"",""x""
""sk3"",""sk"",""3"",""t096"",""""
""sk3"",""sk"",""3"",""t144"",""x""
""sk3"",""sk"",""3"",""t192"",""x""
";

        Assert.AreEqual(checkCsv, rr);
    }



}