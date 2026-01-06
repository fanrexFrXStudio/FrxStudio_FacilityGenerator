namespace FrxStudio.Generator
{
    public class InterNode
    {
        public CellPosition Position;

        public Direction[] Exits;

        // этот перекресток уже соединен с другим перекрестком?
        public bool AlreadyConnectedWithInterRoom;
    }
}
