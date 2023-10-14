namespace MafiaLib;

public static class Extensions
{
    public static void Deconstruct<T>(this IEnumerable<T> enumerable, out T first, out T second)
    {
        first = enumerable.Any() ? enumerable.First() : default;
        second = enumerable.Skip(1).FirstOrDefault();
    }
}