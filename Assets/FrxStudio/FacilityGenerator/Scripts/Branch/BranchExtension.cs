using System.Collections.Generic;
using UnityEngine;

namespace FrxStudio.Generator
{
    public static class BranchExtension
    {
        #region Extra paths

        public static string GetPairKey(CellPosition from, CellPosition to)
        {
            var hash1 = from.GetHashCode();
            var hash2 = to.GetHashCode();

            if (hash1 > hash2)
                return $"{hash1}-{hash2}";

            return $"{hash2}-{hash1}";
        }


        #endregion

        public static (int remainingIndex, int connectedIndex) GetClosestPair(
            List<CellPosition> remaining,
            List<CellPosition> connected,
            Grid grid)
        {
            var bestRemIdx = -1;
            var bestConIdx = -1;
            var bestDistance = int.MaxValue;

            for (var i = 0; i < remaining.Count; i++)
            {
                for (var j = 0; j < connected.Count; j++)
                {
                    var distance = grid.GetManhattan(connected[j], remaining[i]);

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestRemIdx = i;
                        bestConIdx = j;
                    }
                }
            }

            return (bestRemIdx, bestConIdx);
        }

        public static (CellPosition from, CellPosition to) GetPathStartPositions(
            CellPosition fromLeaf,
            CellPosition toLeaf,
            Grid grid)
        {
            var fromCell = grid.GetCell(fromLeaf);
            var toCell = grid.GetCell(toLeaf);

            if (fromCell.Owner == null || toCell.Owner == null)
                return (CellPosition.Invalid, CellPosition.Invalid);

            var fromStart = GridExternal.GetNext(grid, fromLeaf, fromCell.Owner.InstanceDirection);
            var toStart = GridExternal.GetNext(grid, toLeaf, toCell.Owner.InstanceDirection);

            return (fromStart, toStart);
        }

        public static List<PathfindNode> GetPathToList(PathfindNode pathEnd)
        {
            var nodes = new List<PathfindNode>();
            var iteration = pathEnd;

            while (iteration != null)
            {
                nodes.Add(iteration);
                iteration = iteration.Parent;
            }

            return nodes;
        }

        // === МИГРАЦИЯ ===
        public static ExitsMask MarkExit(ExitsMask exits, Direction dir)
        {
            return exits.Add(dir);
        }

        // look at direction, where NOT have exit
        public static Direction GetTShapeDirection(ExitsMask exits)
        {
            if (!exits.Has(Direction.Left))
                return Direction.Left;

            if (!exits.Has(Direction.Right))
                return Direction.Right;

            if (!exits.Has(Direction.Down))
                return Direction.Down;

            return Direction.Up;
        }

        public static Direction GetCShapeDirection(ExitsMask exits)
        {
            if (exits.Has(Direction.Down) && exits.Has(Direction.Left))
                return Direction.Up;      // Down + Left

            if (exits.Has(Direction.Left) && exits.Has(Direction.Up))
                return Direction.Right;   // Left + Up

            if (exits.Has(Direction.Up) && exits.Has(Direction.Right))
                return Direction.Down;    // Up + Right

            if (exits.Has(Direction.Right) && exits.Has(Direction.Down))
                return Direction.Left;    // Right + Down

            Debug.LogWarning(
                "[Generator]: BranchExtension: GetCShapeDirection: Unknown exit pattern, using Direction.Up");

            return Direction.Up;
        }
    }
}