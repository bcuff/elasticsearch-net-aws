using System;
using System.Linq;

namespace Elasticsearch.Net.Aws
{
    /// <summary>
    /// Encapsulates AWS settings needed to talk to elasticsearch
    /// </summary>
    public class AwsSettings : AwsCredentials
    {
        /// <summary>
        /// Gets or sets the region. e.g. us-east-1. Required.
        /// </summary>
        public string Region { get; set; }
    }
}
