using System;
using System.Collections.Generic;

namespace game
{
    class Utils
    {
        public class IgnoreList<T> : List<T>
        {
            new public void Add(T item) { }
            new public void Remove(T item) { }
            new public void Clear() { }
        }
    }

    static class StringExtensions
    {
        public static string Capitalize(this string str) =>
        str switch
        {
            null => throw new ArgumentNullException(nameof(str)),
            "" => throw new ArgumentException(
                $"{nameof(str)} cannot be empty", nameof(str)
            ),
            _ => string.Concat(str[0].ToString().ToUpper(), str.AsSpan(1))
        };
    }

    static class ListExtension
    {
        public static void AddAndForget<T>(this List<T> list, T item, int maxElements)
        {
            if (list.Count >= maxElements)
                list.RemoveRange(0, maxElements - list.Count + 1);
            list.Add(item);
        }
    }
}
