namespace Hazelnut.Nokdu;

internal static class ForEachEnumerable
{
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> body)
    {
        foreach (var obj in enumerable)
            body(obj);
    }
}