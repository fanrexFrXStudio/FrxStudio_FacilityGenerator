using UnityEngine;

namespace FrxStudio.Generator
{
    public struct CellOwner
    {
        public readonly GameObject Instance;
        public readonly Direction InstanceDirection;

        public CellOwner(GameObject instance, Direction direction)
        {
            Instance = instance;
            InstanceDirection = direction;
        }
    }
}