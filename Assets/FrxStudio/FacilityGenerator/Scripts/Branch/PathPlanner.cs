using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FrxStudio.Generator
{
    /// <summary>
    /// Planner of path
    /// </summary>
    public class PathPlanner : IGizmoDrawable
    {
        private readonly Grid grid;
        private readonly LeafGenerator leafGenerator;
        private readonly Pathfinder pathfinder;

        private readonly Dictionary<CellPosition, BranchNode> pathMarkers = new();

        public PathPlanner(Grid grid, LeafGenerator leafGenerator, Pathfinder pathfinder)
        {
            this.grid = grid;
            this.leafGenerator = leafGenerator;

            this.pathfinder = pathfinder;
        }

        public Dictionary<CellPosition, BranchNode> GetPathMarkers() => pathMarkers;

        /// <summary>
        /// Mark paths
        /// </summary>
        public bool MarkAllPaths(bool logging = false)
        {
            var leaves = leafGenerator.PlacedLeafs?.ToArray();

            if (leaves == null || leaves.Length <= 1)
                return true;

            var remaining = new List<CellPosition>(leaves);
            var connected = new List<CellPosition>
            {
                remaining[0]
            };

            remaining.RemoveAt(0);

            // connect leafs mst
            while (remaining.Count > 0)
            {
                var (bestRemIdx, bestConIdx) = BranchExtension.GetClosestPair(remaining, connected, grid);

                if (bestRemIdx == -1)
                {
                    if (logging)
                        Debug.LogError("[Generator]: BranchGenerator: Failed to find pair to connect");

                    return false;
                }

                var fromLeaf = connected[bestConIdx];
                var toLeaf = remaining[bestRemIdx];

                var (fromStart, toStart) = BranchExtension.GetPathStartPositions(fromLeaf, toLeaf, grid);

                if (fromStart == CellPosition.Invalid || toStart == CellPosition.Invalid)
                {
                    if (logging)
                        Debug.LogError($"[Generator]: Cannot get start positions for path");

                    return false;
                }

                var pathEnd = pathfinder.GetPath(fromStart, toStart);

                if (pathEnd == null)
                {
                    if (logging)
                        Debug.LogError($"[Generator]: No path found from {fromStart} to {toStart}");

                    return false;
                }

                MarkPathExits(pathEnd, fromLeaf, toLeaf);

                connected.Add(remaining[bestRemIdx]);
                remaining.RemoveAt(bestRemIdx);
            }

            return true;
        }

        /// <summary>
        /// Mark path exits
        /// </summary>
        public void MarkPathExits(PathfindNode pathEnd, CellPosition fromLeaf, CellPosition toLeaf)
        {
            var nodes = BranchExtension.GetPathToList(pathEnd);

            for (var i = 0; i < nodes.Count; i++)
            {
                var pos = nodes[i].Cell.Position;

                if (grid.GetCell(pos).IsBusy && grid.GetCell(pos).Owner != null)
                    continue;

                if (!pathMarkers.ContainsKey(pos))
                    pathMarkers[pos] = new BranchNode { Position = pos };

                var marker = pathMarkers[pos];

                if (i > 0)
                    marker.Exits = marker.Exits.Add(GridExtension.GetDirectionBetween(grid, pos, nodes[i - 1].Cell.Position));

                if (i < nodes.Count - 1)
                    marker.Exits = marker.Exits.Add(GridExtension.GetDirectionBetween(grid, pos, nodes[i + 1].Cell.Position));
            }

            if (nodes.Count > 0)
            {
                MarkLeafConnection(nodes[0].Cell.Position, toLeaf);
                MarkLeafConnection(nodes[^1].Cell.Position, fromLeaf);
            }
        }

        private void MarkLeafConnection(CellPosition pathPos, CellPosition leafPos)
        {
            var dir = GridExtension.GetDirectionBetween(grid, pathPos, leafPos);

            if (!pathMarkers.ContainsKey(pathPos))
                pathMarkers[pathPos] = new BranchNode { Position = pathPos };

            pathMarkers[pathPos].Exits = pathMarkers[pathPos].Exits.Add(dir);
        }

        public void DrawGizmo()
        {

        }
    }
}
