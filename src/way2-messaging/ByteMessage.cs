using System.Collections.Generic;
using System.Globalization;
using System;
using System.Linq;

namespace Way2.Messaging;

public readonly struct Way2SocketMessage : IEquatable<Way2SocketMessage>
{
    public const byte FrameHeader = 0x7D;

    public static Way2SocketMessage Empty => default;
    public static Way2SocketMessage Error { get; }
    public static Way2SocketMessage ReadSerialNumber { get; }
    public static Way2SocketMessage ReadStatusRecord { get; }
    public static Way2SocketMessage ReadCurrentRecordDateTime { get; } 
    public static Way2SocketMessage ReadCurrentEnergyValue { get; } 

    public bool IsEmpty => functionCode == FunctionCodes.Empty;
    public bool IsError => functionCode == FunctionCodes.Error;

    private readonly byte length;
    private readonly byte functionCode;
    private readonly ArraySegment<byte> data;
    private readonly byte checksum;

    static Way2SocketMessage()
    {
        Error = new Way2SocketMessage(FunctionCodes.Error);
        ReadSerialNumber = new Way2SocketMessage(FunctionCodes.ReadSerialNumber);
        ReadStatusRecord = new Way2SocketMessage(FunctionCodes.ReadStatusRecord);
        ReadCurrentRecordDateTime = new Way2SocketMessage(FunctionCodes.ReadCurrentRecordDateTime);
        ReadCurrentEnergyValue = new Way2SocketMessage(FunctionCodes.ReadCurrentEnergyValue);
    }

    public Way2SocketMessage()
    {
        this.length = 0;
        this.functionCode = FunctionCodes.Empty;
        this.data = ArraySegment<byte>.Empty;
        this.checksum = functionCode;
    }

    public Way2SocketMessage(byte functionCode) : this()
    {
        this.functionCode = functionCode;
        this.checksum = functionCode;
    }

    private Way2SocketMessage(byte functionCode, byte length, ArraySegment<byte> data)
    {
        this.length = length;
        this.functionCode = functionCode;
        this.data = data;
        this.checksum = CalculateChecksum(length, functionCode, data);
    }

    public override int GetHashCode()
        => IsEmpty
            ? this.functionCode.GetHashCode() 
            : HashCode.Combine(length, functionCode, data.GetHashCode());

    public bool Equals(Way2SocketMessage message)
        => this.functionCode == message.functionCode &&
            this.length == message.length &&
            this.data == message.data;

    public override bool Equals(object? obj)
        => obj is Way2SocketMessage message && Equals(message);


    public static implicit operator byte[](Way2SocketMessage message)
        => message.IsEmpty
            ? new[] { FrameHeader, message.length, message.functionCode, message.checksum } 
            : new[] { FrameHeader, message.length, message.functionCode }
                    .Concat(message.data.Append(message.checksum))
                    .ToArray();

    public static implicit operator string(Way2SocketMessage message)
        => message.IsEmpty
            ? string.Empty
            : System.Text.Encoding.UTF8.GetString(message.data);

    public static implicit operator Range(Way2SocketMessage message)
    {
        if (message.length == 4)
        {
            var start = ((message.data[0] << 8) | message.data[1]);
            var end = ((message.data[2] << 8) | message.data[3]);

            return new Range(start, end);
        }

        return Range.All;
    }

    public static implicit operator bool(Way2SocketMessage message)
        => message.length == 1 && message.data[0] == 0x00;

    public static implicit operator DateTime(Way2SocketMessage message)
    {
        if (message.length == 5)
        {
            ushort first = message.data[0];
            ushort second = message.data[1];

            ushort yearAndMonth = (ushort)((first << 8) | second);

            ushort year = (ushort) (yearAndMonth >> 4);
            ushort month = (ushort)(((yearAndMonth << 12) & 0xF000) >> 12);

            ushort third = message.data[2];
            ushort forth = message.data[3];
            ushort fifth = message.data[4];

            ushort dayHourMin = (ushort)((third << 8) | forth);

            ushort day = (ushort) (dayHourMin >> 11);
            ushort hour = (ushort) ((dayHourMin >> 6) & (0xFFFF >> 11));
            ushort min = (ushort) (((dayHourMin << 10) & 0xFFFF) >> 10);

            ushort seconds = (ushort)(fifth >> 2);

            return new DateTime(year, month, day, hour, min, seconds);
        }

        return DateTime.MinValue;
    }

    public static implicit operator float(Way2SocketMessage message)
        => message.IsEmpty ? 0 : BitConverter.ToSingle(message.data.Reverse().ToArray(), 0);

    public static Way2SocketMessage Parse(byte[]? payload)
    {
        if (payload is null)
        {
            return Error;
        }

        if (payload.Length < 3)
        {
            return Error;
        }

        byte length = payload[1];

        if (length != payload.Length - 4)
        {
            return Error;
        }

        byte functionCode = payload[2];

        if (!FunctionCodes.IsValidReceiver(functionCode))
        {
            return Error;
        }

        byte checksum = payload.Length == 4 ? payload[3] : payload.Last();

        if (length > 0)
        {
            var data = payload.AsSpan(3, length).ToArray();

            var calculatedChecksum = CalculateChecksum(length, functionCode, data);

            if (checksum != calculatedChecksum)
            {
                return Error;
            }

            return new(functionCode, length, data);
        }

        if (checksum != functionCode)
        {
            return Error;
        }

        return new(functionCode);
    }

    public static byte CalculateChecksum(byte length, byte functionCode, ArraySegment<byte> data)
    {
        byte checksum = functionCode;

        if (length > 0)
        {
            checksum ^= length;
            foreach(byte value in data)
            {
                checksum ^= value;
            }
        }

        return checksum;
    }

    public static Way2SocketMessage SetRecordIndex(ushort index)
        => new Way2SocketMessage(
                functionCode: FunctionCodes.SetRecordIndex,
                data: new byte[] 
                { 
                    (byte)((0xFF00 & index) >> 8), 
                    (byte)(0x00FF & index)
                },
                length: 0x02);

}

public class ByteMessage
{
    public static ByteMessage Empty { get; }
    public static ByteMessage Error { get; }
    public static ByteMessage ReadSerialNumber { get; }
    public static ByteMessage ReadStatusRecord { get; }
    public static ByteMessage ReadCurrentRecordDateTime { get; } 
    public static ByteMessage ReadCurrentEnergyValue { get; } 
    public const int MinimumSize = 4;

    public byte FrameHeader { get; private set; } = 0x7D;
    public byte? Length { get; private set; }
    public byte FunctionCode { get; private set; }
    public byte[] Data { get; private set; }
    public byte CalculatedChecksum => Calculate();
    public virtual byte Checksum { get; private set; }

    static ByteMessage()
    {
        ReadSerialNumber = CreateMessage(FunctionCodes.ReadSerialNumber);
        ReadStatusRecord = CreateMessage(FunctionCodes.ReadStatusRecord);
        ReadCurrentRecordDateTime = CreateMessage(FunctionCodes.ReadCurrentRecordDateTime);
        ReadCurrentEnergyValue = CreateMessage(FunctionCodes.ReadCurrentEnergyValue);
        Error = CreateMessage(FunctionCodes.Error);
        Empty = CreateMessage(FunctionCodes.Empty);
    }

    private ByteMessage() {  }

    private static ByteMessage CreateMessage(byte functionCode)
        => new FixedByteMessage
        {
            FunctionCode = functionCode,
            Data = new byte[] { 0x00 },
        };

    public static ByteMessage SetRecordIndex(ushort index)
    {
        byte first = (byte)((0xFF00 & index) >> 8);
        byte second = (byte)(0x00FF & index);

        var data = new byte[]{ first, second };
        
        return new FixedByteMessage
        {
            FunctionCode = FunctionCodes.SetRecordIndex,
            Data = data, 
            Length = Convert.ToByte(data.Length)
        };
    }

    public static ByteMessage Parse(byte[] message)
    {
        const int CountFields = 4;
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        if (message is null)
        {
            return ByteMessage.Error;
        }

        if (message.Length < 3)
        {
            throw new ArgumentException("The message byte array is empty");
        }

        var functionCode = message[2];
        byte? length = message[1];
        byte[] data = new byte[]{ 0x00 };
        byte checksum = 0x00;

        if (message.Length > 3)
        {
            if ((length.GetValueOrDefault(0) + CountFields) == message.Length)
            {
                data = GetDataFrom(message, length.Value).ToArray();
                checksum = message[message.Length - 1];
            }
            else
                throw new InvalidOperationException($"{length} - {message.Length}");
        }

        return new ByteMessage
        {
            FunctionCode = functionCode,
            Data = data,
            Length = length,
            Checksum = checksum
        };

        IEnumerable<byte> GetDataFrom(byte[] message, byte length)
        {
            for(var i = 0; i < length; i++)
            {
                yield return message[i + 3];
            }
        }
    }

    public float AsFloat()
        => BitConverter.ToSingle(Data.Reverse().ToArray(), 0);
    
    public string AsString()
        => System.Text.Encoding.UTF8.GetString(Data);

    public bool AsBoolean()
        => Data[0] == 0x00;

    public DateTime AsDateTime()
    {
        ushort first = Data[0];
        ushort second = Data[1];

        ushort yearAndMonth = (ushort)((first << 8) | second);

        ushort year = (ushort) (yearAndMonth >> 4);
        ushort month = (ushort)(((yearAndMonth << 12) & 0xF000) >> 12);

        ushort third = Data[2];
        ushort forth = Data[3];
        ushort fifth = Data[4];

        ushort dayHourMin = (ushort)((third << 8) | forth);

        ushort day = (ushort) (dayHourMin >> 11);
        ushort hour = (ushort) ((dayHourMin >> 6) & (0xFFFF >> 11));
        ushort min = (ushort) (((dayHourMin << 10) & 0xFFFF) >> 10);

        ushort seconds = (ushort)(fifth >> 2);

        return new DateTime(year, month, day, hour, min, seconds);
    }

    public IEnumerable<ushort> AsShortEnumerable()
    {
        for(int i = 0; i < Data.Length; i+=2)
        {
            ushort first = Data[i];
            ushort second = Data[i + 1];

            ushort value = (ushort)((first << 8) | second);
            yield return value;
        }
    }

    public byte[] ToByteArray()
    {
        var header = new byte[] { this.FrameHeader, this.Length.GetValueOrDefault(0x00), this.FunctionCode };

        if (Length.HasValue)
        {
            header = header.Concat(Data).ToArray();
        }

        return header.Concat(new[] { this.Checksum }).ToArray();
    }

    protected byte Calculate()
    {
        byte checksum = this.FunctionCode;

        if (Length.HasValue)
        {
            checksum ^= Length.Value;
        }

        foreach(byte value in Data)
        {
            checksum ^= value;
        }

        return checksum;
    }

    private class FixedByteMessage : ByteMessage
    {
        public override byte Checksum => CalculatedChecksum;
    }
}
