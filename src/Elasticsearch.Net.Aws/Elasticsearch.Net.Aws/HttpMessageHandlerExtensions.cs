#if NETSTANDARD
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Elasticsearch.Net.Aws
{
    internal static class HttpMessageHandlerExtensions
    {
        static readonly Func<HttpMessageHandler, HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendFunc = GetSendAsync();

        static Func<HttpMessageHandler, HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> GetSendAsync()
        {
            var info = typeof(HttpMessageHandler).GetTypeInfo();
            var q = from method in info.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    where method.Name == "SendAsync"
                    let p = method.GetParameters()
                    where p.Length == 2 && p[0].ParameterType == typeof(HttpRequestMessage) && p[1].ParameterType == typeof(CancellationToken)
                    select method;
            var sendAsyncMethod = q.FirstOrDefault();
            if (sendAsyncMethod == null) throw new InvalidOperationException($"Unable to find SendAsync method on handler of type {nameof(HttpMessageHandler)}");
            var handler = Expression.Parameter(typeof(HttpMessageHandler), "handler");
            var request = Expression.Parameter(typeof(HttpRequestMessage), "request");
            var cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
            return Expression.Lambda<Func<HttpMessageHandler, HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>>(
                Expression.Call(handler, sendAsyncMethod, request, cancellationToken),
                "WrappedSendAsync",
                new[] { handler, request, cancellationToken }
            ).Compile();
        }

        public static Task<HttpResponseMessage> SendAsync(this HttpMessageHandler handler, HttpRequestMessage request, CancellationToken cancellationToken = default(CancellationToken))
        {
            // this reflection is necessary to bypass the "protected internal" access modifiers
            // on HttpMessageHandler.SendAsync
            // unfortunately I haven't found a good way around doing this.
            return _sendFunc(handler, request, cancellationToken);
        }
    }
}
#endif
