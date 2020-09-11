using System;

namespace DataPowerTools.Strings
{
    /// <summary>
    /// A date from string provider is a delegate that returns a DateTime for any given string.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public delegate DateTime? DateFromStringProvider(string str);
}