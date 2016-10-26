using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elasticsearch.Net.Aws
{
    internal static class HttpRequestExtensions
    {
        public static string GetHeaderValue(this IHttpRequest request, string name)
            => string.Join(",", request.GetHeaderValues(name));
    }
}
