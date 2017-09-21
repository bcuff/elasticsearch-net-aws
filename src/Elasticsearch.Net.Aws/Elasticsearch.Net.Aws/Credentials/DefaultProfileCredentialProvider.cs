using System.Linq;
using Amazon.Runtime.CredentialManagement;

namespace Elasticsearch.Net.Aws
{
    public class DefaultProfileCredentialProvider : ICredentialsProvider
    {
        public AwsCredentials GetCredentials()
        {
            var chain = new CredentialProfileStoreChain();
            var profiles = chain.ListProfiles();
            CredentialProfile profile = profiles.FirstOrDefault();

            if (profile == null) return null;

            var credentials = profile.GetAWSCredentials(chain).GetCredentials();
            var credential = new AwsCredentials { AccessKey = credentials.AccessKey, SecretKey = credentials.SecretKey, Token = credentials.Token };

            return new AwsCredentials
            {
                AccessKey = credential.AccessKey,
                SecretKey = credential.SecretKey,
                Token = credential.Token
            };
        }
    }
}