using DataPowerTools.Connectivity.Json;
using DataPowerTools.Extensions;
using DataPowerTools.PowerTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests;

[TestClass]
public class CsharpDataReaderExtensionsTests
{
    [TestMethod]
    public void TestReadToCSharpArrayExtension()
    {
        var csv = @"Col1	Col2	Col3
AAA	AA	1
AAB	AA	2
AAC	AB	3
";

        var dd = csv.ReadCsvString('\t', true);

        var r = dd.ReadToCSharpArray();

        Assert.AreEqual(@"new MyClass[] {
	new() { 
		Col1 = ""AAA"",
		Col2 = ""AA"",
		Col3 = 1
	}
	new() { 
		Col1 = ""AAB"",
		Col2 = ""AA"",
		Col3 = 2
	}
	new() { 
		Col1 = ""AAC"",
		Col2 = ""AB"",
		Col3 = 3
	}
}
", r);
    }
    
}