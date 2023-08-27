using System.Collections;

namespace BitVectorLib;

/// <summary>
///     Enumerates over the bits in a <see cref="BitVector" />.
/// </summary>
public struct BitVectorEnumerator : IEnumerator<bool>
{
    private readonly BitVector _bitVector;
    private long _currentIndex;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BitVectorEnumerator" /> struct.
    /// </summary>
    /// <param name="bitVector">The <see cref="BitVector" /> to enumerate.</param>
    public BitVectorEnumerator(BitVector bitVector)
    {
        _bitVector = bitVector;
        _currentIndex = -1;
    }

    /// <summary>
    ///     Advances the enumerator to the next bit of the <see cref="BitVector" />.
    /// </summary>
    /// <returns>
    ///     True if the enumerator was successfully advanced to the next bit; false if the enumerator has reached the end
    ///     of the <see cref="BitVector" />.
    /// </returns>
    public bool MoveNext()
    {
        if (_currentIndex + 1 < _bitVector.BitLength)
        {
            _currentIndex++;
            return _bitVector.GetBit(_currentIndex);
        }

        return false;
    }

    /// <summary>
    ///     Sets the enumerator to its initial position, which is before the first bit of the <see cref="BitVector" />.
    /// </summary>
    public void Reset()
    {
        _currentIndex = -1;
    }

    /// <summary>
    ///     Gets the current bit at the enumerator's current position.
    /// </summary>
    object IEnumerator.Current => Current;

    /// <summary>
    ///     Gets the current bit at the enumerator's current position.
    /// </summary>
    public bool Current => _bitVector.GetBit(_currentIndex);

    /// <summary>
    ///     Releases all resources used by the <see cref="BitVectorEnumerator" />.
    /// </summary>
    public void Dispose()
    {
    }
}