using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FrxStudio.Generator
{
    /// <summary>
    /// The spawner of branch rooms
    /// </summary>
    public class BranchSpawner
    {
        private readonly Grid grid;
        private readonly FacilityGenerator generator;
        private readonly ScriptableGeneratorPreset preset;
        private readonly System.Random random;

        private readonly Dictionary<ScriptableRoomBase, byte> remainingRequiredRooms = new();

        public BranchSpawner(Grid grid, FacilityGenerator generator, ScriptableGeneratorPreset preset, System.Random random)
        {
            this.grid = grid;
            this.generator = generator;
            this.preset = preset;
            this.random = random;

            InitializeRequiredRooms();
        }

        /// <summary>
        /// Initialize must spawn branches and intersections
        /// </summary>
        private void InitializeRequiredRooms()
        {
            var branchRooms = preset.Rooms.OfType<ScriptableBranchRoom>();
            var interRooms = preset.Rooms.OfType<ScriptableInterRoom>();

            var relevantRooms = branchRooms.Cast<ScriptableRoomBase>()
                .Concat(interRooms.Cast<ScriptableRoomBase>());

            foreach (var room in relevantRooms)
            {
                if (room.SpawnChance == 100 && room.Count > 0)
                    remainingRequiredRooms[room] = room.Count;
            }
        }

        /// <summary>
        /// Create rooms in paths
        /// </summary>
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

                var (roomToSpawn, spawnDirection) = DetermineRoomType(marker, hallways, cShapes, tShapes, crosses);

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

                if (remainingRequiredRooms.ContainsKey(roomToSpawn))
                {
                    remainingRequiredRooms[roomToSpawn]--;

                    if (remainingRequiredRooms[roomToSpawn] == 0)
                        remainingRequiredRooms.Remove(roomToSpawn);
                }
            }

            if (remainingRequiredRooms.Count > 0)
                return false;

            return true;
        }

        /// <summary>
        /// Get scriptable room and direction, by exitsCount
        /// </summary>
        private (ScriptableRoomBase room, Direction direction) DetermineRoomType(
            BranchNode marker,
            ScriptableBranchRoom[] hallways, ScriptableBranchRoom[] cShapes,
            ScriptableInterRoom[] tShapes, ScriptableInterRoom[] crosses)
        {
            return marker.Exits.Count switch
            {
                2 => GetCorridorRoom(marker, hallways, cShapes),
                3 => GetTShapeRoom(marker, tShapes),
                4 => GetCrossRoom(crosses),
                _ => (null, Direction.Up)
            };
        }

        #region Shapes room data

        private (ScriptableRoomBase room, Direction direction) GetTShapeRoom(
            BranchNode marker,
            ScriptableInterRoom[] tShapes)
        {
            return (SelectRoom(tShapes), BranchExtension.GetTShapeDirection(marker.Exits));
        }

        private (ScriptableRoomBase room, Direction direction) GetCrossRoom(
            ScriptableInterRoom[] crosses)
        {
            return (SelectRoom(crosses), Direction.Up);
        }

        private (ScriptableRoomBase room, Direction direction) GetCorridorRoom(
            BranchNode marker,
            ScriptableBranchRoom[] hallways,
            ScriptableBranchRoom[] cShapes)
        {
            var exits = marker.Exits;

            if ((exits.Has(Direction.Up) && exits.Has(Direction.Down)) ||
                (exits.Has(Direction.Left) && exits.Has(Direction.Right)))
            {
                return (SelectRoom(hallways), exits.Has(Direction.Up) ? Direction.Up : Direction.Right);
            }

            return (SelectRoom(cShapes), BranchExtension.GetCShapeDirection(exits));
        }

        #endregion

        private ScriptableRoomBase SelectRoom(ScriptableRoomBase[] rooms)
        {
            var availableRequired = rooms.Where(r => remainingRequiredRooms.ContainsKey(r)).ToArray();

            if (availableRequired.Length > 0)
                return availableRequired[random.Next(availableRequired.Length)];

            var regularRooms = rooms.Where(r => r.SpawnChance < 100).ToArray();

            if (regularRooms.Length > 0)
                return regularRooms[random.Next(regularRooms.Length)];

            return rooms[random.Next(rooms.Length)];
        }
    }
}