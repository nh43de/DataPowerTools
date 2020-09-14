using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataPowerTools.Tests.Models
{
    public class Test123V1
    {
        [Key]
        [Column(Order = 1, TypeName = "VARCHAR(100)")]
        public string Pk1 { get; set; }

        [Column(Order = 2, TypeName = "DATE")]
        [Key]
        public DateTime Pk2 { get; set; }

        [Column(Order = 3, TypeName = "DATE")]
        public DateTime Col3 { get; set; }

        [Column(Order = 4, TypeName = "INT")]
        public int Col4 { get; set; }
    }
}