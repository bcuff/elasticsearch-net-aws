#if NET45

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace Elasticsearch.Net.Aws
{
    public class HttpWebRequestAdapter : IRequest
    {
        private class HeadersAdapter : IHeaders
        {
            private readonly WebHeaderCollection _headers;

            public HeadersAdapter(WebHeaderCollection headers)
            {
                _headers = headers;
            }

            public string this[string name]
            {
                get { return _headers[name]; }
                set { _headers[name] = value; }
            }

            public IEnumerable<string> GetValues(string name) => _headers.GetValues(name);
            public IEnumerator GetEnumerator() => _headers.GetEnumerator();
        }

        private readonly HttpWebRequest _request;

        public HttpWebRequestAdapter(HttpWebRequest request)
        {
            _request = request;
            Headers = new HeadersAdapter(_request.Headers);
        }

        public IHeaders Headers { get; private set; }
        public string Method => _request.Method;
        public Uri RequestUri => _request.RequestUri;
    }
}

#endif