using System;

namespace DataPowerTools.Tests.Models
{
    public class CoFicoLtv
    {
        public short CoFicoLtvId { get; set; } // CoFicoLtvId (Primary key)
        public string CatCoFicoLtv { get; set; } // CatCoFicoLtv (length: 5)
        public double ValCoFicoLtv { get; set; } // ValCoFicoLtv

        public CoFicoLtv Failure => throw new Exception("Emulates access to diposed context + lazy loading.");
    }
}