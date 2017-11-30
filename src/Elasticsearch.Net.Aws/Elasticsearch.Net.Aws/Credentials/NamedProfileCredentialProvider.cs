using System.Security;
using Amazon.Runtime.CredentialManagement;
using static System.Environment;
using static Amazon.Runtime.CredentialManagement.SharedCredentialsFile;

namespace Elasticsearch.Net.Aws
{
    public class NamedProfileCredentialProvider : ICredentialsProvider
    {
        const string AwsProfileEnvironmentVariable = "AWS_PROFILE";

        readonly string _profileName;

        public NamedProfileCredentialProvider(string profileName = null)
        {
            _profileName = profileName;
        }

        public AwsCredentials GetCredentials()
        {
            string profileName;
            try
            {
                profileName = _profileName ??
                              GetEnvironmentVariable(AwsProfileEnvironmentVariable) ??
                              DefaultProfileName;
            }
            catch (SecurityException)
            {
                return null;
            }

            var chain = new CredentialProfileStoreChain();
            if (!chain.TryGetAWSCredentials(profileName, out var credentials))
            {
                return null;
            }

            var immutableCredentials = credentials.GetCredentials();
            if (immutableCredentials == null) { return null; }

            return new AwsCredentials
            {
                AccessKey = immutableCredentials.AccessKey,
                SecretKey = immutableCredentials.SecretKey,
                Token = immutableCredentials.Token
            };
        }
    }
}