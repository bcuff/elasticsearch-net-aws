using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace Elasticsearch.Net.Aws
{
    internal class SignableHttpRequestMessage : IHttpRequest
    {
        readonly HttpRequestMessage _request;
        readonly RequestData _requestData;

        public SignableHttpRequestMessage(HttpRequestMessage request, RequestData requestData)
        {
            _request = request;
            _requestData = requestData;
        }

        public Uri Uri => _request.RequestUri;
        public string Method => _request.Method.Method;
        public byte[] Body
        {
            get
            {
                // note - this probably won't work if compression is enabled
                if (_requestData?.PostData == null) return null;
                if (_requestData.PostData.WrittenBytes != null) return _requestData.PostData.WrittenBytes;
                using (var ms = new MemoryStream())
                {
                    _requestData.PostData.Write(ms, _requestData.ConnectionSettings);
                    return ms.ToArray();
                }
            }
        }

        public IEnumerable<string> GetHeaderKeys() => _request.Headers.Select(h => h.Key);

        public IEnumerable<string> GetHeaderValues(string name) => _request.Headers.GetValues(name);

        public void SetHeader(string name, string value)
        {
            _request.Headers.Remove(name);
            _request.Headers.Add(name, value);
        }
    }
}
