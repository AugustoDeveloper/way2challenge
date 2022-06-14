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

public class Way2TcpClientFactoryTest
{
    [Fact]
    [Trait(nameof(Way2TcpClientFactory), nameof(IWay2TcpClientFactory.CreateClient))]
    public async Task Given_An_Instance_When_Pass_Invalid_Args_To_CreateAsync_Should_Thrown_Exceptions()
    {
        IWay2TcpClientFactory factory = new Way2TcpClientFactory();
        FluentActions
            .Invoking(() => factory.CreateClient(null, 1010))
            .Should()
            .ThrowExactly<ArgumentNullException>()
            .WithParameterName("hostName");

        FluentActions
            .Invoking(() => factory.CreateClient("localhost", -1))
            .Should()
            .ThrowExactly<ArgumentException>()
            .WithParameterName("port");
    }
}
