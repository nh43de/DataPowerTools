using System;

namespace DataPowerTools.Tests.Models
{
    public class OfferRate
    {
        public short ClientId { get; set; }
        
        public DateTime RecordDate { get; set; }
        
        public string Account { get; set; }

        public double? OfferRate_ { get; set; } 

        public double? WaBookRate { get; set; }

        public OfferRate Client { get; set; }
    }
}