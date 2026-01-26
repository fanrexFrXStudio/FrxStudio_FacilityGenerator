namespace FrxStudio.Generator
{
    /// <summary>
    /// Branch node, exits and position
    /// </summary>
    public class BranchNode
    {
        public CellPosition Position;
        public ExitsMask Exits = ExitsMask.Empty;
    }
}