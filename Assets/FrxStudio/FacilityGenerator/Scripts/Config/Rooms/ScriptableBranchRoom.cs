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

        public override byte SpawnChance => spawnChance;
    }

    public enum BranchShape
    {
        CShape,
        Hallway
    }
}
