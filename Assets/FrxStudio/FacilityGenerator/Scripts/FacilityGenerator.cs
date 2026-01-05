using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

namespace FrxStudio.Generator
{
    public class FacilityGenerator : MonoBehaviour
    {
        #region Var

        [SerializeField]
        private GeneratorPreset preset;
        
        [SerializeField, ReadOnly]
        private FacilityGeneratorGizmo gizmo;

        [SerializeField]
        private bool generateInAwake;

        [Range(1, 16), SerializeField]
        private byte attemps = 8;

        [SerializeField]
        private int seed;

        private System.Random random;
        private readonly List<GameObject> spawned = new();

        private Grid grid;
        private LeafGenerator leafGenerator;

        private int initialSeed;

        private int RandomSeed => UnityEngine.Random.Range(1, int.MaxValue);

        #endregion

        #region Mono

        private void OnValidate() =>
            gizmo = GetComponent<FacilityGeneratorGizmo>();

        private void Awake()
        {
            if (generateInAwake)
                Generate();

            initialSeed = seed;
        }

        #endregion

        #region Logic

        public bool Spawn(ScriptableRoomBase room, CellPosition position, Direction direction)
        {
            var cell = grid.GetCell(position);

            if (cell == Cell.Invalid)
            {
                Debug.LogWarning("[Generator]: Attemp to spawn room in invalid cell");
                return false;
            }

            if (cell.IsBusy)
            {
                Debug.LogWarning("[Generator]: Attemp to spawn room in busy cell");
                return false;
            }

            var instance = Instantiate(room.Prefab, position.WorldPosition, grid.DirectionToEuler(direction));
            
            cell.SetOwner(instance, direction);
            grid.SetCell(position, cell);

            spawned.Add(instance);

            return true;
        }

        [Button("ClearSpawned")]
        public void ClearSpawned()
        {
            foreach (var obj in spawned)
                Destroy(obj);

            spawned.Clear();
            gizmo.ClearAllDrawables();
        }

        [Button("Generate")]
        public void Generate()
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            InitializeRandom();
            ClearSpawned();

            InitializeGrid();

            for (var attempt = 0; attempt < attemps; attempt++)
            {
                if (InitializeLeafs())
                    break;

                Debug.Log("[Generator]: Failed to initialize leafs. Attempt " + (attempt + 1));
                random = new(RandomSeed);
                ClearSpawned();
            }

            stopWatch.Stop();

            Debug.Log("[Generator]: <color=green>" +
                $"Successfully generated in {stopWatch.ElapsedMilliseconds} milliseconds</color>");
        }

        private void InitializeRandom()
        {
            if (seed <= 0 || initialSeed <= 0)
                seed = RandomSeed;

            random = new(seed);
        }

        private bool InitializeLeafs()
        {
            leafGenerator = new(preset, this, grid, random);
            return leafGenerator.SpawnLeafs();
        }

        private void InitializeGrid()
        {
            grid = new(preset, transform.position.y);
            gizmo.AddDrawable(grid);
        }

        #endregion
    }
}