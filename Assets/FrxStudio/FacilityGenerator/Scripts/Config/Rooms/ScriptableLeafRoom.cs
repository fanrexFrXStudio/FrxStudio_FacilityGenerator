using NaughtyAttributes;
using UnityEngine;

namespace FrxStudio.Generator
{
    /// <summary>
    /// Leaf room - тупиковая комната
    /// Всегда с шансом спавна 100, так как сам генератор подразумевает
    /// создание этих тупиковых комнат,
    /// и последующее соединение их коридорами
    /// </summary>
    [CreateAssetMenu(fileName = "NewLeaf",
        menuName = "FrxStudio/Generator/CreateNewLeafRoom")]
    public class ScriptableLeafRoom : ScriptableRoomBase
    {
        [Foldout("Spawn")]
        public bool OverrideDirection;

        [Foldout("Spawn"), ShowIf(nameof(OverrideDirection))]
        public Direction OverriddenDirection;

        [Foldout("Spawn"), Range(-1, 100)]
        public int MinCellsFromEdge, MaxCellsFromEdge;

        [Foldout("Spawn"), Range(0, 100)]
        public int MinCellsFromLeaf;

        // я учитываю что, комната может появиться в углу, и это приведет в багам
        // генерации, оссобенно, если направление будет переопределенно вбок ( право или елево )

        public override byte SpawnChance => 100;
    }
}
