using System;
using System.Threading;
using System.Threading.Tasks;

namespace Way2.Communication;

public class Way2TcpClientFactory : IWay2TcpClientFactory
{
    IWay2TcpClient IWay2TcpClientFactory.CreateClient(string hostName, int port)
    {
        if (string.IsNullOrWhiteSpace(hostName))
        {
            throw new ArgumentNullException(nameof(hostName));
        }

        if (port < 1)
        {
            throw new ArgumentException($"The port {port} is invalid.", nameof(port));
        }

        ITcpClient tcpClient = new TcpClientWrapper();
        tcpClient.Connect(hostName, port);
        return new Way2TcpClient(tcpClient);
    }
}
