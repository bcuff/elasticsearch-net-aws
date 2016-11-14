using System;
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

            Trace.Write(String.Format("AccessKeyId: {0}", credentials.AccessKeyId));
            Assert.False(String.IsNullOrEmpty(credentials.AccessKeyId));

            Trace.Write(String.Format("SecretAccessKey: {0}", credentials.SecretAccessKey));
            Assert.False(String.IsNullOrEmpty(credentials.SecretAccessKey));

            Trace.Write(String.Format("Token: {0}", credentials.Token));
            Assert.False(String.IsNullOrEmpty(credentials.Token));
        }
    }
}
