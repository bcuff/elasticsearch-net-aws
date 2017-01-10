#if NET45
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elasticsearch.Net.Aws
{
    public class AppConfigCredentialProvider : ICredentialsProvider
    {
        public AwsCredentials GetCredentials()
        {
            var key = System.Configuration.ConfigurationManager.AppSettings["AWSAccessKey"];
            var secret = System.Configuration.ConfigurationManager.AppSettings["AWSSecretKey"];
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret)) return null;
            return new AwsCredentials
            {
                AccessKey = key,
                SecretKey = secret,
            };
        }
    }
}
#endif
