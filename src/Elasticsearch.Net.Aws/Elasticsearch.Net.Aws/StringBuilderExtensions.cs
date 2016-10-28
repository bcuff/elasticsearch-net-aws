using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elasticsearch.Net.Aws
{
    internal static class StringBuilderExtensions
    {
        private static readonly char[] HexUpperChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        public static void AppendUrlHexEscaped(this StringBuilder builder, char value)
        {
            const char leftMask = (char)0xf0;
            const char rightMask = (char)0x0f;
            builder.Append('%');
            builder.Append(HexUpperChars[(value & leftMask) >> 4]);
            builder.Append(HexUpperChars[value & rightMask]);
        }
    }
}
