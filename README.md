# BitVector

The BitVector struct provides a lightweight, memory-efficient, and high-performance solution for working with individual bits within your .NET applications. With its versatile API and optimized bitwise operations, the BitVector struct is the ideal choice for scenarios where memory conservation and efficient bit manipulation are essential.

## Key Features

Efficient Bit Storage: The BitVector struct employs a memory-efficient approach by utilizing a single bit per item. This efficient storage mechanism ensures minimal memory usage, making it perfect for scenarios dealing with large datasets.

Immutable Design: Every operation on the BitVector struct generates a new instance, ensuring the immutability of the data. This design prevents unintentional modifications and enhances data integrity.

Broad Bit Range Support: The BitVector struct supports a wide range of bit lengths, accommodating requirements ranging from small to very large bit sequences.

## Operations

The BitVector struct offers a comprehensive set of bitwise operations designed for optimal performance:

Perform logical AND, OR, XOR, and NOT operations efficiently.
Utilize familiar bitwise operators (&, |, ^, and ~) to execute these operations seamlessly.
Simple Usage

``` csharp
BitVector bits = new BitVector(16); // Initialize with 16 bits
bits = bits.SetAll(true); // Set all bits to true
BitVector result = bits | new BitVector(16); // Perform OR operation
string binaryString = bits.ToBinaryString(); // Convert to binary string
```

## A Step Beyond BitArray

While the built-in BitArray class in .NET offers basic bit manipulation capabilities, the BitVector struct stands out with its memory efficiency, performance optimization, and user-friendly API. Whether you're working on cryptography, compression algorithms, or any application requiring precise bit control, the BitVector struct is your go-to solution.

## Installation

Get started with BitVector by installing it from NuGet using the following command:

``` shell
Install-Package BitVectorSharp
```

## License

MIT