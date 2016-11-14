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

            public string XAmzDate { get { return _headers["x-amz-date"]; } set { _headers["x-amz-date"] = value; } }
            public string Authorization { get { return _headers[HttpRequestHeader.Authorization]; } set { _headers[HttpRequestHeader.Authorization] = value; } }
            public string XAmzSecurityToken { get { return _headers["x-amz-security-token"]; } set { _headers["x-amz-security-token"] = value; } }
            public IEnumerable<string> GetValues(string name) => _headers.GetValues(name);
            public IEnumerable<string> Keys => _headers.AllKeys;
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