using System;
using Amazon;
using Amazon.Runtime;

namespace IntegrationTests
{
    public static class TestConfig
    {
        public static Uri Endpoint { get; set; }

        public static string Region { get; set; }

        public static ImmutableCredentials Credentials { get; set; }
    }
}
