using System;
using System.Collections.Generic;
using System.Linq;

namespace FrxStudio.Generator
{
    public class LeafGenerator
    {
        public IReadOnlyList<CellPosition> PlacedLeafs => placedLeafs;

        private readonly ScriptableGeneratorPreset preset;
        private readonly FacilityGenerator generator;
        private readonly Grid grid;
        private readonly Random random;

        private readonly List<CellPosition> placedLeafs = new();

        public LeafGenerator(ScriptableGeneratorPreset preset, FacilityGenerator generator, Grid grid, Random random)
        {
            this.preset = preset;
            this.generator = generator;
            this.grid = grid;
            this.random = random;
        }

        public bool SpawnLeafs()
        {
            var leafs = preset.Rooms.OfType<ScriptableLeafRoom>().ToArray();

            if (leafs.Length == 0)
            {
                UnityEngine.Debug.LogWarning("[Generator]: Leafs count is 0");
                return false;
            }

            foreach (var leaf in leafs)
            {
                for (var count = 0; count < leaf.Count; count++)
                {
                    var position = GetLeafPosition(leaf);

                    if (position == CellPosition.Invalid)
                    {
                        UnityEngine.Debug.Log("[Generator]: No valid position found for leaf");
                        return false;
                    }

                    var direction = GetDirection(leaf, position);

                    if (!generator.Spawn(leaf, position, direction))
                    {
                        UnityEngine.Debug.Log("[Generator]: Leaf failed to spawn");
                        return false;
                    }

                    placedLeafs.Add(position);
                }
            }

            return true;
        }

        private Direction GetDirection(ScriptableLeafRoom config, CellPosition position)
        {
            if (!config.OverrideDirection)
                return grid.GetRandomDirection(position, random);

            return grid.Rotate(
                grid.GetNearestEdgeInwardDirection(position),
                config.OverriddenDirection);
        }

        private CellPosition GetLeafPosition(ScriptableLeafRoom config)
        {
            var cells = grid.GetCells(cellPosition => !grid.GetCell(cellPosition).IsBusy).ToList();
            var candidatesCells = new List<CellPosition>();

            foreach (var cellPosition in cells)
            {
                if (IsValidByEdgeDistance(config, cellPosition) &&
                    !HasConflictWithPlacedLeafs(config, cellPosition))
                    candidatesCells.Add(cellPosition);
            }

            if (candidatesCells.Count == 0)
                return CellPosition.Invalid;

            return candidatesCells[random.Next(candidatesCells.Count)];
        }

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
                    WouldLookAtExistingLeaf(config, cellPosition, placedLeaf))
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

        private bool WouldLookAtExistingLeaf(ScriptableLeafRoom config, CellPosition cellPosition, CellPosition placedLeaf)
        {
            // Если направление переопределено - проверяем конкретное
            if (config.OverrideDirection)
            {
                var futureDirection = grid.Rotate(
                    grid.GetNearestEdgeInwardDirection(cellPosition),
                    config.OverriddenDirection);

                return grid.GetNext(cellPosition, futureDirection) == placedLeaf;
            }

            // Если случайное - проверяем не является ли существующая комната прямым соседом
            // Если да - есть риск что случайное направление будет смотреть на неё
            var manhattan = grid.GetManhattan(cellPosition, placedLeaf);
            if (manhattan != 1)
                return false;

            // Проверяем: находится ли существующая комната в одном из 4 направлений
            foreach (var dir in (Direction[])Enum.GetValues(typeof(Direction)))
            {
                if (grid.GetNext(cellPosition, dir) == placedLeaf)
                    return true;
            }

            return false;
        }
    }
}