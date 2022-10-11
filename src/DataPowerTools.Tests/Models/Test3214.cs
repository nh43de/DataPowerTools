namespace DataPowerTools.Tests.Models;

public class Test3214 : Test321
{
    public string Col4 { get; set; }

    public static Test3214[] GetTest3214s()
    {
        return new[]
        {
            new Test3214 {Col1 = 1, Col2 = 1.1m, Col3 = "TestCol3", Col4 = "excessive data"},
            new Test3214 {Col1 = 11, Col2 = 11.1m},
            new Test3214 {Col1 = 111, Col2 = 111.1m},
            new Test3214 {Col1 = 1111}
        };
    }
}