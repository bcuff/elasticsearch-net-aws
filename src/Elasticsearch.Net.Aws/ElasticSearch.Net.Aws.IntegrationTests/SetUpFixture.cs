using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
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
            var config = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            TestConfig.Endpoint = config["endpoint"];
            TestConfig.AwsSettings = new AwsSettings
            {
                AccessKey = config["accessKey"],
                SecretKey = config["secretKey"],
                Region = config["region"],
            };
        }
    }
}
