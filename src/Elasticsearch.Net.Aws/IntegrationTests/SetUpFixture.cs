using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
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
            var serializer = new JavaScriptSerializer();
            var config =serializer.Deserialize<Dictionary<string, string>>(json);

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
