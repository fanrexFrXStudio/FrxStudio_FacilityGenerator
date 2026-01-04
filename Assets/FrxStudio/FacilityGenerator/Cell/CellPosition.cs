using UnityEngine;

namespace FrxStudio.Generator
{
    public struct CellPosition
    {
        public readonly int X, Y;
        public readonly Vector3 WorldPosition;

        public CellPosition(int x, int y, Vector3 worldPosition)
        {
            X = x;
            Y = y;
            WorldPosition = worldPosition;
        }
    }
}