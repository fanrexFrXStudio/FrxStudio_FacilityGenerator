using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrxStudio.Generator
{
    public static class GridExternal
    {
        public static Direction[] CachedDirections
        {
            get
            {
                cachedDirections ??= (Direction[])Enum.GetValues(typeof(Direction));
                return cachedDirections;
            }
            set => cachedDirections = value;
        }

        private static Direction[] cachedDirections;

        public static CellPosition[] GetNeighbords(
            this Grid grid, CellPosition from,
            Func<CellPosition, bool> predicate = null)
        {
            var neighbords = new List<CellPosition>();

            foreach (var direction in CachedDirections)
            {
                var next = grid.GetNext(from, direction);

                if (next == CellPosition.Invalid)
                    continue;

                if (predicate != null)
                {
                    if (predicate(next))
                        neighbords.Add(next);

                    continue;
                }

                neighbords.Add(next);
            }

            return neighbords.ToArray();
        }

        public static CellPosition GetNext(this Grid grid, CellPosition from, Direction to)
        {
            (int x, int y) targetPos;

            targetPos = to switch
            {
                Direction.Up => new(0, 1),
                Direction.Down => new(0, -1),
                Direction.Right => new(1, 0),
                Direction.Left => new(-1, 0),
                // unreal
                _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
            };

            targetPos.x += from.X;
            targetPos.y += from.Y;

            var cell = grid.GetCell(targetPos.x, targetPos.y);

            if (cell == Cell.Invalid)
                return CellPosition.Invalid;

            return cell.Position;
        }

        public static CellPosition[] GetCells(this Grid grid, Func<CellPosition, bool> predicate = null)
        {
            var candidates = new List<CellPosition>();

            for (var x = 0; x < grid.CellsCountX; x++)
            {
                for (var y = 0; y < grid.CellsCountY; y++)
                {
                    var cellPosition = grid.GetCell(x, y).Position;

                    if (predicate(cellPosition))
                        candidates.Add(cellPosition);
                }
            }

            return candidates.ToArray();
        }

        public static CellPosition GetNearestEdgePosition(this Grid grid, CellPosition position)
        {
            if (position == CellPosition.Invalid)
                return CellPosition.Invalid;

            var leftDistance = position.X;
            var rightDistance = grid.CellsCountX - 1 - position.X;
            var downDistance = position.Y;
            var upDistance = grid.CellsCountY - 1 - position.Y;

            var min = Math.Min(Math.Min(leftDistance, rightDistance), Math.Min(downDistance, upDistance));

            if (min == leftDistance)
            {
                var cell = grid.GetCell(0, position.Y);
                return cell == Cell.Invalid ? CellPosition.Invalid : cell.Position;
            }

            if (min == rightDistance)
            {
                var cell = grid.GetCell(grid.CellsCountX - 1, position.Y);
                return cell == Cell.Invalid ? CellPosition.Invalid : cell.Position;
            }

            if (min == downDistance)
            {
                var cell = grid.GetCell(position.X, 0);
                return cell == Cell.Invalid ? CellPosition.Invalid : cell.Position;
            }

            var topCell = grid.GetCell(position.X, grid.CellsCountY - 1);
            return topCell == Cell.Invalid ? CellPosition.Invalid : topCell.Position;
        }

        public static Direction GetNearestEdgeInwardDirection(this Grid grid, CellPosition pos)
        {
            var width = grid.CellsCountX;
            var height = grid.CellsCountY;

            var distanceToLeft = pos.X;
            var distanceToRight = width - 1 - pos.X;
            var distanceToBottom = pos.Y;
            var distanceToTop = height - 1 - pos.Y;

            var min = Math.Min(
                Math.Min(distanceToLeft, distanceToRight),
                Math.Min(distanceToBottom, distanceToTop));

            if (min == distanceToLeft)
                return Direction.Right;   // inward from left border
            if (min == distanceToRight)
                return Direction.Left;    // inward from right border
            if (min == distanceToBottom)
                return Direction.Up;      // inward from down border

            return Direction.Down;        // inward from top border
        }

        public static Direction Rotate(
            this Grid _,
            Direction baseDir, Direction offset)
        {
            var result = ((int)baseDir + (int)offset) % CachedDirections.Length;
            return (Direction)result;
        }

        public static int GetManhattan(this Grid _, CellPosition from, CellPosition to) =>
            Math.Abs(from.X - to.X) +
            Math.Abs(from.Y - to.Y);

        public static Quaternion DirectionToEuler(this Grid _, Direction direction)
        {
            var rotation = direction switch
            {
                Direction.Up => 0,
                Direction.Down => 180,
                Direction.Right => 90,
                Direction.Left => 270,
                // unreal
                _ => throw new Exception()
            };

            return Quaternion.Euler(0, rotation, 0);
        }

        public static Direction GetRandomDirection(this Grid _, System.Random random) =>
            CachedDirections[random.Next(CachedDirections.Length)];

        public static Direction GetRandomDirection(this Grid grid, CellPosition pos, System.Random random)
        {
            var validDirections = new List<Direction>();

            foreach (var dir in CachedDirections)
            {
                var next = grid.GetNext(pos, dir);

                if (next != CellPosition.Invalid)
                    validDirections.Add(dir);
            }

            if (validDirections.Count == 0)
            {
                Debug.LogWarning("[Generator]: GridExternal: No valid direction");
                return Direction.Up;
            }

            return validDirections[random.Next(validDirections.Count)];
        }

        public static Direction GetDirectionBetween(this Grid _, CellPosition from, CellPosition to)
        {
            var directionX = to.X - from.X;
            var directionY = to.Y - from.Y;

            if (directionX > 0)
                return Direction.Right;
            if (directionX < 0)
                return Direction.Left;
            if (directionY > 0)
                return Direction.Up;
            if (directionY < 0)
                return Direction.Down;

            throw new ArgumentException("[Generator]: From and To positions are the same or not aligned on a single axis");
        }

        public static Direction GetRelativeDirection(
            this Grid _,
            CellPosition from, CellPosition to, Direction localDirection)
        {
            var directionX = to.X - from.X;
            var directionY = to.Y - from.Y;

            var horizontal = Math.Sign(directionX);
            var vertical = Math.Sign(directionY);

            return localDirection switch
            {
                Direction.Up => vertical != 0 ? (vertical > 0 ? Direction.Up : Direction.Down) : (horizontal > 0 ? Direction.Right : Direction.Left),
                Direction.Down => vertical != 0 ? (vertical > 0 ? Direction.Down : Direction.Up) : (horizontal > 0 ? Direction.Left : Direction.Right),
                Direction.Left => horizontal != 0 ? (horizontal > 0 ? Direction.Left : Direction.Right) : (vertical > 0 ? Direction.Up : Direction.Down),
                Direction.Right => horizontal != 0 ? (horizontal > 0 ? Direction.Right : Direction.Left) : (vertical > 0 ? Direction.Down : Direction.Up),
                _ => throw new Exception()
            };
        }
    }
}
