using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FrxStudio.Generator
{
    public static class BranchExtension
    {
        public static PathfindNode ConnectFromTo(
            this Pathinder pathfinder,
            CellPosition from, CellPosition to,
            FacilityGenerator generator,
            ScriptableBranchRoom[] rooms,
            Grid grid)
        {
            var fromCell = grid.GetCell(from);
            var toCell = grid.GetCell(to);

            if (fromCell == null || toCell == null)
            {
                Debug.Log("[Generator]: ConnectFromTo: Failed to connect, from or to cell is invalid");
                return null;
            }

            var fromNext = GridExternal.GetNext(grid, from, fromCell.Owner.InstanceDirection);
            var toNext = GridExternal.GetNext(grid, to, toCell.Owner.InstanceDirection);

            if (fromNext == null || toNext == null)
            {
                Debug.Log("[Generator]: ConnectFromTo: Failed to connect, from next or to next is invalid. Check the configuration");
                return null;
            }

            var path = pathfinder.GetPath(fromNext, toNext);
            if (path == null)
            {
                Debug.Log("[Generator]: ConnectFromTo: Path not found");
                return null;
            }

            Debug.Log("From next is " + fromNext + ", to next is " + toNext);

            var nodes = new List<PathfindNode>();
            var iter = path;

            while (iter != null)
            {
                nodes.Add(iter);
                iter = iter.Parent;
            }

            for (var i = 0; i < nodes.Count; i++)
            {
                var current = nodes[i];
                var child = (i > 0) ? nodes[i - 1] : null;
                var parent = (i < nodes.Count - 1) ? nodes[i + 1] : null;

                // Если child == null => это конечный узел пути (с приставкой к комнате to)
                // Если parent == null => это начальный узел пути (с приставкой к комнате from)
                var inPos = child != null ? child.Cell.Position : to;
                var outPos = parent != null ? parent.Cell.Position : from;

                var (config, direction) = GetRoomInstanceData(
                    current.Cell.Position,
                    inPos,
                    outPos,
                    rooms);

                if (!generator.Spawn(config, current.Cell.Position, direction))
                {
                    Debug.LogError("[Generator]: Failed to spawn branch");
                    return null;
                }
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

            var rotation = GetCShapeDirection(outDirection, inDirection);
            return (cShapeRoom, rotation);
        }

        private static Direction GetCShapeDirection(Direction outDirection, Direction inDirection)
        {
            // THIS METHOD WAS MADE A SELECTION METHOD
            // TOUCHING SOMETHING WOULD POSSIBLY BREAK EVERYTHING

            // этот метод ПОЗЖЕ будет приведен в нормальный вид
            // пока что он работает стабильно, и повороты всегда верные
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