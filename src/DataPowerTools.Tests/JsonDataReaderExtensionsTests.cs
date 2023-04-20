using DataPowerTools.Connectivity.Json;
using DataPowerTools.Extensions;
using DataPowerTools.PowerTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests;

[TestClass]
public class JsonDataReaderExtensionsTests
{
    [TestMethod]
    public void TestGenerateJson()
    {
        var csv = @"Col1	Col2	Col 3
AAA	AA	1
AAB	AA	2
AAC	AB	3
";


        var dd = csv.ReadCsvString('\t', true);

        var r = dd.ToJson(true);



    }



}