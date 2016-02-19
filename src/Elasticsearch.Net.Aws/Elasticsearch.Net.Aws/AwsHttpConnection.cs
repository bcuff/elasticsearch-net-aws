using System;
using System.IO;
using System.Net;

namespace Elasticsearch.Net.Aws
{
    /// <summary>
    /// Encapsulates an IConnection that works with AWS's Elasticsearch service.
    /// </summary>
    public class AwsHttpConnection : HttpConnection
    {
        private string _accessKey;
        private string _secretKey;
        private string _token;
        private string _region;
        private AuthType _authType;

        /// <summary>
        /// Initializes a new instance of the AwsHttpConnection class with the specified AccessKey, SecretKey and Token.
        /// </summary>
        /// <param name="awsSettings">AWS specific settings required for signing requests.</param>
        public AwsHttpConnection(AwsSettings awsSettings)
        {
            if (awsSettings == null) throw new ArgumentNullException("awsSettings");
            if (string.IsNullOrWhiteSpace(awsSettings.AccessKey)) throw new ArgumentException("awsSettings.AccessKey is invalid.", "awsSettings");
            if (string.IsNullOrWhiteSpace(awsSettings.SecretKey)) throw new ArgumentException("awsSettings.SecretKey is invalid.", "awsSettings");
            if (string.IsNullOrWhiteSpace(awsSettings.Region)) throw new ArgumentException("awsSettings.Region is invalid.", "awsSettings");
            _accessKey = awsSettings.AccessKey;
            _secretKey = awsSettings.SecretKey;
            _token = awsSettings.Token;
            _region = awsSettings.Region.ToLowerInvariant();
            _authType = AuthType.AccessKey;
        }

        /// <summary>
        /// Initializes a new instance of the AwsHttpConnection class with credentials from the Instance Profile service
        /// </summary>
        /// <param name="settings">The NEST/Elasticsearch.Net settings.</param>
        /// <param name="region">AWS region</param>
        public AwsHttpConnection(string region)
        {
            if (string.IsNullOrWhiteSpace(region)) throw new ArgumentException("region is invalid.", "region");
            _region = region.ToLowerInvariant();
            _authType = AuthType.InstanceProfile;
        }

        protected override HttpWebRequest CreateHttpWebRequest(RequestData requestData)
        {
            if (_authType == AuthType.InstanceProfile)
            {
                RefreshCredentials();
            }
            var request = base.CreateHttpWebRequest(requestData);
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
            SignV4Util.SignRequest(request, data, _accessKey, _secretKey, _token, _region, "es");
            return request;
        }

        private void RefreshCredentials()
        {
            var credentials = InstanceProfileService.GetCredentials();
            if (credentials == null)
                throw new Exception("Unable to retrieve session credentials from instance profile service");

            _accessKey = credentials.AccessKeyId;
            _secretKey = credentials.SecretAccessKey;
            _token = credentials.Token;
        }
    }
}
