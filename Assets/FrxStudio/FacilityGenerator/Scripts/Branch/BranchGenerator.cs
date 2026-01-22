using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FrxStudio.Generator
{
    public class BranchGenerator : IGizmoDrawable
    {
        private class PathNode
        {
            public CellPosition Position;
            public ExitsMask Exits = ExitsMask.Empty;
        }

        private readonly Pathinder pathfinder;
        private readonly Grid grid;
        private readonly LeafGenerator leafGenerator;
        private readonly ScriptableGeneratorPreset preset;
        private readonly FacilityGenerator generator;
        private readonly System.Random random;

        private readonly Dictionary<CellPosition, PathNode> pathMarkers = new();
        private PathfindNode debugPath;

        public BranchGenerator(
            Grid grid, LeafGenerator leafGenerator,
            ScriptableGeneratorPreset preset,
            FacilityGenerator generator,
            System.Random random)
        {
            this.grid = grid;
            this.leafGenerator = leafGenerator;
            this.preset = preset;
            this.generator = generator;
            this.random = random;

            pathfinder = new(grid);
        }

        public bool ConnectBranch()
        {
            var corridorRooms = preset.Rooms.OfType<ScriptableBranchRoom>().ToArray();
            var interRooms = preset.Rooms.OfType<ScriptableInterRoom>().ToArray();

            if (corridorRooms.Length == 0 || interRooms.Length == 0)
            {
                Debug.LogError("[Generator]: Missing corridor or intersection rooms");
                return false;
            }

            // step 1: find path and mark exits WITHOUT spawning room
            if (!MarkAllPaths())
                return false;

            // step 2: spawn rooms in paths
            if (!SpawnRoomsOnPaths(corridorRooms, interRooms))
                return false;

            return true;
        }

        private bool MarkAllPaths()
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
                // find closets pair: connected list - not connected list
                var (bestRemIdx, bestConIdx) = BranchExtension.GetClosestPair(remaining, connected, grid);

                if (bestRemIdx == -1)
                {
                    Debug.LogError("[Generator]: BranchGenerator: Failed to find pair to connect");
                    return false;
                }

                var fromLeaf = connected[bestConIdx];
                var toLeaf = remaining[bestRemIdx];

                var (fromStart, toStart) = BranchExtension.GetPathStartPositions(
                    fromLeaf, toLeaf, grid);

                if (fromStart == CellPosition.Invalid || toStart == CellPosition.Invalid)
                {
                    Debug.LogError($"[Generator]: Cannot get start positions for path");
                    return false;
                }

                // find the path
                var pathEnd = pathfinder.GetPath(fromStart, toStart);

                if (pathEnd == null)
                {
                    Debug.LogError($"[Generator]: No path found from {fromStart} to {toStart}");
                    return false;
                }

                debugPath = pathEnd;

                // mark exits each cell path
                MarkPathExits(pathEnd, fromLeaf, toLeaf);

                connected.Add(remaining[bestRemIdx]);
                remaining.RemoveAt(bestRemIdx);
            }

            return true;
        }

        private void MarkPathExits(PathfindNode pathEnd, CellPosition fromLeaf, CellPosition toLeaf)
        {
            var nodes = BranchExtension.GetPathToList(pathEnd);

            for (var i = 0; i < nodes.Count; i++)
            {
                var pos = nodes[i].Cell.Position;

                // skip already spawned room
                if (grid.GetCell(pos).IsBusy && grid.GetCell(pos).Owner != null)
                    continue;

                if (!pathMarkers.ContainsKey(pos))
                    pathMarkers[pos] = new PathNode { Position = pos };

                var marker = pathMarkers[pos];

                // mark the exit to next node
                if (i > 0)
                    marker.Exits = marker.Exits.Add(
                        GridExternal.GetDirectionBetween(grid, pos, nodes[i - 1].Cell.Position));

                // mark the exit to child node
                if (i < nodes.Count - 1)
                    marker.Exits = marker.Exits.Add(
                        GridExternal.GetDirectionBetween(grid, pos, nodes[i + 1].Cell.Position));
            }

            // mark connections with leafs
            if (nodes.Count > 0)
            {
                MarkLeafConnection(nodes[0].Cell.Position, toLeaf);
                MarkLeafConnection(nodes[^1].Cell.Position, fromLeaf);
            }
        }

        // mark connection from path to leaf
        private void MarkLeafConnection(CellPosition pathPos, CellPosition leafPos)
        {
            var dir = GridExternal.GetDirectionBetween(grid, pathPos, leafPos);

            if (!pathMarkers.ContainsKey(pathPos))
                pathMarkers[pathPos] = new PathNode { Position = pathPos };

            pathMarkers[pathPos].Exits = pathMarkers[pathPos].Exits.Add(dir);
        }

        private bool SpawnRoomsOnPaths(ScriptableBranchRoom[] corridorRooms, ScriptableInterRoom[] interRooms)
        {
            var hallways = corridorRooms.Where(r => r.Shape == BranchShape.Hallway).ToArray();
            var cShapes = corridorRooms.Where(r => r.Shape == BranchShape.CShape).ToArray();
            var tShapes = interRooms.Where(r => r.Shape == InterShape.TShape).ToArray();
            var crosses = interRooms.Where(r => r.Shape == InterShape.CrossShape).ToArray();

            if (hallways.Length == 0 || cShapes.Length == 0 || tShapes.Length == 0 || crosses.Length == 0)
            {
                Debug.LogError("[Generator]: Missing required room shapes");
                return false;
            }

            foreach (var kvp in pathMarkers)
            {
                var marker = kvp.Value;

                // skip already busy cells
                if (grid.GetCell(marker.Position).IsBusy && grid.GetCell(marker.Position).Owner != null)
                    continue;

                // get room type by exits
                var (roomToSpawn, spawnDirection) = DetermineRoomType(
                    marker, marker.Exits.Count, hallways, cShapes, tShapes, crosses);

                if (roomToSpawn == null)
                {
                    Debug.LogError($"[Generator]: BranchGenerator: Failed to determine room for {marker.Position}");
                    return false;
                }

                if (!generator.Spawn(roomToSpawn, marker.Position, spawnDirection))
                {
                    Debug.LogError($"[Generator]: BranchGenerator: Failed to spawn {roomToSpawn.name} at {marker.Position}");
                    return false;
                }
            }

            return true;
        }

        // returned room type and direction by exits
        private (ScriptableRoomBase room, Direction direction) DetermineRoomType(
            PathNode marker, int exitCount,
            ScriptableBranchRoom[] hallways, ScriptableBranchRoom[] cShapes,
            ScriptableInterRoom[] tShapes, ScriptableInterRoom[] crosses)
        {
            return exitCount switch
            {
                2 => GetCorridorRoom(marker, hallways, cShapes),
                3 => (tShapes[random.Next(tShapes.Length)], BranchExtension.GetTShapeDirection(marker.Exits)),
                4 => (crosses[random.Next(crosses.Length)], Direction.Up),
                _ => (null, Direction.Up)
            };
        }

        // returned branch type: hallway or curve ( CShape )
        private (ScriptableRoomBase room, Direction direction) GetCorridorRoom(
            PathNode marker,
            ScriptableBranchRoom[] hallways,
            ScriptableBranchRoom[] cShapes)
        {
            var exits = marker.Exits;

            if ((exits.Has(Direction.Up) && exits.Has(Direction.Down)) ||
                (exits.Has(Direction.Left) && exits.Has(Direction.Right)))
                return (hallways[random.Next(hallways.Length)],
                    exits.Has(Direction.Up) ? Direction.Up : Direction.Right);

            // CShape
            return (cShapes[random.Next(cShapes.Length)],
                BranchExtension.GetCShapeDirection(exits));
        }

        #region Debug

        public void DrawGizmo()
        {
            if (debugPath == null)
                return;

            var current = debugPath;

            while (current != null)
            {
                Gizmos.color = new(0, 1, 0, 0.35f);
                Gizmos.DrawCube(current.Cell.Position.WorldPosition, preset.CellSize * Vector3.one);
                current = current.Parent;
            }
        }

        #endregion
    }
}