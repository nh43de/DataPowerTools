using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataPowerTools.Tests.Models
{

    /*
     CREATE TABLE [Test123] (
        [Col1] INT     NOT NULL,
        [Col2] DECIMAL NULL,
        [Col3] VARCHAR(50)
    );
     */


    [Serializable]
    public class Test321
    {
        [Column("NewCol3")]
        public string Col3 { get; set; }
        public decimal Col2 { get; set; }

        public int Col1 { get; set; }

        public static Test321[] GetTest321s()
        {
            return new[]
            {
                new Test321 {Col1 = 1},
                new Test321 {Col1 = 11},
                new Test321 {Col1 = 111},
                new Test321 {Col1 = 1111}
            };
        }

        public static Test321[] GetTest321s2()
        {
            return new[]
            {
                new Test321 {Col1 = 1, Col2 = 1.1m, Col3 = "TestCol3"},
                new Test321 {Col1 = 11},
                new Test321 {Col1 = 111},
                new Test321 {Col1 = 1111}
            };
        }
    }

}