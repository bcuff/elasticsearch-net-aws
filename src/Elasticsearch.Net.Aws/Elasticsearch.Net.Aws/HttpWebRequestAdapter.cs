#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Elasticsearch.Net.Aws
{
    class HttpWebRequestAdapter : IRequest, IHeaders
    {
        readonly HttpWebRequest _request;
        readonly RequestData _requestData;
        byte[] _content;
        public HttpWebRequestAdapter(HttpWebRequest request, RequestData requestData)
        {
            _request = request;
            _requestData = requestData;
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

        public byte[] Content => _content ?? throw new InvalidOperationException("You must first call PrepareForSigningAsync");

        public Task PrepareForSigningAsync()
        {
            if (_requestData.PostData == null)
            {
                _content = Array.Empty<byte>();
                return Task.CompletedTask;
            }
            var data = _requestData.PostData.WrittenBytes;
            if (data == null)
            {
                using (var ms = new MemoryStream())
                {
                    _requestData.PostData.Write(ms, _requestData.ConnectionSettings);
                    data = ms.ToArray();
                }
            }
            _content = data;
            return Task.CompletedTask;
        }
    }
}
#endif
