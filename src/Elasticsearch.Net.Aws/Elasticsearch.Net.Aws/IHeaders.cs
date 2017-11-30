﻿using System.Collections.Generic;

namespace Elasticsearch.Net.Aws
{
    internal interface IHeaders
    {
        string XAmzDate { get; set; }
        string Authorization { get; set; }
        string XAmzSecurityToken { get; set; }
        IEnumerable<string> GetValues(string name);
        IEnumerable<string> Keys { get; }
    }
}
