using UnityEngine;

namespace FrxStudio.Generator
{
    public class BranchGenerator : IGizmoDrawable
    {
        private readonly Pathinder pathfinder;
        private readonly Grid grid;
        private readonly LeafGenerator leafGenerator;
        private readonly ScriptableGeneratorPreset preset;

        // test a to b
        private PathfindNode instantiated;

        public BranchGenerator(
            Grid grid, LeafGenerator leafGenerator,
            ScriptableGeneratorPreset preset)
        {
            this.grid = grid;
            this.leafGenerator = leafGenerator;
            this.preset = preset;
            
            pathfinder = new(grid);

            instantiated = pathfinder.GetPath(leafGenerator.PlacedLeafs[0], leafGenerator.PlacedLeafs[1]);
            Debug.Log("Instantiated set " + instantiated.Cell.Position);
        }

        public void DrawGizmo()
        {
            if (instantiated == null)
                return;

            var current = instantiated;

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
