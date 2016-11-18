using System;

namespace Elasticsearch.Net.Aws
{
    internal interface IRequest
    {
        IHeaders Headers { get; }
        string Method { get; }
        Uri RequestUri { get; }
    }
}
