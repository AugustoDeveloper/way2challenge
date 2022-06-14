using System.Threading;
using System.Threading.Tasks;

namespace Way2.Communication;

public interface INetworkStream
{
    bool DataAvailable { get; }
    Task WriteAsync(byte[] content, CancellationToken cancellation);
    Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellation);
}
