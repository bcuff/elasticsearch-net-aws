using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elasticsearch.Net.Aws
{
    internal static class StringBuilderExtensions
    {
        private static readonly char[] _hexUpperChars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        public static void AppendHexEscaped(this StringBuilder builder, char value)
        {
            builder.Append('%');
            builder.Append(_hexUpperChars[(value & 0xf0) >> 4]);
            builder.Append(_hexUpperChars[value & 0xf]);
        }
    }
}
