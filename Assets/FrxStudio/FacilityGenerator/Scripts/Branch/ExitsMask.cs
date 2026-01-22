using System;

namespace FrxStudio.Generator
{
    [Flags]
    public enum ExitBits : byte
    {
        None = 0,
        Up = 1 << 0,
        Left = 1 << 1,
        Right = 1 << 2,
        Down = 1 << 3
    }

    public class ExitsMask : IEquatable<ExitsMask>
    {
        private readonly ExitBits bits;

        public ExitsMask(ExitBits bits)
        {
            this.bits = bits;
        }

        public bool Has(Direction dir) =>
            (bits & ToBit(dir)) != 0;

        public int Count =>
            ((bits & ExitBits.Up) != 0 ? 1 : 0) +
            ((bits & ExitBits.Left) != 0 ? 1 : 0) +
            ((bits & ExitBits.Right) != 0 ? 1 : 0) +
            ((bits & ExitBits.Down) != 0 ? 1 : 0);

        public static ExitsMask Empty => new(ExitBits.None);
        public static ExitsMask Invalid => new(ExitBits.None);

        public ExitsMask Add(Direction dir) =>
            new(bits | ToBit(dir));

        private static ExitBits ToBit(Direction dir) => dir switch
        {
            Direction.Up => ExitBits.Up,
            Direction.Left => ExitBits.Left,
            Direction.Right => ExitBits.Right,
            Direction.Down => ExitBits.Down,
            _ => ExitBits.None
        };

        public bool Equals(ExitsMask other) => bits == other.bits;
        public override bool Equals(object obj) => obj is ExitsMask other && Equals(other);
        public override int GetHashCode() => (int)bits;

        public static bool operator ==(ExitsMask a, ExitsMask b) => a.bits == b.bits;
        public static bool operator !=(ExitsMask a, ExitsMask b) => a.bits != b.bits;
    }
}
