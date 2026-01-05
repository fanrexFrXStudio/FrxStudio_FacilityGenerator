using NaughtyAttributes;
using UnityEngine;

namespace FrxStudio.Generator
{
    public abstract class ScriptableRoomBase : ScriptableObject
    {
        [Foldout("Debug")]
        public string Name;

        [Foldout("Base")]
        public GameObject Prefab;

        public abstract byte SpawnChance { get; }
    }
}
