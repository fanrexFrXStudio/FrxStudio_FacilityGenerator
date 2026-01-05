using System;
using UnityEngine;

namespace FrxStudio.Generator
{
    public readonly struct Cell
    {
        public readonly CellPosition Position;
        public readonly CellOwner Owner;

        public bool IsBusy => Owner.Instance != null;

        public static Cell Invalid => new(-1, -1, Vector3.zero);

        public Cell(int x, int y, Vector3 worldPosition)
        {
            Position = new(x, y, worldPosition);
            Owner = new();
        }

        public readonly void SetOwner(GameObject instance, Direction instanceDirection)
        {
            Owner.SetOwner(instance, instanceDirection);
        }

        #region Operator

        public override bool Equals(object obj)
        {
            if (obj is Cell cell)
            {
                if (cell.Position == Position)
                    return true;
            }

            return false;
        }

        public override int GetHashCode() => Position.GetHashCode();

        public static bool operator ==(Cell origin, Cell target) => origin.Equals(target);
        public static bool operator !=(Cell origin, Cell target) => !origin.Equals(target);

        #endregion
    }
}