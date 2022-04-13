#if NETSTANDARD
using Amazon;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Elasticsearch.Net.Aws
{
    class SigningHttpMessageHandler : HttpClientHandler
    {
        readonly AWSCredentials _credentials;
        readonly RegionEndpoint _region;
        readonly HttpMessageHandler _innerHandler;
        bool _innerHandlerDisposed;

        public SigningHttpMessageHandler(AWSCredentials credentials, RegionEndpoint region, HttpMessageHandler innerHandler)
        {
            _credentials = credentials;
            _region = region;
            _innerHandler = innerHandler;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var credentials = await _credentials.GetCredentialsAsync().ConfigureAwait(false);
            await SignV4Util.SignRequestAsync(new HttpRequestMessageAdapter(request), credentials, _region.SystemName, "es").ConfigureAwait(false);
            return await _innerHandler.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_innerHandlerDisposed)
            {
                if (disposing)
                {
                    _innerHandler.Dispose();
                }
                _innerHandlerDisposed = true;
            }
            base.Dispose();
        }
    }
}
#endif
