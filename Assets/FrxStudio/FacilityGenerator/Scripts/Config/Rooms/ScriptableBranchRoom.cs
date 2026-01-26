using NaughtyAttributes;
using UnityEngine;

namespace FrxStudio.Generator
{
    [CreateAssetMenu(fileName = "NewBranch",
        menuName = "FrxStudio/Generator/CreateNewBranchRoom")]
    public class ScriptableBranchRoom : ScriptableRoomBase
    {
        [Foldout("Base")]
        public BranchShape Shape;

        [Foldout("Spawn"), Range(1, 100), SerializeField]
        private byte spawnChance;

        [Foldout("Size"), ShowIf(nameof(Large))]
        public Vector2Int ExitOffset;

        public override byte SpawnChance => spawnChance;

        public override Vector2Int[] ExitsOffset => new Vector2Int[] { ExitOffset };
    }

    public enum BranchShape
    {
        CShape,
        Hallway
    }
}
