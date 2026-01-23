using UnityEngine;
using NaughtyAttributes;

namespace FrxStudio.Generator
{
    [CreateAssetMenu(fileName = "NewGeneratorPreset",
        menuName = "FrxStudio/Generator/CreateNewGeneratorPreset")]
    public class ScriptableGeneratorPreset : ScriptableObject
    {
        [Foldout("Cell"), Range(1, 50)]
        public float CellSize = 10;

        [Foldout("Cell"), Range(.01f, 5)]
        public float CellSpacing = 0.1f;

        [Foldout("Cell"), Range(3, 50)]
        public int CellsCountX = 7, CellsCountY = 7;

        public ScriptableRoomBase[] Rooms;

        [Foldout("Required Room"), Range(.05f, 0.5f)]
        public float MustRoomBaseChance = 0.15f;

        [Foldout("Required Room"), Range(1.1f, 2.0f)]
        public float MustRoomChanceMultiplier = 1.3f;

        [Foldout("Required Room"), Range(0f, 2f)]
        public float MustRoomChanceResetValue = 2;

        [Foldout("Extra Paths")]
        public ExtraPathStrategy ExtraPathType = ExtraPathStrategy.ByDistance;

        [Foldout("Extra Paths"), ShowIf(nameof(ShowFixedCount)), 
            Tooltip("Fixed extra paths count"), Range(1, 10)]
        public int ExtraPathCount = 2;

        [Foldout("Extra Paths"), ShowIf(nameof(ShowByDistance)),
            Tooltip("Min distance between leafs, to create extra path"), Range(1, 20)]
        public int ExtraPathMinDistance = 5;

        [Foldout("Extra Paths"), ShowIf(nameof(ShowByDistance)),
            Tooltip("Max extra paths count"), Range(1, 10)]
        public int ExtraPathMaxCount = 3;

        [Foldout("Extra Paths"), ShowIf(nameof(ShowByLeafCount)),
            Tooltip("Percent of leafs count (0.5 = 50%) Процент от количества комнат (0.5 = 50%)"), Range(0f, 1f)]
        public float ExtraPathPercentage = 0.3f;

        private bool ShowFixedCount => ExtraPathType == ExtraPathStrategy.Fixed;
        private bool ShowByDistance => ExtraPathType == ExtraPathStrategy.ByDistance;
        private bool ShowByLeafCount => ExtraPathType == ExtraPathStrategy.ByLeafCount;
    }

    public enum ExtraPathStrategy
    {
        [Tooltip("Не создавать дополнительные пути")]
        None,

        [Tooltip("Фиксированное количество путей")]
        Fixed,

        [Tooltip("По дистанции (соединяет далёкие комнаты)")]
        ByDistance,

        [Tooltip("По количеству комнат (процент от общего числа)")]
        ByLeafCount
    }
}