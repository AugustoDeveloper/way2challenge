namespace Way2.Communication;

public interface ITcpClient
{
    INetworkStream? GetStream();
    void Connect(string hostName, int port);
}
