using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using DataPowerTools.Strings;

namespace DataPowerTools.Extensions
{
    public static class SecureStringExtensions
    {
        public static string GetString(
            this SecureString source)
        {
            string result = null;
            var length = source.Length;
            var pointer = IntPtr.Zero;
            var chars = new char[length];

            try
            {
                pointer = Marshal.SecureStringToBSTR(source);
                Marshal.Copy(pointer, chars, 0, length);

                result = string.Join("", chars);
            }
            finally
            {
                if (pointer != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(pointer);
                }
            }

            return result;
        }
    }
}