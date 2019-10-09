#if NETSTANDARD
using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;

namespace Elasticsearch.Net.Aws
{
    internal class HttpRequestMessageAdapter : IRequest
    {
        private class HeadersAdapter : IHeaders
        {
            private readonly HttpRequestMessage _message;

            public HeadersAdapter(HttpRequestMessage message)
            {
                _message = message;
            }

            private string ToSingleValue(IEnumerable<string> values) => String.Join(",", values);

            public string XAmzDate { get { return ToSingleValue(_message.Headers.GetValues("x-amz-date")); } set { _message.Headers.TryAddWithoutValidation("x-amz-date", value); } }
            public string Authorization { get { return ToSingleValue(_message.Headers.GetValues("Authorization")); } set { _message.Headers.TryAddWithoutValidation("Authorization", value); } }
            public string XAmzSecurityToken { get { return ToSingleValue(_message.Headers.GetValues("x-amz-security-token")); } set { _message.Headers.TryAddWithoutValidation("x-amz-security-token", value); } }

            public IEnumerable<string> GetValues(string name)
            {
                IEnumerable<string> values = null;
                var found = _message.Headers.TryGetValues(name, out values) || (_message.Content?.Headers.TryGetValues(name, out values) ?? false);
                return values;
            }
            public IEnumerable<string> Keys => _message.Headers.Select(h => h.Key).Concat(_message.Content?.Headers.Select(h => h.Key) ?? Enumerable.Empty<string>());
        }

        private readonly HttpRequestMessage _message;

        public HttpRequestMessageAdapter(HttpRequestMessage message)
        {
            _message = message;
            Headers = new HeadersAdapter(_message);
        }   

        public IHeaders Headers { get; private set; }
        public string Method => _message.Method.ToString();
        public Uri RequestUri => _message.RequestUri;
    }
}
#endif