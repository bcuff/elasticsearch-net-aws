using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elasticsearch.Net.Aws
{
    public class InstanceProfileCredentialProvider : ICredentialsProvider
    {
        public AwsCredentials GetCredentials()
        {
            var credentials = InstanceProfileService.GetCredentials();
            if (credentials != null)
            {
                return new AwsCredentials
                {
                    AccessKey = credentials.AccessKeyId,
                    SecretKey = credentials.SecretAccessKey,
                    Token = credentials.Token,
                };
            }
            return null;
        }
    }
}
