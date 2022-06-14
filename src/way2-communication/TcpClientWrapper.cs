using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Way2.Communication;

internal class TcpClientWrapper : ITcpClient
{
    private readonly TcpClient client;
    private INetworkStream networkStream;

    internal TcpClientWrapper()
    {
        this.client = new TcpClient();
    }

    INetworkStream? ITcpClient.GetStream()
    {
        if (this.networkStream is null && this.client.Connected)
        {
            this.networkStream = new NetworkStreamWrapper(this.client.GetStream());
        }

        return this.networkStream;
    }

    void ITcpClient.Connect(string hostName, int port)
    {
        this.client.Connect(hostName, port);
    }

    private class NetworkStreamWrapper : INetworkStream
    {
        private readonly NetworkStream stream;

        public NetworkStreamWrapper(NetworkStream stream)
        {
            this.stream = stream;
        }

        bool INetworkStream.DataAvailable => stream.DataAvailable;

        Task INetworkStream.WriteAsync(byte[] content, CancellationToken cancellation)
            => stream.WriteAsync(content, 0, content.Length, cancellation);

        Task<int> INetworkStream.ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellation)
            => stream.ReadAsync(buffer, offset, count, cancellation);
    }
}
