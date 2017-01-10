using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elasticsearch.Net.Aws
{
    public class CredentialChainProvider : ICredentialsProvider
    {
        public static ICredentialsProvider Default { get; } = new CredentialChainProvider(
#if NET45
            new AppConfigCredentialProvider(),
#endif
            new EnvironmentVariableCredentialsProvider(),
            new InstanceProfileCredentialProvider()
        );

        public CredentialChainProvider(params ICredentialsProvider[] credentialProviders)
            : this((IEnumerable<ICredentialsProvider>)credentialProviders)
        {
        }

        readonly ICredentialsProvider[] _credentialChain;

        public CredentialChainProvider(IEnumerable<ICredentialsProvider> credentialProviders)
        {
            _credentialChain = credentialProviders.ToArray();
        }

        public AwsCredentials GetCredentials()
        {
            foreach (var provider in _credentialChain)
            {
                var creds = provider.GetCredentials();
                if (creds != null) return creds;
            }
            return null;
        }
    }
}
