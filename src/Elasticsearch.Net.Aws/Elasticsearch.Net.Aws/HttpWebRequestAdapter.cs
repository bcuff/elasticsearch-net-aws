#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Elasticsearch.Net.Aws
{
    class HttpWebRequestAdapter : IRequest, IHeaders
    {
        readonly HttpWebRequest _request;
        public HttpWebRequestAdapter(HttpWebRequest request)
        {
            _request = request;
        }

        public IHeaders Headers => this;

        public string Method => _request.Method;

        public Uri RequestUri => _request.Address;

        public string XAmzDate
        {
            get => _request.Headers["x-amz-date"];
            set => _request.Headers["x-amz-date"] = value;
        }

        public string Authorization
        {
            get => _request.Headers[HttpRequestHeader.Authorization];
            set => _request.Headers[HttpRequestHeader.Authorization] = value;
        }

        public string XAmzSecurityToken
        {
            get => _request.Headers["x-amz-security-token"];
            set => _request.Headers["x-amz-security-token"] = value;
        }

        public IEnumerable<string> Keys => _request.Headers.AllKeys;

        public IEnumerable<string> GetValues(string name) => _request.Headers.GetValues(name);
    }
}
#endif
