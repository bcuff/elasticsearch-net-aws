using System;
using System.Collections.Generic;
using System.IO;
using Amazon;
using Amazon.Runtime;
using Newtonsoft.Json;
using NUnit.Framework;

namespace IntegrationTests
{
    [SetUpFixture]
    public class SetUpFixture
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            var json = File.ReadAllText("TargetConfig.json").Trim();
            var config = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            TestConfig.Endpoint = new Uri(config["endpoint"]);
            TestConfig.Region = config["secretKey"];
            TestConfig.Credentials = new BasicAWSCredentials(config["accessKey"], config["secretKey"]).GetCredentials();
        }
    }
}
