using NaughtyAttributes;
using UnityEngine;

namespace FrxStudio.Generator
{
    [CreateAssetMenu(fileName = "NewInter",
        menuName = "FrxStudio/Generator/CreateNewInterRoom")]
    public class ScriptableInterRoom : ScriptableRoomBase
    {
        [Foldout("Base")]
        public BranchShape Shape;

        [Foldout("Spawn"), Range(1, 99), SerializeField]
        private byte spawnChance;

        public override byte SpawnChance => spawnChance;
    }

    public enum InterShape
    {
        CShape,
        Hallway
    }
}
