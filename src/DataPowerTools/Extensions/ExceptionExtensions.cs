using System;
using System.Collections.Generic;
using System.Linq;

namespace DataPowerTools.Extensions
{
    public static class ExceptionExtensions
    {
        public static string ConcatenateInners(this Exception ex)
        {
            var rtn = ex.Message + " " + string.Join(" ",  ex.GetAllInnerExceptions().Select(e => e.Message).ToArray());

            return string.IsNullOrWhiteSpace(rtn) ? "Unknown exception ocurred." : rtn;
        }

        public static IEnumerable<Exception> GetAllInnerExceptions(this Exception ex)
        {
            var iex = ex.InnerException;
            while (iex != null)
            {
                yield return iex;
                iex = iex.InnerException;
            }
        }
    }
}
