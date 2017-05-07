using System.Collections;
using System.Collections.Generic;

namespace Elasticsearch.Net.Aws
{
    public interface IHeaders
    {
        string XAmzDate { get; set; }
        string Authorization { get; set; }
        string XAmzSecurityToken { get; set; }
        IEnumerable<string> GetValues(string name);
        IEnumerable<string> Keys { get; }
    }
}
