using System.Threading;
using System.Threading.Tasks;
using Way2.Messaging;

namespace Way2.Communication;

public interface IWay2TcpClient
{
    Task<Way2SocketMessage> SendAsync(Way2SocketMessage message, CancellationToken cancellation);
}
