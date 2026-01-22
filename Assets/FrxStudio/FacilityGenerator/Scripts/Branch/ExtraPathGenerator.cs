using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FrxStudio.Generator
{
    public sealed class ExtraPathGenerator
    {
        private readonly LeafGenerator leafGenerator;
        private readonly BranchGenerator branchGenerator;

        private readonly Pathfinder pathfinder;
        private readonly Grid grid;

        private readonly ScriptableGeneratorPreset preset;
        private readonly System.Random random;

        private readonly HashSet<string> existingConnections = new();
        private readonly Dictionary<CellPosition, BranchNode> pathMarkers = new();

        public ExtraPathGenerator(
            LeafGenerator leafGenerator, BranchGenerator branchGenerator,
            Pathfinder pathfinder, Grid grid,
            ScriptableGeneratorPreset preset, System.Random random)
        {
            this.leafGenerator = leafGenerator;
            this.branchGenerator = branchGenerator;

            this.pathfinder = pathfinder;
            this.grid = grid;

            this.preset = preset;
            this.random = random;
        }

        #region Extra paths

        private string GetPairKey(CellPosition from, CellPosition to)
        {
            var hash1 = from.GetHashCode();
            var hash2 = to.GetHashCode();

            if (hash1 > hash2)
                return $"{hash1}-{hash2}";

            return $"{hash2}-{hash1}";
        }

        public void MarkExtraPaths()
        {
            const byte minLeafsCount = 2;
            var leafs = leafGenerator.PlacedLeafs?.ToArray();

            if (leafs == null || leafs.Length < minLeafsCount)
                return;

            var type = preset.ExtraPathType;

            switch (type)
            {
                case ExtraPathStrategy.Fixed:
                    CreateFixedCountPaths(leafs, preset.ExtraPathCount);
                    break;

                case ExtraPathStrategy.ByDistance:
                    CreatePathsByDistance(leafs, preset.ExtraPathMinDistance, preset.ExtraPathMaxCount);
                    break;

                case ExtraPathStrategy.ByLeafCount:
                    AddPathsByLeafCount(leafs);
                    break;
            }
        }

        // type 1: fixed paths count
        private void CreateFixedCountPaths(CellPosition[] leaves, int targetCount)
        {
            var maxAttempts = 10;
            var added = 0;

            while (added < targetCount && maxAttempts-- > 0)
            {
                var from = leaves[random.Next(leaves.Length)];
                var to = leaves[random.Next(leaves.Length)];

                if (from == to)
                    continue;

                // check if such a connection already exists
                var connectionKey = GetPairKey(from, to);
                if (existingConnections.Contains(connectionKey))
                    continue;

                // check the minimum distance (do not connect too close rooms)
                if (grid.GetManhattan(from, to) < preset.ExtraPathMinDistance)
                    continue;

                if (TryCreatePath(from, to))
                {
                    existingConnections.Add(connectionKey);
                    added++;
                }
            }

            if (added < targetCount)
                Debug.LogWarning($"[Generator]: Could only create {added}/{targetCount} extra paths");
        }

        // type 2: by distance 
        private void CreatePathsByDistance(CellPosition[] leaves, int minDistance, int maxCount)
        {
            var pairs = new List<(CellPosition from, CellPosition to, int distance)>();

            for (var i = 0; i < leaves.Length; i++)
            {
                for (var j = i + 1; j < leaves.Length; j++)
                {
                    var distance = grid.GetManhattan(leaves[i], leaves[j]);

                    if (distance >= minDistance)
                        pairs.Add((leaves[i], leaves[j], distance));
                }
            }

            // Sort by descending distance (farthest first)
            pairs = pairs.OrderByDescending(p => p.distance).ToList();

            // create paths

            var added = 0;

            foreach (var (from, to, distance) in pairs)
            {
                if (added >= maxCount)
                    break;

                var connectionKey = GetPairKey(from, to);

                if (existingConnections.Contains(connectionKey))
                    continue;

                if (TryCreatePath(from, to))
                {
                    existingConnections.Add(connectionKey);
                    added++;
                }
            }
        }

        // type 3: by leafs count 
        private void AddPathsByLeafCount(CellPosition[] leaves)
        {
            // number of additional paths = (number of sheets - 1) * coefficient
            // example, with 5 sheets and a coefficient of 0.5, there will be 2 additional paths
            var targetCount = Mathf.Max(1, Mathf.RoundToInt((leaves.Length - 1) * preset.ExtraPathPercentage));
            CreateFixedCountPaths(leaves, targetCount);
        }

        private bool TryCreatePath(CellPosition from, CellPosition to)
        {
            var (fromStart, toStart) = BranchExtension.GetPathStartPositions(from, to, grid);

            if (fromStart == CellPosition.Invalid || toStart == CellPosition.Invalid)
                return false;

            var pathEnd = pathfinder.GetPath(fromStart, toStart);

            if (pathEnd == null)
                return false;

            branchGenerator.MarkPathExits(pathEnd, from, to);
            return true;
        }

        #endregion
    }
}