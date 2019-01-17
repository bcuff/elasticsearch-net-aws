using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Elasticsearch.Net.Aws;
using NUnit.Framework;
using System.Diagnostics;

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

            TestConfig.Endpoint = config["endpoint"];
            TestConfig.Region = config["region"];
        }
    }
}
