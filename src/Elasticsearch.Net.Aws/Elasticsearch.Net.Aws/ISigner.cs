namespace Elasticsearch.Net.Aws
{
    /// <summary>
    /// Interface represent an abstraction of request signer.
    /// </summary>
    public interface ISigner
    {
        void SignRequest(IRequest request, byte[] data);
    }
}
