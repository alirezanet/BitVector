namespace BitVector;

/// <summary>
///     The <c>BitVector</c> struct provides a memory-efficient and high-performance solution
///     for manipulating individual bits within .NET applications. It optimizes memory usage
///     by storing a single bit per item and offers a versatile set of bitwise operations.
///     This struct is particularly suitable for scenarios where memory conservation and
///     efficient bit manipulation are crucial.
/// </summary>
public readonly struct BitVector
{
    private readonly byte[] _bitBuffer;
    public readonly ulong Lenght;
    public readonly long BufferLenght;

    public BitVector(ulong lenght)
    {
        BufferLenght = (uint)((lenght + 7) / 8);
        _bitBuffer = new byte[BufferLenght];
        Lenght = lenght;
    }

    public BitVector(byte[] bitBuffer, ulong lenght)
    {
        _bitBuffer = bitBuffer;
        Lenght = lenght;
        BufferLenght = bitBuffer.LongLength;
    }

    public BitVector(ulong lenght, bool defaultValue = false)
    {
        BufferLenght = (uint)((lenght + 7) / 8);
        _bitBuffer = new byte[BufferLenght];
        Lenght = lenght;
        InternalSetAll(defaultValue);
    }

    public BitVector SetAll(bool value)
    {
        return new BitVector(Lenght, value);
    }

    public byte[] ToByteArray()
    {
        return _bitBuffer.ToArray();
    }

    public string ToBinaryString()
    {
        return string.Join("", _bitBuffer.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
    }

    public BitVector Set(bool value, params ulong[] bitIndexes)
    {
        var newBitBuffer = new byte[BufferLenght];
        _bitBuffer.CopyTo(newBitBuffer, 0);

        foreach (var bitIndex in bitIndexes)
        {
            if (bitIndex >= Lenght)
                throw new ArgumentOutOfRangeException(nameof(bitIndexes), "Bit index is out of range.");

            var byteIndex = bitIndex / 8;
            var bitOffset = (int)bitIndex % 8;

            var mask = (byte)(1 << bitOffset);
            var newValue = (byte)(value ? mask : 0);

            newBitBuffer[byteIndex] = (byte)(newBitBuffer[byteIndex] & ~mask);
            newBitBuffer[byteIndex] |= newValue;
        }

        return new BitVector(newBitBuffer, Lenght);
    }

    private static byte GetValue(bool value)
    {
        return (byte)(value ? 0xFF : 0x00);
    }

    private void InternalSetAll(bool value)
    {
        var byteValue = GetValue(value);
        for (var i = 0; i < _bitBuffer.Length; i++)
        {
            if (i == 0)
            {
                var remainingBits = (int)(Lenght % 8);
                _bitBuffer[i] = (byte)(byteValue >> (8 - remainingBits));
                continue;
            }

            _bitBuffer[i] = byteValue;
        }
    }

    public BitVector And(BitVector value)
    {
        var maxBufferLength = Math.Max(BufferLenght, value.BufferLenght);

        var newBitBuffer = new byte[maxBufferLength];

        for (var i = 0; i < maxBufferLength; i++)
        {
            var thisByte = i < BufferLenght ? _bitBuffer[i] : (byte)0;
            var valueByte = i < value.BufferLenght ? value._bitBuffer[i] : (byte)0;

            newBitBuffer[i] = (byte)(thisByte & valueByte);
        }

        return new BitVector(newBitBuffer, (ulong)maxBufferLength * 8);
    }

    public BitVector Or(BitVector value)
    {
        var maxBufferLength = Math.Max(BufferLenght, value.BufferLenght);

        var newBitBuffer = new byte[maxBufferLength];

        for (var i = 0; i < maxBufferLength; i++)
        {
            var thisByte = i < BufferLenght ? _bitBuffer[i] : (byte)0;
            var valueByte = i < value.BufferLenght ? value._bitBuffer[i] : (byte)0;

            newBitBuffer[i] = (byte)(thisByte | valueByte);
        }

        return new BitVector(newBitBuffer, (ulong)maxBufferLength * 8);
    }

    public BitVector Xor(BitVector value)
    {
        var maxBufferLength = Math.Max(BufferLenght, value.BufferLenght);

        var newBitBuffer = new byte[maxBufferLength];

        for (var i = 0; i < maxBufferLength; i++)
        {
            var thisByte = i < BufferLenght ? _bitBuffer[i] : (byte)0;
            var valueByte = i < value.BufferLenght ? value._bitBuffer[i] : (byte)0;

            newBitBuffer[i] = (byte)(thisByte ^ valueByte);
        }

        return new BitVector(newBitBuffer, (ulong)maxBufferLength * 8);
    }

    public BitVector LeftShift(int count)
    {
        var newBitBuffer = new byte[BufferLenght];
        var byteShift = count / 8;
        var bitShift = count % 8;

        for (var i = 0; i < BufferLenght; i++)
            if (i + byteShift < BufferLenght)
            {
                var shiftedByte = (byte)(_bitBuffer[i + byteShift] << bitShift);
                newBitBuffer[i] |= shiftedByte;

                if (bitShift > 0 && i + byteShift + 1 < BufferLenght)
                {
                    shiftedByte = (byte)(_bitBuffer[i + byteShift + 1] >> (8 - bitShift));
                    newBitBuffer[i] |= shiftedByte;
                }
            }

        return new BitVector(newBitBuffer, Lenght);
    }

    public BitVector RightShift(int count)
    {
        var newBitBuffer = new byte[BufferLenght];
        var byteShift = count / 8;
        var bitShift = count % 8;

        for (var i = 0; i < BufferLenght; i++)
            if (i >= byteShift)
            {
                var shiftedByte = (byte)(_bitBuffer[i - byteShift] >> bitShift);
                newBitBuffer[i] |= shiftedByte;

                if (bitShift > 0 && i - byteShift - 1 >= 0)
                {
                    shiftedByte = (byte)(_bitBuffer[i - byteShift - 1] << (8 - bitShift));
                    newBitBuffer[i] |= shiftedByte;
                }
            }

        return new BitVector(newBitBuffer, Lenght);
    }

    public int CountSetBits()
    {
        var count = 0;
        foreach (var byteValue in _bitBuffer) count += CountSetBitsInByte(byteValue);
        return count;
    }

    public int FirstSetBitIndex()
    {
        for (var i = 0; i < BufferLenght * 8; i++)
            if (GetBit(i))
                return i;
        return -1;
    }

    public bool GetBit(long bitIndex)
    {
        if ((ulong)bitIndex >= Lenght) throw new ArgumentOutOfRangeException(nameof(bitIndex), "Bit index is out of range.");

        var byteIndex = bitIndex / 8;
        var bitOffset = (int)bitIndex % 8;

        var mask = (byte)(1 << bitOffset);
        return (_bitBuffer[byteIndex] & mask) != 0;
    }

    private static int CountSetBitsInByte(byte value)
    {
        var count = 0;
        while (value != 0)
        {
            count += value & 1;
            value >>= 1;
        }

        return count;
    }

    public BitVector Not()
    {
        var newBitBuffer = new byte[BufferLenght];

        for (var i = 0; i < BufferLenght; i++) newBitBuffer[i] = (byte)~_bitBuffer[i];

        return new BitVector(newBitBuffer, Lenght);
    }

    
    public static BitVector operator <<(BitVector value, int shiftAmount)
    {
        return value.LeftShift(shiftAmount);
    }

    public static BitVector operator >>(BitVector value, int shiftAmount)
    {
        return value.RightShift(shiftAmount);
    }
    
    public static BitVector operator &(BitVector left, BitVector right)
    {
        return left.And(right);
    }

    public static BitVector operator |(BitVector left, BitVector right)
    {
        return left.Or(right);
    }

    public static BitVector operator ^(BitVector left, BitVector right)
    {
        return left.Xor(right);
    }

    public static BitVector operator ~(BitVector value)
    {
        return value.Not();
    }
}