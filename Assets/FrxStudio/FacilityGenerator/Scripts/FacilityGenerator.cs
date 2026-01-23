using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FrxStudio.Generator
{
    public class FacilityGenerator : MonoBehaviour
    {
        #region Var

        [SerializeField]
        private ScriptableGeneratorPreset preset;

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
        private BranchGenerator branchGenerator;

        private int initialSeed;

        private int RandomSeed => UnityEngine.Random.Range(1, int.MaxValue);

        #endregion

        #region Mono

        private void OnValidate() =>
            gizmo = GetComponent<FacilityGeneratorGizmo>();

        private void Awake()
        {
            initialSeed = seed;

            if (generateInAwake)
                Generate();
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

            cell.SetOwner(instance, direction, room);
            grid.SetCell(position, cell);

            spawned.Add(instance);

            return true;
        }

        [Button("ClearSpawned", EButtonEnableMode.Playmode)]
        public void ClearSpawned()
        {
            foreach (var obj in spawned)
                Destroy(obj);

            spawned.Clear();
            gizmo.ClearAllDrawables();
        }

        [Button("Start stress test (2500 iterations, and result)", EButtonEnableMode.Playmode)]
        private void GeneratorStressTest()
        {
            const int iterations = 2500;
            const int maxFailedIterationsAllowed = 15;

            StartCoroutine(ForEachGenerate(iterations));

            IEnumerator ForEachGenerate(int iterations)
            {
                var failedCount = 0;

                for (var i = 0; i < iterations; i++)
                {
                    seed = 0;

                    if (!Generate(false))
                    {
                        failedCount++;
                        Debug.LogWarning("[Generator]: StressTest: In iteration " + i + ", not generated");
                    }                   
                    
                    // skip 1 frame
                    yield return null;
                }

                Debug.Log("[Generator]: StressTest: Ended, failed generations: " + failedCount);

                if (failedCount > maxFailedIterationsAllowed)
                {
                    Debug.LogWarning("[Generator]: StressTest: Try to change configuration, current is not stable");
                }
                else
                {
                    Debug.Log("[Generator]: StressTest: Current configuration is stable!");
                }
            }
        }

        [Button("Generate", EButtonEnableMode.Playmode)]
        public bool Generate(bool logging = true)
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            InitializeRandom();
            ClearSpawned();

            var isSpawned = false;

            for (var attempt = 0; attempt < attemps; attempt++)
            {
                InitializeGrid();

                if (InitializeLeafs() &&
                    InitializeBranch())
                {
                    isSpawned = true;
                    break;
                }

                if (logging)
                    Debug.LogWarning("[Generator]: Failed to spawn. Attempt " + (attempt + 1));

                seed = RandomSeed;
                random = new(seed);

                ClearSpawned();
            }

            if (!isSpawned)
            {
                Debug.LogError("[Generator]: Generation not spawned :( Check the configuration");
                return false;
            }

            stopWatch.Stop();

            if (logging)
                Debug.Log("[Generator]: <color=green>" +
                    $"Successfully generated in {stopWatch.Elapsed.TotalMilliseconds:F3} milliseconds</color>");

            return true;
        }

        private void InitializeRandom()
        {
            if (seed <= 0 || initialSeed <= 0)
                seed = RandomSeed;

            random = new(seed);
        }

        private void InitializeGrid()
        {
            grid = new(preset, transform.position.y);
            gizmo.AddDrawable(grid);
        }

        private bool InitializeLeafs()
        {
            leafGenerator = new(preset, this, grid, random);
            return leafGenerator.SpawnLeafs();
        }

        private bool InitializeBranch()
        {
            branchGenerator = new(grid, leafGenerator, preset, this, random);
            return branchGenerator.ConnectBranch();
        }

        #endregion
    }
}