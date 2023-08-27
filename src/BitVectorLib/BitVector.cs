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

    /// <summary>
    ///     Initializes a new instance of the <c>BitVector</c> struct with the specified bit length.
    ///     The bit vector is initially filled with zeros.
    /// </summary>
    /// <param name="bitLength">The length of the bit vector in bits.</param>
    public BitVector(long bitLength)
    {
        checked
        {
            BufferLength = (int)((bitLength + 7) / 8);
            _bitBuffer = new byte[BufferLength];
            BitLength = bitLength;
        }
    }

    /// <summary>
    ///     Initializes a new instance of the <c>BitVector</c> struct using the provided byte buffer and bit length.
    /// </summary>
    /// <param name="bitBuffer">The byte buffer containing the bit values.</param>
    /// <param name="bitLenght">The length of the bit vector in bits.</param>
    public BitVector(byte[] bitBuffer, long bitLenght)
    {
        _bitBuffer = bitBuffer;
        BitLength = bitLenght;
        BufferLength = bitBuffer.Length;
    }

    /// <summary>
    ///     Initializes a new instance of the <c>BitVector</c> struct with the specified bit length and sets all bits to the
    ///     specified default value.
    /// </summary>
    /// <param name="bitLength">The length of the bit vector in bits.</param>
    /// <param name="defaultValue">The default value to set for all bits.</param>
    public BitVector(long bitLength, bool defaultValue)
    {
        this = new BitVector(bitLength);
        InternalSetAll(defaultValue);
    }

    /// <summary>
    ///     Initializes a new instance of the <c>BitVector</c> struct with the specified bit length and sets specific
    ///     bits at the specified non-zero indexes.
    /// </summary>
    /// <param name="bitLength">The length of the bit vector in bits.</param>
    /// <param name="nonZeroIndexes">An array of non-zero bit indexes to set.</param>
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

    /// <summary>
    ///     Initializes a new instance of the <c>BitVector</c> struct from a binary string representation.
    /// </summary>
    /// <param name="binaryString">A binary string representing the bit vector.</param>
    /// <remarks>The binary string should consist of '0' and '1' characters only.</remarks>
    /// <exception cref="ArgumentException">Thrown if the binary string is null, empty, or contains invalid characters.</exception>
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

    /// <summary>
    ///     Returns an enumerator that iterates through the <c>BitVector</c>.
    ///     WARNING: The enumerator is slow and may involve boxing.
    /// </summary>
    /// <returns>An enumerator instance for the <c>BitVector</c>.</returns>
    public BitVectorEnumerator GetEnumerator()
    {
        return new BitVectorEnumerator(this);
    }

    /// <inheritdoc />
    IEnumerator<bool> IEnumerable<bool>.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    ///     Sets all bits in the <c>BitVector</c> to the specified value.
    /// </summary>
    /// <param name="value">The value to set all bits to.</param>
    /// <returns>A new <c>BitVector</c> with all bits set to the specified value.</returns>
    public BitVector SetAll(bool value)
    {
        return new BitVector(BitLength, value);
    }

    /// <summary>
    ///     Converts the <c>BitVector</c> to a byte array.
    /// </summary>
    /// <returns>A byte array representing the contents of the <c>BitVector</c>.</returns>
    public byte[] ToByteArray()
    {
        return _bitBuffer.ToArray();
    }

    /// <summary>
    ///     Copies the contents of the <c>BitVector</c> to the specified buffer, starting at the specified index.
    /// </summary>
    /// <param name="buffer">The destination buffer.</param>
    /// <param name="index">The starting index in the destination buffer.</param>
    public void CopyTo(byte[] buffer, int index)
    {
        _bitBuffer.CopyTo(buffer, index);
    }

    /// <summary>
    ///     Creates a new <c>BitVector</c> with the specified bits set to the given value.
    /// </summary>
    /// <param name="value">The value to set the specified bits to.</param>
    /// <param name="bitIndexes">The indexes of the bits to set.</param>
    /// <returns>A new <c>BitVector</c> with the specified bits set to the given value.</returns>
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

    /// <summary>
    ///     Gets the byte value corresponding to the given boolean value.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    /// <returns>The byte value corresponding to the given boolean value.</returns>
    private static byte GetValue(bool value)
    {
        return (byte)(value ? 0xFF : 0x00);
    }

    /// <summary>
    ///     Sets all bits in the <c>BitVector</c> to the specified value.
    /// </summary>
    /// <param name="value">The value to set all bits to.</param>
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

    /// <summary>
    ///     Performs a bitwise AND operation between this <c>BitVector</c> and another <c>BitVector</c>.
    /// </summary>
    /// <param name="value">The <c>BitVector</c> to perform the AND operation with.</param>
    /// <returns>A new <c>BitVector</c> resulting from the AND operation.</returns>
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

    /// <summary>
    ///     Performs a bitwise OR operation between this <c>BitVector</c> and another <c>BitVector</c>.
    /// </summary>
    /// <param name="value">The <c>BitVector</c> to perform the OR operation with.</param>
    /// <returns>A new <c>BitVector</c> resulting from the OR operation.</returns>
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

    /// <summary>
    ///     Performs a bitwise XOR operation between this <c>BitVector</c> and another <c>BitVector</c>.
    /// </summary>
    /// <param name="value">The <c>BitVector</c> to perform the XOR operation with.</param>
    /// <returns>A new <c>BitVector</c> resulting from the XOR operation.</returns>
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

    /// <summary>
    ///     Shifts the bits of the <c>BitVector</c> to the left by the specified number of positions.
    /// </summary>
    /// <param name="count">The number of positions to shift the bits to the left.</param>
    /// <returns>A new <c>BitVector</c> with the shifted bits.</returns>
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

    /// <summary>
    ///     Shifts the bits of the <c>BitVector</c> to the right by the specified number of positions.
    /// </summary>
    /// <param name="count">The number of positions to shift the bits to the right.</param>
    /// <returns>A new <c>BitVector</c> with the shifted bits.</returns>
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

    /// <summary>
    ///     Counts the total number of set (1) bits in the BitVector.
    /// </summary>
    /// <returns>The count of set bits.</returns>
    public long CountSetBits()
    {
        return _bitBuffer.Aggregate<byte, long>(0, (current, byteValue) => current + CountSetBitsInByte(byteValue));
    }

    /// <summary>
    ///     Returns the index of the first set (1) bit in the BitVector.
    /// </summary>
    /// <returns>The index of the first set bit, or -1 if no set bit is found.</returns>
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

    /// <summary>
    ///     Returns the value of the bit at the specified index.
    /// </summary>
    /// <param name="bitIndex">The index of the bit to retrieve.</param>
    /// <returns>True if the bit is set (1), false if it's not set (0).</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the provided bit index is out of range.</exception>
    public bool GetBit(long bitIndex)
    {
        if (bitIndex >= BitLength)
            throw new ArgumentOutOfRangeException(nameof(bitIndex), "Bit index is out of range.");

        var byteIndex = BufferLength - 1 - bitIndex / 8;
        var bitOffset = (int)bitIndex % 8;

        var mask = (byte)(1 << bitOffset);
        return (_bitBuffer[byteIndex] & mask) != 0;
    }

    /// <summary>
    ///     Counts the number of set (1) bits in a given byte value.
    /// </summary>
    /// <param name="value">The byte value to count set bits in.</param>
    /// <returns>The count of set bits in the byte.</returns>
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

    /// <summary>
    ///     Generates a hash code for the current BitVector instance.
    /// </summary>
    /// <returns>The hash code for the BitVector.</returns>
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

    /// <summary>
    ///     Determines whether the current BitVector instance is equal to another BitVector using the specified equality mode.
    /// </summary>
    /// <param name="obj">The object to compare with the current BitVector instance.</param>
    /// <param name="mode">The mode indicating which equality comparison to perform.</param>
    /// <returns>true if the specified BitVector is equal to the current BitVector; otherwise, false.</returns>
    public bool Equals(BitVector other, EqualityMode mode)
    {
        return mode switch
        {
            EqualityMode.CompareLengthAndData => EqualsLengthAndData(other),
            EqualityMode.CompareDataOnly => EqualsDataOnly(other),
            _ => throw new ArgumentException("Invalid equality mode.", nameof(mode))
        };
    }

    /// <summary>
    ///     Compares the current BitVector instance to another BitVector, considering both length and data.
    /// </summary>
    /// <param name="other">The BitVector to compare with the current BitVector instance.</param>
    /// <returns>true if the specified BitVector is equal to the current BitVector; otherwise, false.</returns>
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

    /// <summary>
    ///     Compares the current BitVector instance to another BitVector, considering data only.
    /// </summary>
    /// <param name="other">The BitVector to compare with the current BitVector instance.</param>
    /// <returns>true if the specified BitVector is equal to the current BitVector; otherwise, false.</returns>
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

    /// <summary>
    ///     Determines whether the current BitVector instance is equal to another BitVector.
    /// </summary>
    /// <param name="other">The BitVector to compare with the current BitVector instance.</param>
    /// <returns>true if the specified BitVector is equal to the current BitVector; otherwise, false.</returns>
    public bool Equals(BitVector other)
    {
        // Default to comparing both length and data
        return EqualsLengthAndData(other);
    }

    /// <summary>
    ///     Determines whether two BitVector instances are equal.
    /// </summary>
    /// <param name="left">The first BitVector to compare.</param>
    /// <param name="right">The second BitVector to compare.</param>
    /// <returns>true if the two BitVector instances are equal; otherwise, false.</returns>
    public static bool operator ==(BitVector left, BitVector right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Determines whether two BitVector instances are not equal.
    /// </summary>
    /// <param name="left">The first BitVector to compare.</param>
    /// <param name="right">The second BitVector to compare.</param>
    /// <returns>true if the two BitVector instances are not equal; otherwise, false.</returns>
    public static bool operator !=(BitVector left, BitVector right)
    {
        return !(left == right);
    }


    /// <summary>
    ///     Returns a string representation of the BitVector.
    /// </summary>
    /// <returns>A string representation of the BitVector.</returns>
    public override string ToString()
    {
        return string.Join("", _bitBuffer.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
    }

    /// <summary>
    ///     Computes the bitwise NOT of the BitVector.
    /// </summary>
    /// <returns>A new BitVector representing the bitwise NOT of the original BitVector.</returns>
    public BitVector Not()
    {
        var newBitBuffer = new byte[BufferLength];

        for (var i = 0; i < BufferLength; i++)
            newBitBuffer[i] = (byte)~_bitBuffer[i];

        return new BitVector(newBitBuffer, BitLength);
    }


    /// <summary>
    ///     Computes the bitwise left shift of the BitVector by the specified amount.
    /// </summary>
    /// <param name="value">The BitVector to shift.</param>
    /// <param name="shiftAmount">The number of positions to shift the BitVector to the left.</param>
    /// <returns>A new BitVector representing the bitwise left shift of the original BitVector.</returns>
    public static BitVector operator <<(BitVector value, int shiftAmount)
    {
        return value.LeftShift(shiftAmount);
    }

    /// <summary>
    ///     Computes the bitwise right shift of the BitVector by the specified amount.
    /// </summary>
    /// <param name="value">The BitVector to shift.</param>
    /// <param name="shiftAmount">The number of positions to shift the BitVector to the right.</param>
    /// <returns>A new BitVector representing the bitwise right shift of the original BitVector.</returns>
    public static BitVector operator >> (BitVector value, int shiftAmount)
    {
        return value.RightShift(shiftAmount);
    }

    /// <summary>
    ///     Computes the bitwise AND of two BitVectors.
    /// </summary>
    /// <param name="left">The left operand BitVector.</param>
    /// <param name="right">The right operand BitVector.</param>
    /// <returns>A new BitVector representing the bitwise AND of the two input BitVectors.</returns>
    public static BitVector operator &(BitVector left, BitVector right)
    {
        return left.And(right);
    }

    /// <summary>
    ///     Computes the bitwise OR of two BitVectors.
    /// </summary>
    /// <param name="left">The left operand BitVector.</param>
    /// <param name="right">The right operand BitVector.</param>
    /// <returns>A new BitVector representing the bitwise OR of the two input BitVectors.</returns>
    public static BitVector operator |(BitVector left, BitVector right)
    {
        return left.Or(right);
    }

    /// <summary>
    ///     Computes the bitwise XOR of two BitVectors.
    /// </summary>
    /// <param name="left">The left operand BitVector.</param>
    /// <param name="right">The right operand BitVector.</param>
    /// <returns>A new BitVector representing the bitwise XOR of the two input BitVectors.</returns>
    public static BitVector operator ^(BitVector left, BitVector right)
    {
        return left.Xor(right);
    }

    /// <summary>
    ///     Computes the bitwise NOT of a BitVector.
    /// </summary>
    /// <param name="value">The BitVector to negate.</param>
    /// <returns>A new BitVector representing the bitwise NOT of the input BitVector.</returns>
    public static BitVector operator ~(BitVector value)
    {
        return value.Not();
    }
}