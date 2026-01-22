using UnityEngine;

namespace FrxStudio.Generator
{
    public class CellOwner
    {
        public ScriptableRoomBase ScriptableReference { get; private set; }
        public GameObject Instance { get; private set; }
        public Direction InstanceDirection { get; private set; }

        public void SetOwner(GameObject instance, Direction direction, ScriptableRoomBase reference)
        {
            if (Instance != null)
                return;

            Instance = instance;
            InstanceDirection = direction;
            ScriptableReference = reference;
        }
    }
}