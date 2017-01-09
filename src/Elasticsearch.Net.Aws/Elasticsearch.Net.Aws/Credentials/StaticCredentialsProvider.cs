using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elasticsearch.Net.Aws
{
    public class StaticCredentialsProvider : ICredentialsProvider
    {
        readonly AwsCredentials _credentials;

        public StaticCredentialsProvider(AwsCredentials credentials)
        {
            if (credentials == null) throw new ArgumentNullException(nameof(credentials));
            if (string.IsNullOrEmpty(credentials.AccessKey)) throw new ArgumentException("AccessKey is required.", nameof(credentials));
            if (string.IsNullOrEmpty(credentials.SecretKey)) throw new ArgumentException("SecretKey is required.", nameof(credentials));
            _credentials = credentials;
        }

        public AwsCredentials GetCredentials() => _credentials;
    }
}
