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
    }
}