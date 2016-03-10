using System;
using System.Linq;

namespace Elasticsearch.Net.Aws
{
    /// <summary>
    /// Encapsulates 
    /// </summary>
    public class AwsSettings
    {
        /// <summary>
        /// Gets or sets the region. e.g. us-east-1. Required.
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the AWS access key. Required.
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// Gets or sets the AWS secret key. e.g. wJalrXUtnFEMI/K7MDENG+bPxRfiCYEXAMPLEKEY
        ///  Required.
        /// </summary>
        public string SecretKey { get; set; }

    }
}
