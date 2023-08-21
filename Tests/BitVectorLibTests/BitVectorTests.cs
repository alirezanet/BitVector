using BitVectorLib;

namespace BitVectorLibTests;

public class BitVectorTests
{
    
    
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
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
    }

}