using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine.UIElements;

namespace FrxStudio.Generator
{
    /// <summary>
    /// Dead End Generator ( all dead ends are ,must rooms )
    /// </summary>
    public class LeafGenerator
    {
        public IReadOnlyList<CellPosition> PlacedLeafs => placedLeafs;

        private readonly FacilityGenerator generator;
        private readonly Grid grid;

        private readonly ScriptableGeneratorPreset preset;
        private readonly Random random;

        private readonly List<CellPosition> placedLeafs = new();

        public LeafGenerator(ScriptableGeneratorPreset preset, FacilityGenerator generator, Grid grid, Random random)
        {
            this.generator = generator;
            this.grid = grid;

            this.preset = preset;
            this.random = random;
        }

        /// <summary>
        /// Create all leafs, in random position by config
        /// </summary>
        /// <returns></returns>
        public bool SpawnLeafs()
        {
            var leafs = preset.Rooms.OfType<ScriptableLeafRoom>().ToArray();

            if (leafs.Length == 0)
                return false;

            foreach (var leaf in leafs)
            {
                for (var count = 0; count < leaf.Count; count++)
                {
                    //var position = GetLeafPosition(leaf, reservedCells);

                   // if (position == CellPosition.Invalid)
                   //     return false;

                   // var direction = GetDirection(leaf, position);

                    var (position, direction, additionalCells) = GetLeafData(leaf);

                    if (!generator.Spawn(leaf, position, direction, additionalCells))
                        return false;

                    placedLeafs.Add(position);
                }
            }

            return true;
        }

        /*
        /// <summary>
        /// Return random valid direction, if override, then overridden direction
        /// </summary>
        private Direction GetDirection(ScriptableLeafRoom config, CellPosition position)
        {
            if (!config.OverrideDirection)
                return grid.GetRandomDirection(position, random);

            return grid.Rotate(
                grid.GetNearestEdgeInwardDirection(position),
                config.OverriddenDirection);
        }
        */

        private (CellPosition position, Direction direction, List<CellPosition> reservedPositions) GetLeafData(ScriptableLeafRoom room)
        {
            var cells = grid.GetCells(cellPosition => !grid.GetCell(cellPosition).IsBusy).ToList();
            var candidatesCells = new List<(CellPosition, Direction, List<CellPosition>)>();

            foreach (var cellPosition in cells)
            {
                var reservedPositions = new List<CellPosition>();

                var direction = !room.OverrideDirection ?
                    grid.GetRandomDirection(cellPosition, random) :
                    grid.Rotate(grid.GetNearestEdgeInwardDirection(cellPosition), room.OverriddenDirection);

                if (IsValidByEdgeDistance(room, cellPosition) &&
                    !HasConflictWithPlacedLeafs(room, cellPosition) &&
                    grid.IsValidLargeRoom(room, cellPosition, direction, reservedPositions))
                {
                    candidatesCells.Add((cellPosition, direction, new List<CellPosition>(reservedPositions)));
                }
            }

            if (candidatesCells.Count == 0)
                return (CellPosition.Invalid, Direction.Up, null);

            var selected = candidatesCells[random.Next(candidatesCells.Count)];
            return selected;
        }

        /*
        /// <returns>Random valid position</returns>
        private CellPosition GetLeafPosition(ScriptableLeafRoom config, List<CellPosition> reservedPositions)
        {
            var cells = grid.GetCells(cellPosition => !grid.GetCell(cellPosition).IsBusy).ToList();
            var candidatesCells = new List<CellPosition>();

            foreach (var cellPosition in cells)
            {
                if (IsValidByEdgeDistance(config, cellPosition) &&
                    !HasConflictWithPlacedLeafs(config, cellPosition) &&
                    !GridExtension.IsValidLargeRoom(grid, config, cellPosition, Direction.Up, reservedPositions)
                    candidatesCells.Add(cellPosition);
            }

            if (candidatesCells.Count == 0)
                return CellPosition.Invalid;

            return candidatesCells[random.Next(candidatesCells.Count)];
        }
        */
        /*
        /// <summary>
        /// If is large, checking size
        /// </summary>
        private bool IsValidLarge(ScriptableLeafRoom config, CellPosition position, )
        {
            if
        }
        */
        private bool IsValidByEdgeDistance(ScriptableLeafRoom config, CellPosition cellPosition)
        {
            var nearestEdgePosition = grid.GetNearestEdgePosition(cellPosition);
            var manhattan = grid.GetManhattan(cellPosition, nearestEdgePosition);

            return !(config.MinCellsFromEdge != -1 && manhattan < config.MinCellsFromEdge ||
                config.MaxCellsFromEdge != -1 && manhattan > config.MaxCellsFromEdge);
        }

        private bool HasConflictWithPlacedLeafs(ScriptableLeafRoom config, CellPosition cellPosition)
        {
            foreach (var placedLeaf in placedLeafs)
            {
                if (IsTooCloseToLeaf(config, cellPosition, placedLeaf) ||
                    IsLookedAtByExistingLeaf(cellPosition, placedLeaf) ||
                    IsLookAtExistingLeaf(config, cellPosition, placedLeaf))
                    return true;
            }

            return false;
        }

        private bool IsTooCloseToLeaf(
            ScriptableLeafRoom config, CellPosition cellPosition, CellPosition placedLeaf) =>
            grid.GetManhattan(cellPosition, placedLeaf) < config.MinCellsFromLeaf;

        private bool IsLookedAtByExistingLeaf(CellPosition cellPosition, CellPosition placedLeaf)
        {
            var placedLeafDirection = grid.GetCell(placedLeaf).Owner.InstanceDirection;
            return grid.GetNext(placedLeaf, placedLeafDirection) == cellPosition;
        }

        private bool IsLookAtExistingLeaf(ScriptableLeafRoom config, CellPosition cellPosition, CellPosition placedLeaf)
        {
            if (config.OverrideDirection)
            {
                var futureDirection = grid.Rotate(
                    grid.GetNearestEdgeInwardDirection(cellPosition),
                    config.OverriddenDirection);

                return grid.GetNext(cellPosition, futureDirection) == placedLeaf;
            }

            if (grid.GetManhattan(cellPosition, placedLeaf) != 1)
                return false;

            foreach (var dir in GridExtension.CachedDirections)
            {
                if (grid.GetNext(cellPosition, dir) == placedLeaf)
                    return true;
            }

            return false;
        }
    }
}