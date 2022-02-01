using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DataPowerTools.DataReaderExtensibility.Columns;
using DataPowerTools.FastMember;
using DataPowerTools.PowerTools;

namespace DataPowerTools.Extensions
{
    public static class ByteExtensions
    {
        public static int ToInt(this byte[] obj)
        {
            return BitConverter.ToInt32(obj, 0);
        }
    }
}