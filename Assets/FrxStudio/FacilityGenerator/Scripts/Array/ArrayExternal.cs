using System;

namespace FrxStudio
{
    public static class ArrayExternal
    {
        public static bool InBounds<T>(this T[] array, int x) =>
            x >= 0 && x < array.Length;

        public static bool InBounds<T>(this T[,] array, int x, int y) =>
            x >= 0 && x < array.GetLength(0) &&
            y >= 0 && y < array.GetLength(1);
    }
}
