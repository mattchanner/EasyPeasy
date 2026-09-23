using System.Collections;

namespace EasyPeasy.SourceGenerator;

/// <summary>
/// An immutable array with value equality, so generator models compare by content and the
/// incremental pipeline can skip regenerating unchanged interfaces.
/// </summary>
internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    private readonly T[]? items;

    public EquatableArray(T[] items) => this.items = items;

    public static EquatableArray<T> Empty { get; } = new([]);

    public int Length => items?.Length ?? 0;

    public T this[int index] => items![index];

    public bool Equals(EquatableArray<T> other) =>
        AsSpan().SequenceEqual(other.AsSpan());

    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            foreach (var item in items ?? [])
            {
                hash = (hash * 31) + (item?.GetHashCode() ?? 0);
            }

            return hash;
        }
    }

    public ReadOnlySpan<T> AsSpan() => items.AsSpan();

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)(items ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal static class EquatableArrayExtensions
{
    public static EquatableArray<T> ToEquatableArray<T>(this IEnumerable<T> source)
        where T : IEquatable<T> => new(source.ToArray());
}
