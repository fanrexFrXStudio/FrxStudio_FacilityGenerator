using UnityEngine;
using NaughtyAttributes;

namespace FrxStudio.Generator
{
    [CreateAssetMenu(fileName = "NewGeneratorPreset",
        menuName = "FrxStudio/Generator/CreateNewGeneratorPreset")]
    public class ScriptableGeneratorPreset : ScriptableObject
    {
        [Foldout("Cell"), Range(1, 50)]
        public float CellSize;

        [Foldout("Cell"), Range(.01f, 5)]
        public float CellSpacing;

        [Foldout("Cell"), Range(1, 50)]
        public int CellsCountX, CellsCountY;

        public ScriptableRoomBase[] Rooms;

        [Foldout("Extra Paths")]
        [Tooltip("Стратегия создания дополнительных путей")]
        public ExtraPathStrategy ExtraPathStrategy = ExtraPathStrategy.ByDistance;

        [Foldout("Extra Paths")]
        [ShowIf(nameof(ShowFixedCount))]
        [Tooltip("Точное количество дополнительных путей (для стратегии Fixed)")]
        [Range(1, 10)]
        public int ExtraPathCount = 2;

        [Foldout("Extra Paths")]
        [ShowIf(nameof(ShowByDistance))]
        [Tooltip("Минимальная дистанция между комнатами для создания доп. пути")]
        [Range(3, 20)]
        public int ExtraPathMinDistance = 5;

        [Foldout("Extra Paths")]
        [ShowIf(nameof(ShowByDistance))]
        [Tooltip("Максимальное количество доп. путей (для стратегии ByDistance)")]
        [Range(1, 10)]
        public int ExtraPathMaxCount = 3;

        [Foldout("Extra Paths")]
        [ShowIf(nameof(ShowByLeafCount))]
        [Tooltip("Процент от количества комнат (0.5 = 50%)")]
        [Range(0f, 1f)]
        public float ExtraPathPercentage = 0.3f;

        private bool ShowFixedCount => ExtraPathStrategy == ExtraPathStrategy.Fixed;
        private bool ShowByDistance => ExtraPathStrategy == ExtraPathStrategy.ByDistance;
        private bool ShowByLeafCount => ExtraPathStrategy == ExtraPathStrategy.ByLeafCount;
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