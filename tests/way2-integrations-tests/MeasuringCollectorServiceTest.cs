using FluentAssertions;
using Moq;
using Xunit;
using Way2.Communication;
using Way2.Messaging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Way2.Integrations.Tests;

public readonly struct Result<T>
{
    public T? Value { get; }

    public bool Failure { get; }

    public static Result<T> Error { get; }

    static Result()
    {
        Error = new(true);
    }

    public Result()
    {
        Value = default;
        Failure = false;
    }

    private Result(bool isFailure) : this()
    {
        Failure = isFailure;
    }

    public Result(T value) : this()
    {
        Value = value;
    }
}

public interface IMeasuringCollectorService
{
    Task<Result<string>> GetSerialNumberAsync(CancellationToken cancellation = default);
    Task<Result<bool>> SetIndexAsync(ushort index, CancellationToken cancellation = default);
    Task<Result<System.Range>> GetStatusRecordAsync(CancellationToken cancellation = default);
    Task<Result<DateTime>> GetDateTimeFromCurrentIndexAsync(CancellationToken cancellation = default);
    Task<Result<float>> GetEnergyValueFromCurrentIndexAsync(CancellationToken cancellation = default);
}

public class MeasuringCollectorService : IMeasuringCollectorService
{
    private readonly IWay2TcpClient client;

    public MeasuringCollectorService(IWay2TcpClient client) 
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        this.client = client;
    }

    async Task<Result<string>> IMeasuringCollectorService.GetSerialNumberAsync(CancellationToken cancellation)
    {
        var response = await client.SendAsync(Way2SocketMessage.ReadSerialNumber, cancellation);

        if (response.IsError)
        {
            return Result<string>.Error;
        }

        return new(response);
    }

    async Task<Result<bool>> IMeasuringCollectorService.SetIndexAsync(ushort index, CancellationToken cancellation)
    {
        var response = await client.SendAsync(Way2SocketMessage.SetRecordIndex(index), cancellation);

        if (response.IsError)
        {
            return Result<bool>.Error;
        }

        return new(response);
    }

    async Task<Result<DateTime>> IMeasuringCollectorService.GetDateTimeFromCurrentIndexAsync(CancellationToken cancellation)
    {
        var response = await client.SendAsync(Way2SocketMessage.ReadCurrentRecordDateTime, cancellation);

        if (response.IsError)
        {
            return Result<DateTime>.Error;
        }

        return new(response);
    }

    async Task<Result<System.Range>> IMeasuringCollectorService.GetStatusRecordAsync(CancellationToken cancellation)
    {
        var response = await client.SendAsync(Way2SocketMessage.ReadStatusRecord, cancellation);

        if (response.IsError)
        {
            return Result<System.Range>.Error;
        }

        return new(response);
    }

    async Task<Result<float>> IMeasuringCollectorService.GetEnergyValueFromCurrentIndexAsync(CancellationToken cancellation)
    {
        var response = await client.SendAsync(Way2SocketMessage.ReadCurrentEnergyValue, cancellation);

        if (response.IsError)
        {
            return Result<float>.Error;
        }

        return new(response);
    }
}

public class MeasuringCollectorServiceTest
{
    [Fact]
    [Trait(nameof(MeasuringCollectorService), "new()")]
    public void Given_Try_Of_Create_An_Instance_When_Pass_Null_Arg_Should_Thrown_ArgumentNullException()
    {
        //arrange
        Action createServiceWithNullClient = () => new MeasuringCollectorService(null);

        //act

        //assert
        createServiceWithNullClient.Should().ThrowExactly<ArgumentNullException>().WithParameterName("client");
    }

    [Fact]
    [Trait(nameof(MeasuringCollectorService), nameof(IMeasuringCollectorService.GetSerialNumberAsync))]
    public async Task Given_Call_GetSerialNumber_When_TcpClient_Throws_Exception_Should_Throw_Up()
    {
        //arrange
        Mock<IWay2TcpClient> mockClient = new();
        mockClient
            .Setup(c => c.SendAsync(Way2SocketMessage.ReadSerialNumber, default))
            .ThrowsAsync(new Exception("This is mocked exception"))
            .Verifiable();

        IMeasuringCollectorService service = new MeasuringCollectorService(mockClient.Object);

        //act
        Func<Task> getSerialNumberAsync = () => service.GetSerialNumberAsync(default);

        //assert
        await getSerialNumberAsync.Should().ThrowExactlyAsync<Exception>().WithMessage("This is mocked exception");

        mockClient.Verify(c => c.SendAsync(Way2SocketMessage.ReadSerialNumber, default));
    }

    [Fact]
    [Trait(nameof(MeasuringCollectorService), nameof(IMeasuringCollectorService.GetSerialNumberAsync))]
    public async Task Given_Call_GetSerialNumber_When_TcpClient_Get_Result_Should_Returns_ValidSerialNumber_Text()
    {
        //arrange
        var bytes = new byte[] { 0x7D, 0x08, 0x81, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x00, 0xC9 };
        var serialNumberWay2SocketMessage = Way2SocketMessage.Parse(bytes);
        Mock<IWay2TcpClient> mockClient = new();

        mockClient
            .Setup(c => c.SendAsync(Way2SocketMessage.ReadSerialNumber, default))
            .ReturnsAsync(serialNumberWay2SocketMessage)
            .Verifiable();

        IMeasuringCollectorService service = new MeasuringCollectorService(mockClient.Object);

        //act
        var result = await service.GetSerialNumberAsync(default);

        //assert
        result.Should().NotBeNull();
        result.Failure.Should().BeFalse();
        result.Value.Should().Be("ABCDEFG\0");
    }

    [Fact]
    [Trait(nameof(MeasuringCollectorService), nameof(IMeasuringCollectorService.SetIndexAsync))]
    public async Task Given_Call_SetIndexAsync_When_TcpClient_Get_Result_Should_Returns_Valid_Bool()
    {
        //arrange
        var bytes = new byte[] { 0x7D, 0x01, 0x83, 0x00, 0x82 };
        var serialNumberWay2SocketMessage = Way2SocketMessage.Parse(bytes);
        Mock<IWay2TcpClient> mockClient = new();

        mockClient
            .Setup(c => c.SendAsync(It.IsAny<Way2SocketMessage>(), default))
            .ReturnsAsync(serialNumberWay2SocketMessage)
            .Verifiable();

        IMeasuringCollectorService service = new MeasuringCollectorService(mockClient.Object);

        //act
        var result = await service.SetIndexAsync(380, default);

        //assert
        result.Should().NotBeNull();
        result.Failure.Should().BeFalse();
        result.Value.Should().BeTrue();
    }
}
