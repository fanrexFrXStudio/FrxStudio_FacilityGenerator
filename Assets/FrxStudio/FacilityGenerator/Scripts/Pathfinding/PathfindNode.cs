using System;

namespace FrxStudio.Generator
{
    /// <summary>
    /// Data of path
    /// </summary>
    public class PathfindNode : IComparable<PathfindNode>
    {
        public Cell Cell;
        public PathfindNode Parent;

        /// <summary>
        /// On A star - G
        /// </summary>
        public int CostFromStart;

        /// <summary>
        /// On A star - H
        /// </summary>
        public int HeuristicCost;

        /// <summary>
        /// On A star - F
        /// </summary>
        public int Total => CostFromStart + HeuristicCost;

        public int CompareTo(PathfindNode other) => Total.CompareTo(other.Total);

        public override bool Equals(object obj) =>
            obj is PathfindNode node &&
            Cell.Position.Equals(node.Cell.Position);

        public override int GetHashCode() => Cell.Position.GetHashCode();
    }
}
