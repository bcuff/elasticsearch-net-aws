using System.Collections;
using System.Collections.Generic;

namespace Elasticsearch.Net.Aws
{
    public interface IHeaders : IEnumerable
    {
        string this[string name] { get; set; }
        IEnumerable<string> GetValues(string name);
    }
}
