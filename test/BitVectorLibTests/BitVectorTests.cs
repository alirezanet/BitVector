using BitVectorLib;

namespace BitVectorLibTests;

public class BitVectorTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(5, 1)]
    [InlineData(8, 1)]
    [InlineData(9, 2)]
    [InlineData(17, 3)]
    [InlineData(32, 4)]
    [InlineData(33, 5)]
    [InlineData(0x3FFFFFE38, 0x7FFFFFC7)] // Max supported value
    public void New_InitializingNewVectorUsingLength_ShouldAllocateMinimumAmountOfBytes(long length, long bufferLength)
    {
        // act
        var bv = new BitVector(length);

        // assert
        Assert.Equal(bufferLength, bv.BufferLength);
        Assert.True(length / 8 <= bv.BufferLength); // make sure we're only allocating 1 bit     
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(92)]
    [InlineData(127)]
    [InlineData(324)]
    [InlineData(33321)]
    [InlineData(45320)]
    public void New_InitializingNewVectorUsingDefaultTrueValue_ShouldSetAllBitsToTrue(long length)
    {
        // act
        var bv = new BitVector(length, true);

        // assert
        Assert.Equal(new string('1', (int)bv.BitLength).PadLeft(bv.BufferLength * 8, '0'), bv.ToString());
        Assert.True(bv.All(bit => bit));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(92)]
    [InlineData(127)]
    [InlineData(324)]
    [InlineData(33321)]
    [InlineData(45320)]
    public void GetBit_WhenAllBitsAreTrue_ShouldReturnTrue(long length)
    {
        // arrange
        var bv = new BitVector(length, true);

        for (var i = 0; i < length; i++)
        {
            // act & assert
            Assert.True(bv.GetBit(i));
        }
    }

    [Theory]
    [InlineData("0")]
    [InlineData("00")]
    [InlineData("01")]
    [InlineData("11")]
    [InlineData("100")]
    [InlineData("00001010100")]
    [InlineData("11001010100")]
    public void New_InitializeNewBitVectorUsingBinaryString(string binaryString)
    {
        // Act
        var bv = new BitVector(binaryString);

        // Assert
        var expectedLength = binaryString.Length;
        Assert.Equal(expectedLength, bv.BitLength);

        var expectedBufferLength = (expectedLength + 7) / 8;
        Assert.Equal(expectedBufferLength, bv.BufferLength);

        var expectedBinary = binaryString.PadLeft(expectedBufferLength * 8, '0');
        Assert.Equal(expectedBinary, bv.ToString());

        for (var i = 0; i < expectedLength; i++)
        {
            var bitIndexFromRight = expectedLength - 1 - i;
            Assert.Equal(binaryString[i] == '1', bv.GetBit(bitIndexFromRight));
        }
    }
}