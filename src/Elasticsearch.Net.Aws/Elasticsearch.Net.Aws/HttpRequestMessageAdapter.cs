#if NETSTANDARD1_6

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elasticsearch.Net.Aws
{
    public class HttpRequestMessageAdapter : IRequest
    {
        private class HeadersAdapter : IHeaders
        {
            private readonly HttpRequestHeaders _headers;

            public HeadersAdapter(HttpRequestHeaders headers)
            {
                _headers = headers;
            }

            public string this[string name] 
            { 
                get { return String.Join(",", _headers.GetValues(name)); }
                set { _headers.Add(name, value); }
            }

            public IEnumerable<string> GetValues(string name) => _headers.GetValues(name);
            public IEnumerator GetEnumerator() => _headers.GetEnumerator();
        }

        private readonly HttpRequestMessage _message;

        public HttpRequestMessageAdapter(HttpRequestMessage message)
        {
            _message = message;
            Headers = new HeadersAdapter(_message.Headers);
        }   

        public IHeaders Headers { get; private set; }
        public string Method => _message.Method.ToString();
        public Uri RequestUri => _message.RequestUri;
    }
}

#endif