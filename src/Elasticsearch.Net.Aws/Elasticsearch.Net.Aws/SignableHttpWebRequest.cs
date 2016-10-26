#if !DOTNETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Elasticsearch.Net.Aws
{
    internal class SignableHttpWebRequest : IHttpRequest
    {
        public HttpWebRequest Request { get; }

        public SignableHttpWebRequest(HttpWebRequest request, byte[] body)
        {
            Request = request;
            Body = body;
        }

        public Uri Uri => Request.RequestUri;

        public string Method => Request.Method;

        public byte[] Body { get; }

        public IEnumerable<string> GetHeaderKeys()
        {
            return Request.Headers.Cast<string>();
        }

        public IEnumerable<string> GetHeaderValues(string name)
        {
            return Request.Headers.GetValues(name);
        }

        public void SetHeader(string name, string value)
        {
            Request.Headers[name] = value;
        }
    }
}
#endif
