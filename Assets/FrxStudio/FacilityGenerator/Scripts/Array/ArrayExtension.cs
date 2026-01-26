namespace FrxStudio
{
    /// <summary>
    /// The static utils to array and 2d array
    /// </summary>
    public static class ArrayExtension
    {
        /* I stopped using this, but it might come in handy in the future, or you might find it useful :)
        public static bool InBounds<T>(this T[] array, int x) =>
            x >= 0 && x < array.Length;
        */

        public static bool InBounds<T>(this T[,] array, int x, int y) =>
            x >= 0 && x < array.GetLength(0) &&
            y >= 0 && y < array.GetLength(1);
    }
}
