using System;
using System.Threading.Tasks;

namespace Elasticsearch.Net.Aws
{
    internal interface IRequest
    {
        IHeaders Headers { get; }
        string Method { get; }
        Uri RequestUri { get; }
        Task PrepareForSigningAsync();
        byte[] Content { get; }
    }
}
