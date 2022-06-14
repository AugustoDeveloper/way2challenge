namespace Way2.Communication;

public interface IWay2TcpClientFactory
{
    IWay2TcpClient CreateClient(string host, int port);
}
