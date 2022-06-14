using FluentAssertions;
using Moq;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Way2.Communication;
using Way2.Messaging;

namespace Way2.Communication.Tests;

public class Way2TcpClientTest
{
    [Fact]
    [Trait(nameof(Way2TcpClient), "new()")]
    public void Given_An_Instantiation_When_Pass_Null_Arg_Should_Thrown_Exception()
    {
        FluentActions.Invoking(() => new Way2TcpClient(null)).Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    [Trait(nameof(Way2TcpClient), nameof(IWay2TcpClient.SendAsync))]
    public async Task Given_Client_Sending_A_Message_When_Stream_Has_DataAvailable_Should_Returns_An_Instance()
    {
        //arrange
        var readedSize = 0;
        var message = new byte[] { 0x7D, 0x08, 0x81, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x00, 0xC9 };

        Mock<INetworkStream> mockStream = new();
        Mock<ITcpClient> mockTcpClient = new();

        mockTcpClient
            .Setup(c => c.GetStream())
            .Returns(mockStream.Object)
            .Verifiable();

        mockStream
            .Setup(c => c.ReadAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns((byte[] buffer, int offset, int length, CancellationToken cancellationToken) => Task.Run(() =>
                    {
                        var readSize = length <= (message.Length - readedSize) 
                            ? length 
                            : length - message.Length;

                        Array.Copy(message, readedSize, buffer, 0, readSize);
                        readedSize += readSize;

                        mockStream
                            .Setup(s => s.DataAvailable)
                            .Returns(readedSize != message.Length);

                        return readSize;
                    }));

        mockStream
            .Setup(c => c.WriteAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mockStream
            .Setup(s => s.DataAvailable)
            .Returns(true);

        //act
        IWay2TcpClient client = new Way2TcpClient(mockTcpClient.Object);
        var result = await client.SendAsync(Way2SocketMessage.ReadSerialNumber, CancellationToken.None);
        byte[] payload = result;

        //assert
        result.Should().NotBeNull();
        payload
            .Should()
            .NotBeNull()
            .And
            .BeEquivalentTo(message);
        

        mockTcpClient
            .Verify(c => c.GetStream());

        mockStream
            .Verify(c => c.WriteAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()));

        mockStream
            .Verify(c => c.ReadAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Exactly(3));

        mockStream
            .VerifyGet(s => s.DataAvailable, Times.Exactly(4));
    }

    [Fact]
    [Trait(nameof(Way2TcpClient), nameof(IWay2TcpClient.SendAsync))]
    public async Task Given_Client_Sending_A_Message_When_Stream_Has_No_DataAvailable_Should_Returns_An_Empty_Instance()
    {
        //arrange
        Mock<INetworkStream> mockStream = new();
        Mock<ITcpClient> mockTcpClient = new();

        mockTcpClient
            .Setup(c => c.GetStream())
            .Returns(mockStream.Object)
            .Verifiable();

        mockStream
            .Setup(c => c.WriteAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mockStream
            .Setup(s => s.DataAvailable)
            .Returns(false);

        //act
        IWay2TcpClient client = new Way2TcpClient(mockTcpClient.Object);
        var result = await  client.SendAsync(Way2SocketMessage.ReadSerialNumber, CancellationToken.None);

        //assert
        result.Should().Be(Way2SocketMessage.Empty);

        mockTcpClient
            .Verify(c => c.GetStream());

        mockStream
            .Verify(c => c.WriteAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()));

        mockStream
            .VerifyGet(s => s.DataAvailable);
    }
}
