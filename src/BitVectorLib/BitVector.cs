namespace BitVectorLib;

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
    public readonly long Length;
    public readonly int BufferLength;


    public BitVector(long lenght)
    {
        checked
        {
            BufferLength = (int)((lenght + 7) / 8);
            if (BufferLength == 0) BufferLength++;
            _bitBuffer = new byte[BufferLength];
            Length = lenght;
        }
    }

    public BitVector(byte[] bitBuffer, long lenght)
    {
        _bitBuffer = bitBuffer;
        Length = lenght;
        BufferLength = bitBuffer.Length;
    }

    public BitVector(long lenght, bool defaultValue = false)
    {
        checked
        {
            BufferLength = (int)((lenght + 7) / 8);
            if (BufferLength == 0) BufferLength++;
            _bitBuffer = new byte[BufferLength];
            Length = lenght;
        }
        InternalSetAll(defaultValue);
    }

    public BitVector(long length, params long[] autoSetIndexes)
    {
        checked
        {
            BufferLength = (int)((length + 7) / 8);
            _bitBuffer = new byte[BufferLength];
            Length = length;
        }

        foreach (var bitIndex in autoSetIndexes)
        {
            if (bitIndex >= Length)
                throw new ArgumentOutOfRangeException(nameof(autoSetIndexes), "Bit index is out of range.");

            var byteIndex = bitIndex / 8;
            var bitOffset = (int)bitIndex % 8;

            var mask = (byte)(1 << bitOffset);

            _bitBuffer[byteIndex] = (byte)(_bitBuffer[byteIndex] & ~mask);
            _bitBuffer[byteIndex] |= mask;
        }
    }

    public BitVector SetAll(bool value)
    {
        return new BitVector(Length, value);
    }

    public byte[] ToByteArray()
    {
        return _bitBuffer.ToArray();
    }

    public void CopyTo(byte[] buffer, int index)
    {
        _bitBuffer.CopyTo(buffer, index);
    }

    public string ToBinaryString()
    {
        return string.Join("", _bitBuffer.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
    }

    public BitVector Set(bool value, params long[] bitIndexes)
    {
        var newBitBuffer = new byte[BufferLength];
        _bitBuffer.CopyTo(newBitBuffer, 0);

        foreach (var bitIndex in bitIndexes)
        {
            if (bitIndex >= Length)
                throw new ArgumentOutOfRangeException(nameof(bitIndexes), "Bit index is out of range.");

            var byteIndex = bitIndex / 8;
            var bitOffset = (int)bitIndex % 8;

            var mask = (byte)(1 << bitOffset);
            var newValue = (byte)(value ? mask : 0);

            newBitBuffer[byteIndex] = (byte)(newBitBuffer[byteIndex] & ~mask);
            newBitBuffer[byteIndex] |= newValue;
        }

        return new BitVector(newBitBuffer, Length);
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
                var remainingBits = (int)(Length % 8);
                if (remainingBits == 0)
                    _bitBuffer[i] = byteValue;
                else
                    _bitBuffer[i] = (byte)(byteValue >> (8 - remainingBits));

                continue;
            }

            _bitBuffer[i] = byteValue;
        }
    }

    public BitVector And(BitVector value)
    {
        var maxBufferLength = Math.Max(BufferLength, value.BufferLength);

        var newBitBuffer = new byte[maxBufferLength];

        for (var i = 0; i < maxBufferLength; i++)
        {
            var thisByte = i < BufferLength ? _bitBuffer[i] : (byte)0;
            var valueByte = i < value.BufferLength ? value._bitBuffer[i] : (byte)0;

            newBitBuffer[i] = (byte)(thisByte & valueByte);
        }

        return new BitVector(newBitBuffer, maxBufferLength * 8);
    }

    public BitVector Or(BitVector value)
    {
        var maxBufferLength = Math.Max(BufferLength, value.BufferLength);

        var newBitBuffer = new byte[maxBufferLength];

        for (var i = 0; i < maxBufferLength; i++)
        {
            var thisByte = i < BufferLength ? _bitBuffer[i] : (byte)0;
            var valueByte = i < value.BufferLength ? value._bitBuffer[i] : (byte)0;

            newBitBuffer[i] = (byte)(thisByte | valueByte);
        }

        return new BitVector(newBitBuffer, maxBufferLength * 8);
    }

    public BitVector Xor(BitVector value)
    {
        var maxBufferLength = Math.Max(BufferLength, value.BufferLength);

        var newBitBuffer = new byte[maxBufferLength];

        for (var i = 0; i < maxBufferLength; i++)
        {
            var thisByte = i < BufferLength ? _bitBuffer[i] : (byte)0;
            var valueByte = i < value.BufferLength ? value._bitBuffer[i] : (byte)0;

            newBitBuffer[i] = (byte)(thisByte ^ valueByte);
        }

        return new BitVector(newBitBuffer, maxBufferLength * 8);
    }

    public BitVector LeftShift(int count)
    {
        var newBitBuffer = new byte[BufferLength];
        var byteShift = count / 8;
        var bitShift = count % 8;

        for (var i = 0; i < BufferLength; i++)
            if (i + byteShift < BufferLength)
            {
                var shiftedByte = (byte)(_bitBuffer[i + byteShift] << bitShift);
                newBitBuffer[i] |= shiftedByte;

                if (bitShift > 0 && i + byteShift + 1 < BufferLength)
                {
                    shiftedByte = (byte)(_bitBuffer[i + byteShift + 1] >> (8 - bitShift));
                    newBitBuffer[i] |= shiftedByte;
                }
            }

        return new BitVector(newBitBuffer, Length);
    }

    public BitVector RightShift(int count)
    {
        var newBitBuffer = new byte[BufferLength];
        var byteShift = count / 8;
        var bitShift = count % 8;

        for (var i = 0; i < BufferLength; i++)
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

        return new BitVector(newBitBuffer, Length);
    }

    public long CountSetBits()
    {
        long count = 0;
        foreach (var byteValue in _bitBuffer) count += CountSetBitsInByte(byteValue);
        return count;
    }

    public int FirstSetBitIndex()
    {
        for (var i = 0; i < BufferLength * 8; i++)
            if (GetBit(i))
                return i;
        return -1;
    }

    public bool GetBit(long bitIndex)
    {
        if (bitIndex >= Length)
            throw new ArgumentOutOfRangeException(nameof(bitIndex), "Bit index is out of range.");

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

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + Length.GetHashCode();
            for (var i = 0; i < BufferLength; i++) hash = hash * 23 + _bitBuffer[i].GetHashCode();
            return hash;
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is BitVector other) return Equals(other);
        return false;
    }

    public bool Equals(BitVector other)
    {
        if (Length != other.Length)
            return false;

        var byteCount = BufferLength;
        for (var i = 0; i < byteCount; i++)
            if (_bitBuffer[i] != other._bitBuffer[i])
                return false;

        return true;
    }

    public static bool operator ==(BitVector left, BitVector right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BitVector left, BitVector right)
    {
        return !(left == right);
    }


    public override string ToString()
    {
        return ToBinaryString();
    }

    public BitVector Not()
    {
        var newBitBuffer = new byte[BufferLength];

        for (var i = 0; i < BufferLength; i++) newBitBuffer[i] = (byte)~_bitBuffer[i];

        return new BitVector(newBitBuffer, Length);
    }


    public static BitVector operator <<(BitVector value, int shiftAmount)
    {
        return value.LeftShift(shiftAmount);
    }

    public static BitVector operator >> (BitVector value, int shiftAmount)
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