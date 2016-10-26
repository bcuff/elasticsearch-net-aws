using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elasticsearch.Net.Aws
{
    internal interface IHttpRequest
    {
        Uri Uri { get; }
        string Method { get; }
        byte[] Body { get; }
        IEnumerable<string> GetHeaderKeys();
        IEnumerable<string> GetHeaderValues(string name);
        void SetHeader(string name, string value);
    }
}
