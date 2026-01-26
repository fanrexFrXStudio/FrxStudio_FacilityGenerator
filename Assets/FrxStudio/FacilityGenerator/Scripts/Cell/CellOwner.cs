using UnityEngine;

namespace FrxStudio.Generator
{
    /// <summary>
    /// The container of instance, instance scriptable and instance direction
    /// </summary>
    public class CellOwner
    {
        public ScriptableRoomBase ScriptableReference { get; private set; }

        public GameObject Instance { get; private set; }
        public Direction InstanceDirection { get; private set; }

        /// <summary>
        /// True if this part of the room
        /// </summary>
        public bool IsRoomReserved { get; private set; }

        public void SetOwner(GameObject instance, Direction direction, ScriptableRoomBase reference, bool isRoomReserved)
        {
            if (Instance != null)
                return;

            Instance = instance;
            InstanceDirection = direction;

            ScriptableReference = reference;

            IsRoomReserved = isRoomReserved;
        }
    }
}