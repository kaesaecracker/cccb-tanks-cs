using System.Collections;

namespace TanksServer.Helpers;

internal sealed class FixedSizeBitFieldView(Memory<byte> data) : IList<bool>
{
    public int Count => data.Length * 8;
    public bool IsReadOnly => false;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<bool> GetEnumerator()
    {
        return Enumerable().GetEnumerator();

        IEnumerable<bool> Enumerable()
        {
            for (var i = 0; i < Count; i++)
                yield return this[i];
        }
    }

    public void Clear()
    {
        var span = data.Span;
        for (var i = 0; i < data.Length; i++)
            span[i] = 0;
    }

    public void CopyTo(bool[] array, int arrayIndex)
    {
        for (var i = 0; i < Count && i + arrayIndex < array.Length; i++)
            array[i + arrayIndex] = this[i];
    }

    private static (int byteIndex, int bitInByteIndex) GetIndexes(int bitIndex)
    {
        var byteIndex = bitIndex / 8;
        var bitInByteIndex = 7 - bitIndex % 8;
        return (byteIndex, bitInByteIndex);
    }

    public bool this[int bitIndex]
    {
        get
        {
            var (byteIndex, bitInByteIndex) = GetIndexes(bitIndex);
            var bitInByteMask = (byte)(1 << bitInByteIndex);
            return (data.Span[byteIndex] & bitInByteMask) != 0;
        }

        set
        {
            var (byteIndex, bitInByteIndex) = GetIndexes(bitIndex);
            var bitInByteMask = (byte)(1 << bitInByteIndex);

            if (value)
            {
                data.Span[byteIndex] |= bitInByteMask;
            }
            else
            {
                var withoutBitMask = (byte)(ushort.MaxValue ^ bitInByteMask);
                data.Span[byteIndex] &= withoutBitMask;
            }
        }
    }

    public void Add(bool item) => throw new NotSupportedException();
    public bool Contains(bool item) => throw new NotSupportedException();
    public bool Remove(bool item) => throw new NotSupportedException();
    public int IndexOf(bool item) => throw new NotSupportedException();
    public void Insert(int index, bool item) => throw new NotSupportedException();
    public void RemoveAt(int index) => throw new NotSupportedException();
}
