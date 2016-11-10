using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net.Aws;

namespace Tests
{
    public class MockHttpRequest : IHttpRequest
    {
        public Uri Uri { get; set; }
        public string Method { get; set; }
        public byte[] Body { get; set; }
        public NameValueCollection Headers { get; set; } = new NameValueCollection();
        public IEnumerable<string> GetHeaderKeys() => Headers.Keys.Cast<string>();

        public IEnumerable<string> GetHeaderValues(string name)
            => Headers?.GetValues(name) ?? Enumerable.Empty<string>();

        public void SetHeader(string name, string value)
        {
            if (Headers == null) Headers = new NameValueCollection();
            Headers[name] = value;
        }
    }
}
