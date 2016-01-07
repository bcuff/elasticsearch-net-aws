using System.Diagnostics;
using Elasticsearch.Net.Aws;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class InstanceProfileTests
    {
        [Test]
        public void GetCredentials_should_return_valid_credentials()
        {
            var credentials = InstanceProfileService.GetCredentials();

            Trace.Write($"AccessKeyId: {credentials.AccessKeyId}");
            Assert.IsNotNullOrEmpty(credentials.AccessKeyId);

            Trace.Write($"SecretAccessKey: {credentials.SecretAccessKey}");
            Assert.IsNotNullOrEmpty(credentials.SecretAccessKey);

            Trace.Write($"Token: {credentials.Token}");
            Assert.IsNotNullOrEmpty(credentials.Token);
        }
    }
}
