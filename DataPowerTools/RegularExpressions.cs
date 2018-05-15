using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataPowerTools
{
    public static class RegularExpressions
    {

        public static string WebUri =
                @"(http|https|ftp)\://(([a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(:[a-zA-Z0-9]*)?)|(([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3})))/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*"
            ;
    }
}
