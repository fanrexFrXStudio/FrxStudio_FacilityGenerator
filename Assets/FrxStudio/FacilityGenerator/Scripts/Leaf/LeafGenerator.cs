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
                return grid.GetRandomDirection(random);
            
            return grid.Rotate(
                grid.GetNearestEdgeInwardDirection(position),
                config.OverriddenDirection);
        }

        private CellPosition GetLeafPosition(ScriptableLeafRoom config)
        {
            var cells = grid.GetCells(cellPosition => !grid.GetCell(cellPosition).IsBusy).ToList();
            var candidatesCells = new List<CellPosition>();

            // можно на линк, но так читабельнее и чуть производительней (мне похуй)
            foreach (var cellPosition in cells)
            {
                var nearestEdgePosition = grid.GetNearestEdgePosition(cellPosition);
                var manhattan = grid.GetManhattan(cellPosition, nearestEdgePosition);

                if (config.MinCellsFromEdge != -1 && manhattan < config.MinCellsFromEdge)
                    continue;

                if (config.MaxCellsFromEdge != -1 && manhattan > config.MaxCellsFromEdge)
                    continue;

                var placedInRadius = false;

                foreach (var placedLeaf in placedLeafs)
                {
                    var placedLeafManhattan = grid.GetManhattan(cellPosition, placedLeaf);

                    if (placedLeafManhattan < config.MinCellsFromLeaf)
                    {
                        placedInRadius = true;
                        break;
                    }
                }

                if (!placedInRadius)
                    candidatesCells.Add(cellPosition);
            }

            if (candidatesCells.Count == 0)
                return CellPosition.Invalid;

            return candidatesCells[random.Next(candidatesCells.Count)];
        }
    }
}
