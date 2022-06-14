namespace Way2.Messaging;

public static class FunctionCodes
{
    public const byte ReadSerialNumber = 0x01;
    public const byte ReadStatusRecord = 0x02;
    public const byte SetRecordIndex = 0x03;
    public const byte ReadCurrentRecordDateTime = 0x04;
    public const byte ReadCurrentEnergyValue = 0x05;

    public const byte ReceiveSerialNumber = 0x81;
    public const byte ReceiveStatusRecord = 0x82;
    public const byte ReceiveSettedRecordIndex = 0x83;
    public const byte ReceiveCurrentRecordDateTime = 0x84;
    public const byte ReceiveEnergyValueFromCurrentRecord = 0x85;

    public const byte Empty = 0x00;
    public const byte Error = 0xFF;

    public static bool IsValidAction(byte functionCode)
        => functionCode == ReadCurrentEnergyValue ||
            functionCode == ReadStatusRecord ||
            functionCode == SetRecordIndex ||
            functionCode == ReadCurrentRecordDateTime ||
            functionCode == ReadSerialNumber ||
            functionCode == Empty ||
            functionCode == Error;

    public static bool IsValidReceiver(byte functionCode)
        => functionCode == ReceiveCurrentRecordDateTime ||
            functionCode == ReceiveEnergyValueFromCurrentRecord ||
            functionCode == ReceiveSerialNumber ||
            functionCode == ReceiveSettedRecordIndex ||
            functionCode == ReceiveStatusRecord || 
            functionCode == Empty ||
            functionCode == Error;

    public static bool IsValid(byte functionCode)
        => functionCode == ReadCurrentEnergyValue ||
            functionCode == ReadStatusRecord ||
            functionCode == SetRecordIndex ||
            functionCode == ReadCurrentRecordDateTime ||
            functionCode == ReadSerialNumber ||
            functionCode == ReceiveCurrentRecordDateTime ||
            functionCode == ReceiveEnergyValueFromCurrentRecord ||
            functionCode == ReceiveSerialNumber ||
            functionCode == ReceiveSettedRecordIndex ||
            functionCode == Empty ||
            functionCode == Error;
}
