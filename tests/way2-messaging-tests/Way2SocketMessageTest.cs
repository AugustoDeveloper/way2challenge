using System.Collections.Generic;
using System.Globalization;
using Xunit;
using Moq;
using FluentAssertions;
using System;
using System.Linq;
using Way2.Messaging;

namespace Way2.Messaging.Tests;

public class Way2SocketMessageTest
{
    public static IEnumerable<object[]> GetData()
        => new[]
        {
            new object[] { Way2SocketMessage.ReadSerialNumber, Way2SocketMessage.ReadSerialNumber, FunctionCodes.ReadSerialNumber },
            new object[] { Way2SocketMessage.ReadStatusRecord, Way2SocketMessage.ReadStatusRecord, FunctionCodes.ReadStatusRecord },
            new object[] { Way2SocketMessage.ReadCurrentRecordDateTime, Way2SocketMessage.ReadCurrentRecordDateTime, FunctionCodes.ReadCurrentRecordDateTime },
            new object[] { Way2SocketMessage.ReadCurrentEnergyValue, Way2SocketMessage.ReadCurrentEnergyValue, FunctionCodes.ReadCurrentEnergyValue },
            new object[] { Way2SocketMessage.Empty, Way2SocketMessage.Empty, FunctionCodes.Empty },
            new object[] { Way2SocketMessage.Error, Way2SocketMessage.Error, FunctionCodes.Error },
        };

    [Theory]
    [Trait(nameof(Way2SocketMessage), nameof(Way2SocketMessage.ReadSerialNumber))]
    [MemberData(nameof(GetData))]
    public void When_Get_Message_To_Read_Should_Return_Valid_Way2SocketMessage(
            Way2SocketMessage message, 
            Way2SocketMessage expectedMessage,
            byte functionCode)
    {
       //arrange

        //act
        byte[] payload = message;

        //assert
        message.Should().Be(expectedMessage);
        payload.Should().NotBeNull();
        payload.Should().NotBeEmpty();
        payload.Should().HaveCount(4);
        payload.Should().BeEquivalentTo(new byte[4] { Way2SocketMessage.FrameHeader, 0x00, functionCode, functionCode});

    }

    [Theory]
    [Trait(nameof(Way2SocketMessage), nameof(Way2SocketMessage.SetRecordIndex))]
    [InlineData(380, 0x02, 0x01, 0x7C, 0x7C)]
    [InlineData(300, 0x02, 0x01, 0x2C, 0x2C)]
    [InlineData(600, 0x02, 0x02, 0x58, 0x5B)]
    [InlineData(ushort.MaxValue, 0x02, 0xFF, 0xFF, 0x01)]
    [InlineData(ushort.MinValue, 0x02, 0x00, 0x00, 0x01)]
    public void When_Call_SetRecordIndex_With_Numbers_Should_Returns_Valid_Instance(
        ushort index,
        byte length,
        byte firstDataValue,
        byte secondDataValue,
        byte checksum
    )
    {
        //arrange

        //act
        byte[] payload = Way2SocketMessage.SetRecordIndex(index);

        //assert
        payload.Should().NotBeNull();
        payload.Should().NotBeEmpty();
        payload.Should().HaveCount(6);
        payload.Should().BeEquivalentTo(new byte[6] { Way2SocketMessage.FrameHeader, length, FunctionCodes.SetRecordIndex, firstDataValue, secondDataValue, checksum});
    }

    [Fact]
    [Trait(nameof(Way2SocketMessage), nameof(Way2SocketMessage.Parse))]
    public void When_Parse_Receive_SerialNumber_Byte_Array_Message_Should_Returns_Valid_Instace()
    {
        //arrange
        //act
        var message = Way2SocketMessage.Parse(new byte[] { 0x7D, 0x08, 0x81, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x00, 0xC9 });
        string text = message;
        byte[] payload = message;

        //assert
        message.Should().NotBe(Way2SocketMessage.Error);
        text.Should().Be("ABCDEFG\0");
        
        payload.Should().NotBeNull();
        payload.Should().NotBeEmpty();
        payload.Should().HaveCount(12);
        payload.Should().BeEquivalentTo(new byte[] { 0x7D, 0x08, 0x81, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x00, 0xC9 });
    }

    [Fact]
    [Trait(nameof(Way2SocketMessage), nameof(Way2SocketMessage.Parse))]
    public void When_Parse_Receive_Staus_Record_Byte_Array_Message_Should_Returns_Valid_Instace()
    {
        //arrange
        //act
        var message = Way2SocketMessage.Parse(new byte[] { 0x7D, 0x04, 0x82, 0x01, 0x2C, 0x02, 0x58, 0xF1 });
        System.Range range = message;

        //assert
        message.Should().NotBe(Way2SocketMessage.Error);
        range.Start.Value.Should().Be(300);
        range.End.Value.Should().Be(600);
    }

    [Fact]
    [Trait(nameof(Way2SocketMessage), nameof(Way2SocketMessage.Parse))]
    public void When_Parse_Receive_Setted_Record_Index_Byte_Array_Message_Should_Returns_Valid_Instace()
    {
        //arrange
        //act
        var message = Way2SocketMessage.Parse(new byte[] { 0x7D, 0x01, 0x83, 0x00, 0x82 });
        bool result = message;

        //assert
        message.Should().NotBe(Way2SocketMessage.Error);
        result.Should().BeTrue();
    }

    [Fact]
    [Trait(nameof(Way2SocketMessage), nameof(Way2SocketMessage.Parse))]
    public void When_Parse_Receive_Current_Record_DateTime_Byte_Array_Message_Should_Returns_Valid_Instace()
    {
        //arrange
        //act
        var message = Way2SocketMessage.Parse(new byte[] { 0x7D, 0x05, 0x84, 0x7D, 0xE1, 0xBC, 0x59, 0x2B, 0xD3 });
        DateTime result = message;

        //assert
        message.Should().NotBe(Way2SocketMessage.Error);
        result.ToString("yyyy-MM-dd HH:mm:ss").Should().Be("2014-01-23 17:25:10");
    }

    [Fact]
    [Trait(nameof(Way2SocketMessage), nameof(Way2SocketMessage.Parse))]
    public void When_Parse_Receive_Energy_Value_From_Current_Record_Byte_Array_Message_Should_Returns_Valid_Instace()
    {
        //arrange
        //act
        var message = Way2SocketMessage.Parse(new byte[] { 0x7D, 0x04, 0x85, 0x41, 0x20, 0x00, 0x00, 0xE0 });
        float result = message;

        //assert
        message.Should().NotBe(Way2SocketMessage.Error);

        result.Should().Be(10F);
    }

    [Fact]
    [Trait(nameof(Way2SocketMessage), nameof(Way2SocketMessage.Parse))]
    public void Given_A_Null_Byte_Array_When_Pass_To_Parse_Should_Returns_Error_Message()
    {
        var message = Way2SocketMessage.Parse(null);
        message.IsError.Should().BeTrue();
    }

    [Fact]
    [Trait(nameof(Way2SocketMessage), nameof(Way2SocketMessage.Parse))]
    public void Given_A_Empty_Byte_Array_When_Pass_To_Parse_Should_Returns_Error_Message()
    {
        var message = Way2SocketMessage.Parse(Array.Empty<byte>());
        message.IsError.Should().BeTrue();
    }

    [Fact]
    [Trait(nameof(Way2SocketMessage), nameof(Way2SocketMessage.Parse))]
    public void Given_Bytes_Payload_And_Data_Length_Is_Different_Length_Info_Should_Returns_Error_Message()
    {
        var message = Way2SocketMessage.Parse(new byte[] { 0x7D, 0x04, 0x85, 0x00 });
        message.IsError.Should().BeTrue();
    }

    [Fact]
    [Trait(nameof(Way2SocketMessage), nameof(Way2SocketMessage.Parse))]
    public void Given_Bytes_Payload_With_FunctionCode_Invalid_And_Not_Receiver_Should_Returns_Error_Message()
    {
        var message = Way2SocketMessage.Parse(new byte[] { 0x7D, 0x00, 0x15, 0x00 });
        message.IsError.Should().BeTrue();
    }

    [Fact]
    [Trait(nameof(Way2SocketMessage), nameof(Way2SocketMessage.Parse))]
    public void Given_Bytes_Payload_With_Checksum_Invalid_Should_Returns_Error_Message()
    {
        var message = Way2SocketMessage.Parse(new byte[] { 0x7D, 0x00, 0x85, 0x00 });
        message.IsError.Should().BeTrue();
    }

    [Fact]
    [Trait(nameof(Way2SocketMessage), nameof(Way2SocketMessage.Parse))]
    public void Given_Bytes_Payload_With_Data_Inside_When_Checksum_Invalid_Should_Returns_Error_Message()
    {
        var message = Way2SocketMessage.Parse(new byte[] { 0x7D, 0x02, 0x85, 0x01, 0x02, 0x00 });
        message.IsError.Should().BeTrue();
    }
}
