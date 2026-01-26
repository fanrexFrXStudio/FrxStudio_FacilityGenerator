using UnityEngine;
using NaughtyAttributes;

namespace FrxStudio.Generator
{
    [CreateAssetMenu(fileName = "NewGeneratorPreset",
        menuName = "FrxStudio/Generator/CreateNewGeneratorPreset")]
    public class ScriptableGeneratorPreset : ScriptableObject
    {
        /// <summary>
        /// Size in world
        /// </summary>
        [Foldout("Cell"), Range(1, 50)]
        public float CellSize = 10;

        /// <summary>
        /// World spacing between 1 cell
        /// </summary>
        [Foldout("Cell"), Range(.01f, 5)]
        public float CellSpacing = 0.1f;

        [Foldout("Cell"), Range(3, 50)]
        public int CellsCountX = 7, CellsCountY = 7;

        /// <summary>
        /// Rooms to spawn
        /// </summary>
        public ScriptableRoomBase[] Rooms;

        [Foldout("Must branches"), Range(.05f, 0.5f)]
        public float MustBranchBaseChance = 0.15f;

        [Foldout("Must branches"), Range(1.1f, 2.0f)]
        public float MustBranchChanceMultiplier = 1.3f;

        [Foldout("Must branches"), Range(0f, 2f)]
        public float MustBranchChanceResetValue = 2;

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

    /// <summary>
    /// Variant of create additional paths
    /// </summary>
    public enum ExtraPathStrategy
    {
        [Tooltip("Dont create extra path")]
        None,

        [Tooltip("Fixed path count")]
        Fixed,

        [Tooltip("By distance ( connect distant room )")]
        ByDistance,

        [Tooltip("By rooms count (percentage of total)")]
        ByLeafCount
    }
}