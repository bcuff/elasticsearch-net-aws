using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Elasticsearch.Net.Aws
{
    internal static class StringUtil
    {
        public static string Trimall(this string value)
        {
            value = value.Trim();
            var output = new StringBuilder();
            using (var reader = new StringReader(value))
            {
                while (true)
                {
                    var next = reader.Peek();
                    if (next < 0) break;
                    var c = (char)next;
                    if (c == '"')
                    {
                        ReadQuotedString(output, reader);
                        continue;
                    }
                    if (char.IsWhiteSpace(c))
                    {
                        ReadWhitespace(output, reader);
                        continue;
                    }
                    output.Append((char)reader.Read());
                }
            }
            return output.ToString();
        }

        private static void ReadQuotedString(StringBuilder output, StringReader reader)
        {
            var start = reader.Read();
            Debug.Assert(start == '"');
            output.Append('"');
            bool escape = false;
            while (true)
            {
                var next = reader.Read();
                if (next < 0) break;
                var c = (char)next;
                output.Append(c);
                if (escape)
                {
                    escape = false;
                }
                else
                {
                    if (c == '"') break;
                    if (c == '\\') escape = true;
                }
            }
        }

        private static void ReadWhitespace(StringBuilder output, StringReader reader)
        {
            var lastWhitespace = (char)reader.Read();
            Debug.Assert(char.IsWhiteSpace(lastWhitespace));
            while (true)
            {
                var next = reader.Peek();
                if (next < 0) break;
                var c = (char)next;
                if (!char.IsWhiteSpace(c)) break;
                lastWhitespace = c;
                reader.Read();
            }
            output.Append(lastWhitespace);
        }
    }
}
