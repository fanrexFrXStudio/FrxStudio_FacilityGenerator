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

        [Foldout("Spawn"), ShowIf(nameof(MustSpawn))]
        public byte Count;

        public abstract byte SpawnChance { get; }
        protected bool MustSpawn => SpawnChance == 100;
    }
}
