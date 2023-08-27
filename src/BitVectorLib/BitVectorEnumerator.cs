using System.Collections;

namespace BitVectorLib;

public struct BitVectorEnumerator : IEnumerator<bool> 
{
    private readonly BitVector _bitVector;
    private long _currentIndex;

    public BitVectorEnumerator(BitVector bitVector)
    {
        _bitVector = bitVector;
        _currentIndex = -1;
    }

    public bool MoveNext()
    {
        if (_currentIndex + 1 < _bitVector.BitLength)
        {
            _currentIndex++;
            return _bitVector.GetBit(_currentIndex);
        }
        return false;
    }

    public void Reset()
    {
        _currentIndex = -1;
    }

    object IEnumerator.Current => Current;

    public bool Current => _bitVector.GetBit(_currentIndex);
 
    public void Dispose()
    {
    }
}