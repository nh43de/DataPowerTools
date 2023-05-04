using System.Text.RegularExpressions;
using DataPowerTools.Extensions;
using DataPowerTools.PowerTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests;

[TestClass]
public class InsertSqlBuilderTests
{
    [TestMethod]
    public void TestCreateInserts()
    {
        var csv = @"Col1	Col2	Col 3
AAA	AA'C	''
AAB	AA""C	''
AAC	AB""D""'	''
";


        var dd = csv.ReadCsvString('\t', true);

        var r = dd.AsInsertStatements("MyTable", DatabaseEngine.SqlServer);

        Assert.AreEqual(@"INSERT INTO MyTable ([Col1],[Col2],[Col 3]) SELECT 'AAA' as [Col1],'AA''C' as [Col2],'''''' as [Col 3];
INSERT INTO MyTable ([Col1],[Col2],[Col 3]) SELECT 'AAB' as [Col1],'AA""C' as [Col2],'''''' as [Col 3];
INSERT INTO MyTable ([Col1],[Col2],[Col 3]) SELECT 'AAC' as [Col1],'AB""D""''' as [Col2],'''''' as [Col 3];
", r);

    }

    [TestMethod]
    public void TestFitToCsharpClass()
    {
        var csv = @"Col1	Col2	Col 3
AAA	AA'C	''
AAB	AA""C	''
AAC	AB""D""'	''
";

        var dd = csv.ReadCsvString('\t', true);

        var r = dd.FitToCsharpClass("MyClass");
        
        Assert.AreEqual(Regex.Unescape(@"public\ class\ MyClass\ \{\n\tpublic\ string\ Col1\ \{\ get;\ set;\ }\r\n\tpublic\ string\ Col2\ \{\ get;\ set;\ }\r\n\tpublic\ string\ Col3\ \{\ get;\ set;\ }\n}"), r);
    }
}