using System.Collections.Generic;
using System.Globalization;
using Xunit;
using Moq;
using FluentAssertions;
using System;
using System.Linq;
using Way2.Messaging;

namespace Way2.Messaging.Tests;

public class ByteMessageTest
{
    [Fact]
    [Trait(nameof(ByteMessage), nameof(ByteMessage.ReadSerialNumber))]
    public void When_Get_ReadSerialNumber_Should_Returns_Valid_ByteMessage()
    {
        //arrange
        var first = ByteMessage.ReadSerialNumber;
        var second = ByteMessage.ReadSerialNumber;

        //act

        //assert
        first.Should().BeEquivalentTo(second);
        first.FunctionCode.Should().Be(FunctionCodes.ReadSerialNumber);
        first.Length.Should().BeNull();
        first.Data.Should().NotBeNullOrEmpty();
        first.Data.Should().HaveCount(1);
        first.Data.Should().OnlyContain(b => b == 0x00);
        first.Checksum.Should().Be(FunctionCodes.ReadSerialNumber);
    }

    [Fact]
    [Trait(nameof(ByteMessage), nameof(ByteMessage.ReadStatusRecord))]
    public void When_Get_ReadRecordStatus_Should_Returns_Valid_ByteMessage()
    {
        //arrange
        var first = ByteMessage.ReadStatusRecord;
        var second = ByteMessage.ReadStatusRecord;

        //act

        //assert
        first.Should().BeEquivalentTo(second);
        first.FunctionCode.Should().Be(FunctionCodes.ReadStatusRecord);
        first.Length.Should().BeNull();
        first.Data.Should().NotBeNullOrEmpty();
        first.Data.Should().HaveCount(1);
        first.Data.Should().OnlyContain(b => b == 0x00);
        first.Checksum.Should().Be(FunctionCodes.ReadStatusRecord);
    }

    [Fact]
    [Trait(nameof(ByteMessage), nameof(ByteMessage.ReadCurrentRecordDateTime))]
    public void When_Get_ReadCurrentRecordDateTime_Should_Returns_Valid_ByteMessage()
    {
        //arrange
        var first = ByteMessage.ReadCurrentRecordDateTime;
        var second = ByteMessage.ReadCurrentRecordDateTime;

        //act

        //assert
        first.Should().BeEquivalentTo(second);
        first.FunctionCode.Should().Be(FunctionCodes.ReadCurrentRecordDateTime);
        first.Length.Should().BeNull();
        first.Data.Should().NotBeNullOrEmpty();
        first.Data.Should().HaveCount(1);
        first.Data.Should().OnlyContain(b => b == 0x00);
        first.Checksum.Should().Be(FunctionCodes.ReadCurrentRecordDateTime);

    }

    [Fact]
    [Trait(nameof(ByteMessage), nameof(ByteMessage.ReadCurrentEnergyValue))]
    public void When_Get_ReadCurrentEnergyValue_Should_Returns_Valid_ByteMessage()
    {
        //arrange
        var first = ByteMessage.ReadCurrentEnergyValue;
        var second = ByteMessage.ReadCurrentEnergyValue;

        //act

        //assert
        first.Should().BeEquivalentTo(second);
        first.FunctionCode.Should().Be(FunctionCodes.ReadCurrentEnergyValue);
        first.Length.Should().BeNull();
        first.Data.Should().NotBeNullOrEmpty();
        first.Data.Should().HaveCount(1);
        first.Data.Should().OnlyContain(b => b == 0x00);
        first.Checksum.Should().Be(FunctionCodes.ReadCurrentEnergyValue);
    }

    [Fact]
    [Trait(nameof(ByteMessage), nameof(ByteMessage.ReadCurrentEnergyValue))]
    public void When_Get_Error_Should_Returns_Valid_ByteMessage()
    {
        //arrange
        var first = ByteMessage.Error;
        var second = ByteMessage.Error;

        //act

        //assert
        first.Should().BeEquivalentTo(second);
        first.FunctionCode.Should().Be(FunctionCodes.Error);
        first.Length.Should().BeNull();
        first.Data.Should().NotBeNullOrEmpty();
        first.Data.Should().HaveCount(1);
        first.Data.Should().OnlyContain(b => b == 0x00);
        first.Checksum.Should().Be(FunctionCodes.Error);
    }

    [Theory]
    [Trait(nameof(ByteMessage), nameof(ByteMessage.SetRecordIndex))]
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
        var message = ByteMessage.SetRecordIndex(index);

        //assert
        message.Should().NotBeNull();
        message.FunctionCode.Should().Be(FunctionCodes.SetRecordIndex);
        message.Length.Should().Be(length);
        message.Data.Should().NotBeNullOrEmpty();
        message.Data.Should().HaveCount(2);
        message.Data.Should().Contain(d => d == firstDataValue);
        message.Data.Should().Contain(d => d == secondDataValue);
        message.Checksum.Should().Be(checksum);
    }

    [Fact]
    [Trait(nameof(ByteMessage), nameof(ByteMessage.Parse))]
    public void When_Parse_Receive_SerialNumber_Byte_Array_Message_Should_Returns_Valid_Instace()
    {
        //arrange
        //act
        var message = ByteMessage.Parse(new byte[] { 0x7D, 0x08, 0x81, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x00, 0xC9 });

        //assert
        message.Should().NotBeNull();
        message.FunctionCode.Should().Be(FunctionCodes.ReceiveSerialNumber);
        message.Length.Should().NotBeNull();
        message.Length.Should().Be(0x08);
        message.Data.Should().NotBeNullOrEmpty();
        message.Data.Should().HaveCount(8);
        message.Checksum.Should().Be(0xC9);
        message.CalculatedChecksum.Should().Be(message.Checksum);

        message.AsString().Should().Be("ABCDEFG\0");
    }

    [Fact]
    [Trait(nameof(ByteMessage), nameof(ByteMessage.Parse))]
    public void When_Parse_Receive_Staus_Record_Byte_Array_Message_Should_Returns_Valid_Instace()
    {
        //arrange
        //act
        var message = ByteMessage.Parse(new byte[] { 0x7D, 0x04, 0x82, 0x01, 0x2C, 0x02, 0x58, 0xF1 });

        //assert
        message.Should().NotBeNull();
        message.FunctionCode.Should().Be(FunctionCodes.ReceiveStatusRecord);
        message.Length.Should().NotBeNull();
        message.Length.Should().Be(0x04);
        message.Data.Should().NotBeNullOrEmpty();
        message.Data.Should().HaveCount(4);
        message.Checksum.Should().Be(0xF1);
        message.CalculatedChecksum.Should().Be(message.Checksum);

        var indexes = message.AsShortEnumerable();
        indexes.Should().NotBeNullOrEmpty();
        indexes.Should().Contain(i => i == 300);
        indexes.Should().Contain(i => i == 600);
    }

    [Fact]
    [Trait(nameof(ByteMessage), nameof(ByteMessage.Parse))]
    public void When_Parse_Receive_Setted_Record_Index_Byte_Array_Message_Should_Returns_Valid_Instace()
    {
        //arrange
        //act
        var message = ByteMessage.Parse(new byte[] { 0x7D, 0x01, 0x83, 0x00, 0x82 });

        //assert
        message.Should().NotBeNull();
        message.FunctionCode.Should().Be(FunctionCodes.ReceiveSettedRecordIndex);
        message.Length.Should().NotBeNull();
        message.Length.Should().Be(0x01);
        message.Data.Should().NotBeNullOrEmpty();
        message.Data.Should().HaveCount(1);
        message.Checksum.Should().Be(0x82);
        message.CalculatedChecksum.Should().Be(message.Checksum);

        message.AsBoolean().Should().BeTrue();
    }

    [Fact]
    [Trait(nameof(ByteMessage), nameof(ByteMessage.Parse))]
    public void When_Parse_Receive_Current_Record_DateTime_Byte_Array_Message_Should_Returns_Valid_Instace()
    {
        //arrange
        //act
        var message = ByteMessage.Parse(new byte[] { 0x7D, 0x05, 0x84, 0x7D, 0xE1, 0xBC, 0x59, 0x2B, 0xD3 });

        //assert
        message.Should().NotBeNull();
        message.FunctionCode.Should().Be(FunctionCodes.ReceiveCurrentRecordDateTime);
        message.Length.Should().NotBeNull();
        message.Length.Should().Be(0x05);
        message.Data.Should().NotBeNullOrEmpty();
        message.Data.Should().HaveCount(5);
        message.Checksum.Should().Be(0xD3);
        message.CalculatedChecksum.Should().Be(message.Checksum);

        var datetime = message.AsDateTime();
        datetime.ToString("yyyy-MM-dd HH:mm:ss").Should().Be("2014-01-23 17:25:10");
    }

    [Fact]
    [Trait(nameof(ByteMessage), nameof(ByteMessage.Parse))]
    public void When_Parse_Receive_Energy_Value_From_Current_Record_Byte_Array_Message_Should_Returns_Valid_Instace()
    {
        //arrange
        //act
        var message = ByteMessage.Parse(new byte[] { 0x7D, 0x04, 0x85, 0x41, 0x20, 0x00, 0x00, 0xE0 });

        //assert
        message.Should().NotBeNull();
        message.FunctionCode.Should().Be(FunctionCodes.ReceiveEnergyValueFromCurrentRecord);
        message.Length.Should().NotBeNull();
        message.Length.Should().Be(0x04);
        message.Data.Should().NotBeNullOrEmpty();
        message.Data.Should().HaveCount(4);
        message.Checksum.Should().Be(0xE0);
        message.CalculatedChecksum.Should().Be(message.Checksum);

        message.AsFloat().Should().Be(10F);
    }

    [Fact]
    [Trait(nameof(ByteMessage), nameof(ByteMessage.Parse))]
    public void Given_A_Null_Byte_Array_When_Pass_To_Parse_Should_Thrown_Exception()
    {
        Action callParseWithNull = () => ByteMessage.Parse(null);
        callParseWithNull.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    [Trait(nameof(ByteMessage), nameof(ByteMessage.Parse))]
    public void Given_A_Less_Three_Items_Byte_Array_When_Pass_To_Parse_Should_Thrown_Exception()
    {
        Action callParseWithEmpty = () => ByteMessage.Parse(Array.Empty<byte>());
        Action callParseWithOneItem = () => ByteMessage.Parse(new byte[] { 0x00 });
        Action callParseWithTwoItems = () => ByteMessage.Parse(new byte[] { 0x00, 0x00 });
        callParseWithEmpty.Should().ThrowExactly<ArgumentException>();
        callParseWithOneItem.Should().ThrowExactly<ArgumentException>();
        callParseWithTwoItems.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    [Trait(nameof(ByteMessage), nameof(ByteMessage.Parse))]
    public void Given_Four_Items_Byte_Array_And_Length_Not_Match_Of_Data_Length_When_Pass_To_Parse_Should_Returns_An_Instance_With_Invalid_Checksum()
    {
        var message = ByteMessage.Parse(new byte[]
        {
            0x7D, 0x01, 0x83, 0x00
        });

        message.Checksum.Should().NotBe(message.CalculatedChecksum);
    }

    [Fact]
    [Trait(nameof(ByteMessage), nameof(ByteMessage.Parse))]
    public void When()
    {
        //arrange
        //act

        ushort value = 0x7DE1;

        ushort year = (ushort) (value >> 4);

        year.Should().Be(2014);

        ushort month = (ushort)(((value << 12) & 0xF000) >> 12);
        month.Should().Be(1);

        ushort dayHourMin = 0xBC59;
        ushort day = (ushort) (dayHourMin >> 11);
        ushort hour = (ushort) ((dayHourMin >> 6) & (0xFFFF >> 11));
        ushort min = (ushort) (((dayHourMin << 10) & 0xFFFF) >> 10);

        day.Should().Be(23);
        hour.Should().Be(17);
        min.Should().Be(25);

        ushort last = 0x2B;
        ushort second = (ushort)(last >> 2);

        second.Should().Be(10);

        byte floatFirst = 0x41;
        byte floatSecond = 0x20;
        byte floatThird = 0x00;
        byte floatFourth = 0x00;

        //int valueFloat = ((floatFirst << 24) | (floatSecond << 16) | (floatThird << 8) | floatFourth);

        var valueFloat = BitConverter.ToSingle(new byte[]{ floatFirst, floatSecond, floatThird, floatFourth }.Reverse().ToArray(), 0);
        valueFloat.Should().Be(10);

        //assert
    }
}
