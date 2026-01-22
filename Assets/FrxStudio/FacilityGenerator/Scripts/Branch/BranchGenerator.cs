namespace FrxStudio.Generator
{
    public class BranchGenerator
    {
        private readonly PathPlanner pathPlanner;
        private readonly RoomSpawner roomSpawner;
        private readonly ExtraPathGenerator extraPathGenerator;

        private readonly Pathfinder pathfinder;

        public BranchGenerator(
            Grid grid, LeafGenerator leafGenerator,
            ScriptableGeneratorPreset preset,
            FacilityGenerator generator,
            System.Random random)
        {
            pathfinder = new(grid);

            pathPlanner = new PathPlanner(grid, leafGenerator, pathfinder);
            roomSpawner = new RoomSpawner(grid, generator, preset, random);
            extraPathGenerator = new ExtraPathGenerator(leafGenerator, this, pathfinder, grid, preset, random);
        }

        public bool ConnectBranch()
        {
            // step 1: find path and mark exits WITHOUT spawning room
            if (!pathPlanner.MarkAllPaths())
                return false;

            // step 1.5: add extra paths for network structure
            extraPathGenerator.MarkExtraPaths();

            // step 2: spawn rooms in paths
            if (!roomSpawner.SpawnRoomsOnPaths(pathPlanner.GetPathMarkers()))
                return false;

            return true;
        }

        public void MarkPathExits(PathfindNode pathEnd, CellPosition fromLeaf, CellPosition toLeaf) =>
            pathPlanner.MarkPathExits(pathEnd, fromLeaf, toLeaf);
    }
}
