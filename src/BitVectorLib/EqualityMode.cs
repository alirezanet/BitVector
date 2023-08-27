namespace BitVectorLib;

/// <summary>
///     Specifies different modes for comparing equality between <see cref="BitVector" /> instances.
/// </summary>
public enum EqualityMode
{
    /// <summary>
    ///     Compares both length and data of the <see cref="BitVector" />.
    /// </summary>
    CompareLengthAndData,

    /// <summary>
    ///     Compares only the data of the <see cref="BitVector" />.
    /// </summary>
    CompareDataOnly
}