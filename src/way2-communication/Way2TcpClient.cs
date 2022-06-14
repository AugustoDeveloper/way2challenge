using Way2.Messaging;

namespace Way2.Communication;

public class Way2TcpClient : IWay2TcpClient
{
    private readonly ITcpClient client;

    //TODO: LEARN ABOUT DOCUMENTATION!
    public Way2TcpClient(ITcpClient client)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));

        this.client = client;
    }

    async Task<Way2SocketMessage> IWay2TcpClient.SendAsync(Way2SocketMessage message, CancellationToken cancellation)
    {
        byte[] content = message;
        var networkStream = client.GetStream() ?? throw new InvalidOperationException("The stream is invalid to write.");

        await networkStream.WriteAsync(content, cancellation);

        var offset = 0;
        IEnumerable<byte>? contentReceived = null;

        while(networkStream.DataAvailable)
        {
            var buffer = new byte[ByteMessage.MinimumSize];
            offset = await networkStream.ReadAsync(buffer, offset, buffer.Length, cancellation);

            var newBuffer = new byte[offset];

            Array.Copy(buffer, newBuffer, offset);

            contentReceived = contentReceived is null 
                ? newBuffer 
                : contentReceived.Concat(newBuffer);
        }

        return contentReceived is null
            ? Way2SocketMessage.Empty
            : Way2SocketMessage.Parse(contentReceived.ToArray());
    }
}
