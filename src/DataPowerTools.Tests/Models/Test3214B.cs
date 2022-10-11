namespace DataPowerTools.Tests.Models;

public class Test3214B : Test3214
{
    public bool Col5 { get; set; }

    public static Test3214B[] GetTest3214Bs()
    {
        return new[]
        {
            new Test3214B {Col1 = 1, Col2 = 1.1m, Col3 = "TestCol3", Col4 = "excessive data", Col5 = true},
            new Test3214B {Col1 = 11, Col2 = 11.1m},
            new Test3214B {Col1 = 111, Col2 = 111.1m, Col5 = true},
            new Test3214B {Col1 = 1111}
        };
    }
}