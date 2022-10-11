namespace DataPowerTools.Tests.Models
{
    public class Test1234 : Test123
    {
        public string Col4 { get; set; }
        
        public static Test1234[] GetTest1234s()
        {
            return new[]
            {
                new Test1234 {Col1 = 1, Col2 = 1.1m, Col3 = "TestCol3", Col4 = "excessive data"},
                new Test1234 {Col1 = 11, Col2 = 11.1m},
                new Test1234 {Col1 = 111, Col2 = 111.1m},
                new Test1234 {Col1 = 1111}
            };
        }
    }
}