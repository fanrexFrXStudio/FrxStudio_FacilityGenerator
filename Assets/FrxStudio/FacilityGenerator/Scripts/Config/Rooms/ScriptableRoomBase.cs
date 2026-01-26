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

        [Foldout("Size")]
        public bool Large;

        [Foldout("Size"), ShowIf(nameof(Large))]
        public Vector2Int[] AdditionalCells;

        public abstract byte SpawnChance { get; }
        public virtual Vector2Int[] ExitsOffset => null;

        protected bool MustSpawn => SpawnChance == 100;
    }
}
