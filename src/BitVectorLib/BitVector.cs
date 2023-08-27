using System.Collections;

namespace BitVectorLib;

/// <summary>
///     The <c>BitVector</c> struct provides a memory-efficient and high-performance solution
///     for manipulating individual bits within .NET applications. It optimizes memory usage
///     by storing a single bit per item and offers a versatile set of bitwise operations.
///     This struct is particularly suitable for scenarios where memory conservation and
///     efficient bit manipulation are crucial.
/// </summary>
public readonly struct BitVector : IEnumerable<bool>
{
    private readonly byte[] _bitBuffer;
    public readonly long BitLength;
    public readonly int BufferLength;

    public BitVector(long bitLength)
    {
        checked
        {
            BufferLength = (int)((bitLength + 7) / 8);
            _bitBuffer = new byte[BufferLength];
            BitLength = bitLength;
        }
    }

    public BitVector(byte[] bitBuffer, long bitLenght)
    {
        _bitBuffer = bitBuffer;
        BitLength = bitLenght;
        BufferLength = bitBuffer.Length;
    }

    public BitVector(long bitLength, bool defaultValue = false)
    {
        this = new BitVector(bitLength);
        InternalSetAll(defaultValue);
    }

    public BitVector(long bitLength, params long[] nonZeroIndexes)
    {
        this = new BitVector(bitLength);

        foreach (var bitIndex in nonZeroIndexes)
        {
            if (bitIndex >= BitLength)
                throw new ArgumentOutOfRangeException(nameof(nonZeroIndexes), "Bit index is out of range.");

            var byteIndex = bitIndex / 8;
            var bitOffset = (int)bitIndex % 8;

            var mask = (byte)(1 << bitOffset);

            _bitBuffer[byteIndex] = (byte)(_bitBuffer[byteIndex] & ~mask);
            _bitBuffer[byteIndex] |= mask;
        }
    }

    public BitVector(string binaryString)
    {
        if (string.IsNullOrEmpty(binaryString))
            throw new ArgumentException("Binary string cannot be null or empty.", nameof(binaryString));

        var length = binaryString.Length;
        var bufferLength = (length + 7) / 8; // Calculate buffer length more efficiently
        _bitBuffer = new byte[bufferLength];
        BitLength = length;
        BufferLength = bufferLength;

        var byteIndex = bufferLength - 1;
        var bitOffset = 0;

        for (var i = length - 1; i >= 0; i--)
        {
            if (binaryString[i] == '1')
                _bitBuffer[byteIndex] |= (byte)(1 << bitOffset);
            else if (binaryString[i] != '0')
                throw new ArgumentException("Binary string contains invalid characters.", nameof(binaryString));

            if (++bitOffset != 8) continue;
            
            bitOffset = 0;
            byteIndex--;
        }
    }


    public BitVectorEnumerator GetEnumerator()
    {
        return new BitVectorEnumerator(this);
    }

    IEnumerator<bool> IEnumerable<bool>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public BitVector SetAll(bool value)
    {
        return new BitVector(BitLength, value);
    }

    public byte[] ToByteArray()
    {
        return _bitBuffer.ToArray();
    }

    public void CopyTo(byte[] buffer, int index)
    {
        _bitBuffer.CopyTo(buffer, index);
    }


    public BitVector Set(bool value, params long[] bitIndexes)
    {
        var newBitBuffer = new byte[BufferLength];
        _bitBuffer.CopyTo(newBitBuffer, 0);

        foreach (var bitIndex in bitIndexes)
        {
            if (bitIndex >= BitLength)
                throw new ArgumentOutOfRangeException(nameof(bitIndexes), "Bit index is out of range.");

            var byteIndex = bitIndex / 8;
            var bitOffset = (int)bitIndex % 8;

            var mask = (byte)(1 << bitOffset);
            var newValue = (byte)(value ? mask : 0);

            newBitBuffer[byteIndex] = (byte)(newBitBuffer[byteIndex] & ~mask);
            newBitBuffer[byteIndex] |= newValue;
        }

        return new BitVector(newBitBuffer, BitLength);
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
                var remainingBits = (int)(BitLength % 8);
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

        return new BitVector(newBitBuffer, BitLength);
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

        return new BitVector(newBitBuffer, BitLength);
    }

    public long CountSetBits()
    {
        return _bitBuffer.Aggregate<byte, long>(0, (current, byteValue) => current + CountSetBitsInByte(byteValue));
    }

    public int FirstSetBitIndex()
    {
        for (var i = 0; i < BufferLength; i++)
        {
            if (_bitBuffer[i] == 0) continue;

            var bitIndex = i * 8;
            var byteValue = _bitBuffer[i];
            while (byteValue > 0)
            {
                if ((byteValue & 1) == 1) return bitIndex;
                byteValue >>= 1;
                bitIndex++;
            }
        }

        return -1;
    }


    public bool GetBit(long bitIndex)
    {
        if (bitIndex >= BitLength)
            throw new ArgumentOutOfRangeException(nameof(bitIndex), "Bit index is out of range.");

        var byteIndex = BufferLength - 1 - bitIndex / 8;
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
            hash = hash * 23 + BitLength.GetHashCode();
            for (var i = 0; i < BufferLength; i++) hash = hash * 23 + _bitBuffer[i].GetHashCode();
            return hash;
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is BitVector other) return Equals(other);
        return false;
    }

    public bool Equals(BitVector other, EqualityMode mode)
    {
        return mode switch
        {
            EqualityMode.CompareLengthAndData => EqualsLengthAndData(other),
            EqualityMode.CompareDataOnly => EqualsDataOnly(other),
            _ => throw new ArgumentException("Invalid equality mode.", nameof(mode))
        };
    }

    private bool EqualsLengthAndData(BitVector other)
    {
        // Compare both length and data
        if (BitLength != other.BitLength)
            return false;

        var byteCount = BufferLength;
        for (var i = 0; i < byteCount; i++)
            if (_bitBuffer[i] != other._bitBuffer[i])
                return false;

        return true;
    }

    private bool EqualsDataOnly(BitVector other)
    {
        // Compare data only
        var byteCount = Math.Min(BufferLength, other.BufferLength);
        for (var i = 0; i < byteCount; i++)
            if (_bitBuffer[i] != other._bitBuffer[i])
                return false;

        // If one BitVector is longer, check if the remaining bits are all zeros
        if (BufferLength > other.BufferLength)
        {
            for (var i = other.BufferLength; i < BufferLength; i++)
                if (_bitBuffer[i] != 0)
                    return false;
        }
        else if (other.BufferLength > BufferLength)
        {
            for (var i = BufferLength; i < other.BufferLength; i++)
                if (other._bitBuffer[i] != 0)
                    return false;
        }

        return true;
    }


    public bool Equals(BitVector other)
    {
        // Default to comparing both length and data
        return EqualsLengthAndData(other);
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
        return string.Join("", _bitBuffer.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
    }

    public BitVector Not()
    {
        var newBitBuffer = new byte[BufferLength];

        for (var i = 0; i < BufferLength; i++) newBitBuffer[i] = (byte)~_bitBuffer[i];

        return new BitVector(newBitBuffer, BitLength);
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