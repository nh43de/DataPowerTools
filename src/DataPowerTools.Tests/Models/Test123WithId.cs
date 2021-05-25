using System.ComponentModel.DataAnnotations.Schema;

namespace DataPowerTools.Tests.Models
{
    public class Test123WithId
    {
        public int Col1 { get; set; }
        public decimal Col2 { get; set; }

        [Column("NewCol3")]
        public string Col3 { get; set; }
        
        public int Test123Id { get; set; }
    }
}