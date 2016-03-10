using System;
using System.Configuration;
using System.IO;
using System.Net;

namespace Elasticsearch.Net.Aws
{
    /// <summary>
    /// Encapsulates an IConnection that works with AWS's Elasticsearch service.
    /// </summary>
    public class AwsHttpConnection : HttpConnection
    {
        private static string GetAccessKey(AwsSettings awsSettings)
        {
            var key = awsSettings.AccessKey;
            if (!string.IsNullOrWhiteSpace(key)) return key;
            key = ConfigurationManager.AppSettings["AWSAccessKey"];
            if (!string.IsNullOrWhiteSpace(key)) return key;
            return Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
        }

        private static string GetSecretKey(AwsSettings awsSettings)
        {
            var key = awsSettings.SecretKey;
            if (!string.IsNullOrWhiteSpace(key)) return key;
            key = ConfigurationManager.AppSettings["AWSSecretKey"];
            if (!string.IsNullOrWhiteSpace(key)) return key;
            return Environment.GetEnvironmentVariable("AWS_SECRET_KEY");
        }

        private string _accessKey;
        private string _secretKey;
        private string _token;
        private readonly string _region;
        private readonly AuthType _authType;

        /// <summary>
        /// Initializes a new instance of the AwsHttpConnection class with the specified AccessKey, SecretKey and Token.
        /// </summary>
        /// <param name="awsSettings">AWS specific settings required for signing requests.</param>
        public AwsHttpConnection(AwsSettings awsSettings)
        {
            if (awsSettings == null) throw new ArgumentNullException("awsSettings");
            if (string.IsNullOrWhiteSpace(awsSettings.Region)) throw new ArgumentException("awsSettings.Region is invalid.", "awsSettings");
            _region = awsSettings.Region.ToLowerInvariant();
            var key = GetAccessKey(awsSettings);
            var secret = GetSecretKey(awsSettings);
            if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(secret))
            {
                _accessKey = key;
                _secretKey = secret;
                _authType = AuthType.AccessKey;
            }
            else
            {
                _authType = AuthType.InstanceProfile;
            }
        }

        /// <summary>
        /// Initializes a new instance of the AwsHttpConnection class with credentials from the Instance Profile service
        /// </summary>
        /// <param name="region">AWS region</param>
        public AwsHttpConnection(string region)
            : this(new AwsSettings { Region = region })
        {
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
