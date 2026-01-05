using UnityEngine;

namespace FrxStudio.Generator
{
    public class CellOwner
    {
        public GameObject Instance { get; private set; }
        public Direction InstanceDirection { get; private set; }

        public void SetOwner(GameObject instance, Direction direction)
        {
            if (Instance != null)
                return;

            Instance = instance;
            InstanceDirection = direction;
        }
    }
}