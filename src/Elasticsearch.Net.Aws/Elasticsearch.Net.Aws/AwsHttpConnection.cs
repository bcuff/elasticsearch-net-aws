using System;
using System.IO;
using System.Net.Http;
using Elasticsearch.Net;

namespace Elasticsearch.Net.Aws
{
    /// <summary>
    /// Encapsulates an IConnection that works with AWS's Elasticsearch service.
    /// </summary>
    public class AwsHttpConnection : HttpConnection
    {
        private readonly ISigner _signer;

        /// <summary>
        /// Initializes a new instance of the AwsHttpConnection class with the specified AccessKey, SecretKey and Token.
        /// </summary>
        /// <param name="awsSettings">AWS specific settings required for signing requests.</param>
        [Obsolete("Use AwsHttpConnection(ISigner signer)")]
        public AwsHttpConnection(AwsSettings awsSettings)
            : this(
                  awsSettings.Region, 
                  awsSettings.HasCredentials() ? new StaticCredentialsProvider(awsSettings) : CredentialChainProvider.Default
            )
        {
        }

        /// <summary>
        /// Initializes a new instance of the AwsHttpConnection class with credentials from the Instance Profile service
        /// </summary>
        /// <param name="region">AWS region</param>
        [Obsolete("Use AwsHttpConnection(ISigner signer)")]
        public AwsHttpConnection(string region)
            : this(region, CredentialChainProvider.Default)
        {
        }


        /// <summary>
        /// Initializes a new instance of the AwsHttpConnection class with credentials from the Instance Profile service
        /// </summary>
        /// <param name="region">AWS region</param>
        /// <param name="credentialsProvider">The credentials provider.</param>
        [Obsolete("Use AwsHttpConnection(ISigner signer)")]
        public AwsHttpConnection(string region, ICredentialsProvider credentialsProvider)
            : this(new AwsV4Signer(region, "es", credentialsProvider))
        {
        }

        /// <summary>
        /// Initializes a new instance of the AwsHttpConnection class with signer
        /// </summary>
        /// <param name="signer">The request signer.</param>
        public AwsHttpConnection(ISigner signer)
        {
            _signer = signer ?? throw new ArgumentNullException(nameof(signer));
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
            _signer.SignRequest(request, data);
        }
    }
}
