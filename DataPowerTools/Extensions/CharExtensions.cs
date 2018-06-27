using System;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using DataPowerTools.Strings;

namespace DataPowerTools.Extensions
{
    public static class CharExtensions
    {
        public static SecureString ToSecureString(this char[] str)
        {
            var knox = new SecureString();
            foreach (var c in str)
            {
                knox.AppendChar(c);
            }
            return knox;
        }
    }
}