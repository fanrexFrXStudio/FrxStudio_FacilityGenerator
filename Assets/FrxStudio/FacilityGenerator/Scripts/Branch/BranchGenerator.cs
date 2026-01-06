using System.Linq;
using UnityEngine;

namespace FrxStudio.Generator
{
    public class BranchGenerator : IGizmoDrawable
    {
        private readonly Pathinder pathfinder;
        private readonly Grid grid;
        private readonly LeafGenerator leafGenerator;
        private readonly ScriptableGeneratorPreset preset;
        private readonly FacilityGenerator generator;

        // test a to b
        private PathfindNode path;

        public BranchGenerator(
            Grid grid, LeafGenerator leafGenerator,
            ScriptableGeneratorPreset preset,
            FacilityGenerator generator)
        {
            this.grid = grid;
            this.leafGenerator = leafGenerator;
            this.preset = preset;
            this.generator = generator;
            
            pathfinder = new(grid);
        }

        public void ConnectBranch()
        {
            var rooms = preset.Rooms.OfType<ScriptableBranchRoom>().ToArray();

            path = pathfinder.ConnectFromTo(
                leafGenerator.PlacedLeafs[0],
                leafGenerator.PlacedLeafs[1],
                generator,
                rooms);
        }

        public void DrawGizmo()
        {
            if (path == null)
                return;

            var current = path;

            while (current != null)
            {
                // green, 35% transparent
                Gizmos.color = new(0, 1, 0, 0.35f);
                Gizmos.DrawCube(current.Cell.Position.WorldPosition, preset.CellSize * Vector3.one);

                current = current.Parent;
            }
        }
    }
}
