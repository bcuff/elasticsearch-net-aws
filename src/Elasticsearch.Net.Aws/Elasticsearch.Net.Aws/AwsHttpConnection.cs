using System;
using System.IO;
using System.Net.Http;

namespace Elasticsearch.Net.Aws
{
    /// <summary>
    /// Encapsulates an IConnection that works with AWS's Elasticsearch service.
    /// </summary>
    public class AwsHttpConnection : HttpConnection
    {
        private readonly ICredentialsProvider _credentialsProvider;
        private readonly string _region;

        /// <summary>
        /// Initializes a new instance of the AwsHttpConnection class with the specified AccessKey, SecretKey and Token.
        /// </summary>
        /// <param name="awsSettings">AWS specific settings required for signing requests.</param>
        [Obsolete("Use AwsHttpConnection(string region, ICredentialsProvider credentialsProvider)")]
        public AwsHttpConnection(AwsSettings awsSettings)
        {
            if (awsSettings == null) throw new ArgumentNullException(nameof(awsSettings));
            if (string.IsNullOrWhiteSpace(awsSettings.Region)) throw new ArgumentException("awsSettings.Region is invalid.", nameof(awsSettings));
            _region = awsSettings.Region.ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(awsSettings.AccessKey) && !string.IsNullOrWhiteSpace(awsSettings.SecretKey))
            {
                _credentialsProvider = new StaticCredentialsProvider(awsSettings);
            }
            else
            {
                _credentialsProvider = CredentialChainProvider.Default;
            }
        }

        /// <summary>
        /// Initializes a new instance of the AwsHttpConnection class with credentials from the Instance Profile service
        /// </summary>
        /// <param name="region">AWS region</param>
        public AwsHttpConnection(string region)
            : this(region, CredentialChainProvider.Default)
        {
        }


        /// <summary>
        /// Initializes a new instance of the AwsHttpConnection class with credentials from the Instance Profile service
        /// </summary>
        /// <param name="region">AWS region</param>
        /// <param name="credentialsProvider">The credentials provider.</param>
        public AwsHttpConnection(string region, ICredentialsProvider credentialsProvider)
        {
            if (region == null) throw new ArgumentNullException(nameof(region));
            if (string.IsNullOrWhiteSpace(region)) throw new ArgumentException("region is invalid", nameof(region));
            if (credentialsProvider == null) throw new ArgumentNullException(nameof(credentialsProvider));
            _region = region.ToLowerInvariant();
            _credentialsProvider = credentialsProvider;
        }

#if NET45
        protected override System.Net.HttpWebRequest CreateHttpWebRequest(RequestData requestData)
        {
            var request = base.CreateHttpWebRequest(requestData);
            SignRequest(new HttpWebRequestAdapter(request), requestData);
            return request;
        }
#else
        protected override HttpRequestMessage CreateHttpRequestMessage(RequestData requestData)
        {
            var request = base.CreateHttpRequestMessage(requestData);
            SignRequest(new HttpRequestMessageAdapter(request), requestData);
            return request;
        }
#endif
        private void SignRequest(IRequest request, RequestData requestData)
        {
            byte[] data = null;
            if (requestData.PostData != null)
            {
                data = requestData.PostData.WrittenBytes;
                if (data == null)
                {
                    using (var ms = new MemoryStream())
                    {
                        requestData.PostData.Write(ms, requestData.ConnectionSettings);
                        data = ms.ToArray();
                    }
                }
            }
            var credentials = _credentialsProvider.GetCredentials();
            if (credentials == null)
            {
                throw new Exception("Unable to retrieve credentials required to sign the request.");
            }
            SignV4Util.SignRequest(request, data, credentials, _region, "es");
        }
    }
}
