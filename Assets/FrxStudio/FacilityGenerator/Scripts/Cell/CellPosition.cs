using System;
using UnityEngine;

namespace FrxStudio.Generator
{
    public readonly struct CellPosition
    {
        public readonly int X, Y;
        public readonly Vector3 WorldPosition;

        public static CellPosition Invalid => new(-1, -1, Vector3.zero);

        public CellPosition(int x, int y, Vector3 worldPosition)
        {
            X = x;
            Y = y;
            WorldPosition = worldPosition;
        }

        #region Operator

        public override bool Equals(object obj)
        {
            if (obj is CellPosition position)
            {
                if (position.X == X && position.Y == Y)
                    return true;
            }

            return false;
        }

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public static bool operator ==(CellPosition origin, CellPosition target) => origin.Equals(target);
        public static bool operator !=(CellPosition origin, CellPosition target) => !origin.Equals(target);

        public override string ToString() => $"X: {X}, Y: {Y}";

        #endregion
    }
}