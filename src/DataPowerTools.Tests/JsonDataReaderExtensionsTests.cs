using DataPowerTools.Connectivity.Json;
using DataPowerTools.Extensions;
using DataPowerTools.PowerTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests;

[TestClass]
public class JsonDataReaderExtensionsTests
{
    [TestMethod]
    public void TestDataReaderToJsonExtension()
    {
        var csv = @"Col1	Col2	Col 3
AAA	AA	1
AAB	AA	2
AAC	AB	3
";

        var dd = csv.ReadCsvString('\t', true);

        var r = dd.ReadToJson(true);

        Assert.AreEqual(@"[
  {
    ""Col1"": ""AAA"",
    ""Col2"": ""AA"",
    ""Col 3"": ""1""
  },
  {
    ""Col1"": ""AAB"",
    ""Col2"": ""AA"",
    ""Col 3"": ""2""
  },
  {
    ""Col1"": ""AAC"",
    ""Col2"": ""AB"",
    ""Col 3"": ""3""
  }
]", r);
    }

    [TestMethod]
    public void TestGenerateSqlInsertsFromJson()
    {
        var json = @"[
  {
    ""Col1"": ""AAA"",
    ""Col2"": ""AA"",
    ""Col 3"": ""1""
  },
  {
    ""Col1"": ""AAB"",
    ""Col2"": ""AA"",
    ""Col 3"": ""2""
  },
  {
    ""Col1"": ""AAC"",
    ""Col2"": ""AB"",
    ""Col 3"": ""3"",
    ""Col4"": ""Z""
  }
]";
        
        var dd = json.FromJsonToSqlInsertStatements("MyTable", DatabaseEngine.SqlServer);

        Assert.AreEqual(@"INSERT INTO MyTable ([Col1],[Col2],[Col 3]) SELECT 'AAA' as [Col1],'AA' as [Col2],'1' as [Col 3];
INSERT INTO MyTable ([Col1],[Col2],[Col 3]) SELECT 'AAB' as [Col1],'AA' as [Col2],'2' as [Col 3];
INSERT INTO MyTable ([Col1],[Col2],[Col 3],[Col4]) SELECT 'AAC' as [Col1],'AB' as [Col2],'3' as [Col 3],'Z' as [Col4];
", dd);
    }


    [TestMethod]
    public void TestGenerateCsvFromJson()
    {
        var json = @"[
  {
    ""Col1"": ""AAA"",
    ""Col2"": ""AA"",
    ""Col 3"": ""1""
  },
  {
    ""Col1"": ""AAB"",
    ""Col2"": ""AA"",
    ""Col 3"": ""2""
  },
  {
    ""Col1"": ""AAC"",
    ""Col2"": ""AB"",
    ""Col 3"": ""3"",
    ""Col4"": ""Z""
    }
]";

        var dd = json.FromJsonToCsv(true);

        Assert.AreEqual(@"""Col1"",""Col2"",""Col 3"",""Col4""
""AAA"",""AA"",""1""
""AAB"",""AA"",""2""
""AAC"",""AB"",""3"",""Z""
", dd );
        
        var dd2 = json.FromJsonToCsv(false);

        Assert.AreEqual(@"""AAA"",""AA"",""1""
""AAB"",""AA"",""2""
""AAC"",""AB"",""3"",""Z""
", dd2);

    }
}