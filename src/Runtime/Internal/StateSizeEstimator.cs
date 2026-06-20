using System.Collections;
using System.Text;

namespace ModularityKit.Mutator.Runtime.Internal;

internal static class StateSizeEstimator
{
    private static readonly IReadOnlyDictionary<Type, int> PrimitiveTypeSizes = new Dictionary<Type, int>
    {
        [typeof(bool)] = sizeof(bool),
        [typeof(byte)] = sizeof(byte),
        [typeof(sbyte)] = sizeof(sbyte),
        [typeof(char)] = sizeof(char),
        [typeof(short)] = sizeof(short),
        [typeof(ushort)] = sizeof(ushort),
        [typeof(int)] = sizeof(int),
        [typeof(uint)] = sizeof(uint),
        [typeof(long)] = sizeof(long),
        [typeof(ulong)] = sizeof(ulong),
        [typeof(float)] = sizeof(float),
        [typeof(double)] = sizeof(double),
        [typeof(decimal)] = sizeof(decimal),
        [typeof(Guid)] = 16
    };

    public static long Estimate(object? state)
    {
        if (state is null)
            return 0;

        if (state is string text)
            return Encoding.UTF8.GetByteCount(text);

        if (TryEstimateArraySize(state, out var arraySize))
            return arraySize;

        return state is ICollection collection ? collection.Count : 0;
    }

    private static bool TryEstimateArraySize(object state, out long sizeInBytes)
    {
        sizeInBytes = 0;
        if (state is not Array array)
            return false;

        var elementType = state.GetType().GetElementType();
        if (elementType is null)
            return false;

        if (!PrimitiveTypeSizes.TryGetValue(elementType, out var elementSize))
            return false;

        sizeInBytes = array.LongLength * elementSize;
        return true;
    }
}
