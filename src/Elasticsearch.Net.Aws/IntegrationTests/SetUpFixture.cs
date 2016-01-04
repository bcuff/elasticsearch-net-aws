using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Helpers;
using Elasticsearch.Net.Aws;
using NUnit.Framework;

namespace IntegrationTests
{
    [SetUpFixture]
    public class SetUpFixture
    {
        [SetUp]
        public void SetUp()
        {
            var json = File.ReadAllText("TargetConfig.json").Trim();
            dynamic config = Json.Decode(json);
            TestConfig.Endpoint = config.endpoint;
            TestConfig.AwsSettings = new AwsSettings
            {
                AccessKey = config.accessKey,
                SecretKey = config.secretKey,
                Region = config.region,
            };
        }
    }
}
