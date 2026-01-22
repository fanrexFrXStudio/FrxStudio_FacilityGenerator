using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FrxStudio.Generator
{
    public class RoomSpawner
    {
        private readonly Grid grid;
        private readonly FacilityGenerator generator;
        private readonly ScriptableGeneratorPreset preset;
        private readonly System.Random random;

        public RoomSpawner(Grid grid, FacilityGenerator generator, ScriptableGeneratorPreset preset, System.Random random)
        {
            this.grid = grid;
            this.generator = generator;
            this.preset = preset;
            this.random = random;
        }

        public bool SpawnRoomsOnPaths(Dictionary<CellPosition, BranchNode> pathMarkers)
        {
            var corridorRooms = preset.Rooms.OfType<ScriptableBranchRoom>().ToArray();
            var interRooms = preset.Rooms.OfType<ScriptableInterRoom>().ToArray();

            if (corridorRooms.Length == 0 || interRooms.Length == 0)
            {
                Debug.LogError("[Generator]: Missing corridor or intersection rooms");
                return false;
            }

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

                if (grid.GetCell(marker.Position).IsBusy && grid.GetCell(marker.Position).Owner != null)
                    continue;

                var (roomToSpawn, spawnDirection) = DetermineRoomType(marker, marker.Exits.Count, hallways, cShapes, tShapes, crosses);

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

        private (ScriptableRoomBase room, Direction direction) DetermineRoomType(
            BranchNode marker, int exitCount,
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

        private (ScriptableRoomBase room, Direction direction) GetCorridorRoom(
            BranchNode marker,
            ScriptableBranchRoom[] hallways,
            ScriptableBranchRoom[] cShapes)
        {
            var exits = marker.Exits;

            if ((exits.Has(Direction.Up) && exits.Has(Direction.Down)) ||
                (exits.Has(Direction.Left) && exits.Has(Direction.Right)))
                return (hallways[random.Next(hallways.Length)],
                    exits.Has(Direction.Up) ? Direction.Up : Direction.Right);

            return (cShapes[random.Next(cShapes.Length)],
                BranchExtension.GetCShapeDirection(exits));
        }
    }
}
