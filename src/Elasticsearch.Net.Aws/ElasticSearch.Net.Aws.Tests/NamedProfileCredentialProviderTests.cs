using Elasticsearch.Net.Aws;
using NUnit.Framework;

namespace ElasticSearch.Net.Aws.Tests
{
    [TestFixture]
    public class NamedProfileCredentialProviderTests
    {
        [TestCase]
        [Ignore("Requires setting up an aws profile with the expected values")]
        public void GetCredentials_should_return_expected_values()
        {
            var credentialProvider = new NamedProfileCredentialProvider();
            var credentials = credentialProvider.GetCredentials();

            Assert.NotNull(credentials);
            Assert.AreEqual("TestKey", credentials.AccessKey);
            Assert.AreEqual("TestSecret", credentials.SecretKey);
            Assert.AreEqual("TestToken", credentials.Token);
        }
    }
}
