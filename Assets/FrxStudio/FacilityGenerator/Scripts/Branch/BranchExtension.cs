using System.Linq;

namespace FrxStudio.Generator
{
    public static class BranchExtension
    {
        public static PathfindNode ConnectFromTo(
            this Pathinder pathfinder,
            CellPosition from, CellPosition to,
            FacilityGenerator generator,
            ScriptableBranchRoom[] rooms)
        {
            var path = pathfinder.GetPath(from, to);

            var current = path.Parent;
            var child = path;

            while (current != null && current.Parent != null && child != null)
            {
                var parent = current.Parent;

                var nextPos = parent != null ? parent.Cell.Position : child.Cell.Position;

                var (config, direction) = GetRoomInstanceData(
                    current.Cell.Position,
                    child.Cell.Position,
                    nextPos,
                    rooms);

                if (!generator.Spawn(config, current.Cell.Position, direction))
                {
                    UnityEngine.Debug.LogError("[Generator]: Failed to spawn brance");
                    return null;
                }

                child = current;
                current = parent;
            }

            return path;
        }

        private static (ScriptableBranchRoom config, Direction direction) GetRoomInstanceData(
            CellPosition from, CellPosition child, CellPosition next,
            ScriptableBranchRoom[] rooms)
        {
            var cShapeRoom = rooms.Where(room => room.Shape == BranchShape.CShape).ToArray()[0];
            var hallwayShapeRoom = rooms.Where(room => room.Shape == BranchShape.Hallway).ToArray()[0];

            var outDirection = GridExternal.GetDirectionBetween(null, from, next);
            var inDirection = GridExternal.GetDirectionBetween(null, child, from);

            if (outDirection == inDirection)
                return (hallwayShapeRoom, outDirection);

            UnityEngine.Debug.Log("CShape!");
            var rotation = GetCShapeDirection(outDirection, inDirection);
            return (cShapeRoom, rotation);
        }      

        private static Direction GetCShapeDirection(Direction outDirection, Direction inDirection)
        {
            // THIS METHOD WAS MADE A SELECTION METHOD
            // TOUCHING SOMETHING WOULD POSSIBLY BREAK EVERYTHING
            //Debug.Log("Out dir: " + outDirection + ", in dir: " + inDirection);

            if (outDirection == Direction.Down && inDirection == Direction.Right)
                return Direction.Up;

            if (outDirection == Direction.Right && inDirection == Direction.Up)
                return Direction.Left;

            if (outDirection == Direction.Left && inDirection == Direction.Down)
                return Direction.Right;

            if (outDirection == Direction.Up && inDirection == Direction.Left)
                return Direction.Down;

            // Forward <-> Right
            if (outDirection == Direction.Up && inDirection == Direction.Right)
                return Direction.Right;

            // Right <-> Backward
            if (outDirection == Direction.Right && inDirection == Direction.Down ||
                outDirection == Direction.Down && inDirection == Direction.Right)
                return Direction.Down;

            // Backward <-> Left
            if (outDirection == Direction.Down && inDirection == Direction.Left ||
                outDirection == Direction.Left && inDirection == Direction.Down)
                return Direction.Left;

            // Left <-> Forward
            if (outDirection == Direction.Left && inDirection == Direction.Up ||
                outDirection == Direction.Up && inDirection == Direction.Left)
                return Direction.Up;

            return Direction.Up;
        }
    }
}
