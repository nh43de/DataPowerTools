using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataPowerTools.Tests.Models
{
    [Serializable]
    public class Test123
    {
        public int Col1 { get; set; }
        public decimal Col2 { get; set; }

        [Column("NewCol3")]
        public string Col3 { get; set; }

        public static Test123[] GetTest123s()
        {
            return new[]
            {
                new Test123 {Col1 = 1},
                new Test123 {Col1 = 11},
                new Test123 {Col1 = 111},
                new Test123 {Col1 = 1111}
            };
        }

        public static Test123[] GetTest123s2()
        {
            return new[]
            {
                new Test123 {Col1 = 1, Col2 = 1.1m, Col3 = "TestCol3"},
                new Test123 {Col1 = 11},
                new Test123 {Col1 = 111},
                new Test123 {Col1 = 1111}
            };
        }
    }


}