using System;
using System.Linq;
using Elasticsearch.Net.Aws;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class StringUtilTests
    {
        [TestCase("  leading whitespace", "leading whitespace")]
        [TestCase("trailing whitespace  ", "trailing whitespace")]
        [TestCase("  whitespace\t\r  \tin the middle \r\n", "whitespace\tin the middle")]
        public void Trimall_should_follow_rfc_2616(string original, string expected)
        {
            Assert.AreEqual(original.Trimall(), expected.Trimall());
        }
    }
}
