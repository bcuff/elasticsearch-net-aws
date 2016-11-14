using System;

namespace Elasticsearch.Net.Aws
{
    public interface IRequest
    {
        IHeaders Headers { get; }
        string Method { get; }
        Uri RequestUri { get; }
    }
}
