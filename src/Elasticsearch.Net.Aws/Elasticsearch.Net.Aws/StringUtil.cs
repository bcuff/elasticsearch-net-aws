using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
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

        private static string GetCanonicalRequest(HttpWebRequest request, byte[] data)
        {
            var result = new StringBuilder();
            result.Append(request.Method);
            result.Append('\n');
            result.Append(request.RequestUri.AbsolutePath);
            result.Append('\n');
            result.Append(request.RequestUri.Query);
            result.Append('\n');
            WriteCanonicalHeaders(request, result);
            result.Append('\n');
            WriteSignedHeaders(request, result);
            result.Append('\n');
            WriteRequestPayloadHash(data, result);
            return result.ToString();
        }

        private static void WriteCanonicalHeaders(HttpWebRequest request, StringBuilder output)
        {
            var q = from string key in request.Headers
                    let headerName = key.ToLowerInvariant()
                    let headerValues = string.Join(",",
                        request.Headers
                        .GetValues(key) ?? Enumerable.Empty<string>()
                        .Select(v => v.Trimall())
                    )
                    orderby headerName ascending
                    select string.Format("{0}:{1}\n", headerName, headerValues);
            foreach (var line in q)
            {
                output.Append(line);
            }
        }

        private static void WriteSignedHeaders(HttpWebRequest request, StringBuilder output)
        {
            bool started = false;
            foreach (string headerName in request.Headers)
            {
                if (started) output.Append(';');
                output.Append(headerName.ToLowerInvariant());
                started = true;
            }
        }

        static readonly byte[] _emptyBytes = new byte[0];

        private static void WriteRequestPayloadHash(byte[] data, StringBuilder output)
        {
            data = data ?? _emptyBytes;
            byte[] hash;
            using (var algo = HashAlgorithm.Create("SHA256"))
            {
                hash = algo.ComputeHash(data);
            }
            foreach (var b in hash)
            {
                output.AppendFormat("{0:x2}", b);
            }
        }
    }
}
