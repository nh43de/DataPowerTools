using System.ComponentModel.DataAnnotations.Schema;

namespace DataPowerTools.Tests.Models
{
    public class Test123WithRef
    {
        public int Col1 { get; set; }
        public decimal Col2 { get; set; }

        [Column("NewCol3")]
        public string Col3 { get; set; }

        public Test123 Test123 { get; set; }
    }
}