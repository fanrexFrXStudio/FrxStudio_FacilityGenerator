using System;
using System.Collections.Generic;

namespace FrxStudio.Generator
{
    /// <summary>
    /// Find path from a to b ( by algorithm A* )
    /// </summary>
    public class Pathfinder
    {
        private readonly Grid grid;

        private PathfindNode[,] nodes;

        private readonly List<PathfindNode> openedNodes = new();
        private readonly HashSet<PathfindNode> closedNodes = new();

        public Pathfinder(Grid grid)
        {
            this.grid = grid;
        }

        /// <summary>
        /// Clear all nodes data
        /// </summary>
        private void RefreshNodes()
        {
            openedNodes.Clear();
            closedNodes.Clear();

            nodes = new PathfindNode[grid.CellsCountX, grid.CellsCountY];

            for (var x = 0; x < grid.CellsCountX; x++)
            {
                for (var y = 0; y < grid.CellsCountY; y++)
                {
                    var cell = grid.GetCell(x, y);

                    if (cell == null)
                        continue;

                    var px = cell.Position.X;
                    var py = cell.Position.Y;

                    nodes[px, py] = new PathfindNode
                    {
                        Cell = cell,
                        Parent = null,
                        CostFromStart = int.MaxValue,
                        HeuristicCost = 0
                    };
                }
            }
        }

        // RU DOC
        /// <returns>Path from a to b, bypassing occupied cells and the front part</returns>
        public PathfindNode GetPath(CellPosition from, CellPosition to)
        {
            RefreshNodes();

            // Каждый проход по клетке стоит 1 G
            // Every passage by cell cost 1 G
            const int oneCellCost = 1;

            // алгоритм звезды использует коллекции открытых и закрытых нод, для дальнейшей работы с ними
            // сама суть и есть эти списки

            // так как поиск пути работает пока в открытом списке есть комнаты, нам нужно
            // указать первую комнату - точку старта
            openedNodes.Add(new()
            {
                Cell = grid.GetCell(from),
                Parent = null,
                CostFromStart = 0,
                HeuristicCost = grid.GetManhattan(from, to)
            });

            PathfindNode founded = null;

            while (openedNodes.Count > 0)
            {
                // нода с меньшой эвристикой ( примерной дистанцией до цели )
                openedNodes.Sort();
                var priorityNode = openedNodes[0];

                openedNodes.RemoveAt(0);
                closedNodes.Add(priorityNode);

                if (priorityNode.Cell.Position == to)
                {
                    founded = priorityNode;
                    break;
                }

                foreach (var neighbord in grid.GetNeighbors(priorityNode.Cell.Position))
                {
                    var node = nodes[neighbord.X, neighbord.Y];

                    if (ValidateNeighbord(
                        out founded,
                        node,
                        priorityNode,
                        to,
                        oneCellCost))
                    {
                        break;
                    }
                }

                if (founded != null)
                    break;               
            }

            if (founded == null)
                return null;

            return founded;
        }

        /// <returns>Is valid neighbord?</returns>
        private bool ValidateNeighbord(
            out PathfindNode founded,
            PathfindNode neighbord,
            PathfindNode priorityNode,
            CellPosition target,
            int oneCellCost = 1)
        {
            // если бы не было этой проверки - алгоритм всегда бы багался
            if (neighbord.Cell.Position == target)
            {
                neighbord.Parent = priorityNode;
                founded = neighbord;
                return true;
            }

            // есть ли клетка в закрытых? занята ли она?
            if (closedNodes.Contains(neighbord) || neighbord.Cell.IsBusy)
            {
                founded = null;
                return false;
            }

            foreach (var dir in GridExtension.CachedDirections)
            {
                var adjacentPos = grid.GetNext(neighbord.Cell.Position, dir);

                if (adjacentPos == CellPosition.Invalid)
                    continue;

                var adjacentCell = grid.GetCell(adjacentPos);

                if (adjacentCell == Cell.Invalid || !adjacentCell.IsBusy)
                    continue;

                // Если соседняя клетка - это зарезервированная часть большой комнаты
                if (adjacentCell.Owner.IsRoomReserved)
                {
                    // Блокируем проход, так как нельзя идти вплотную к дополнительным клеткам
                    // (они часть одной большой комнаты)
                    founded = null;
                    return false;
                }

                // Проверка на фронт основной клетки комнаты
                var frontDir = grid.GetDirectionBetween(adjacentCell.Position, neighbord.Cell.Position);

                if (adjacentCell.Owner.InstanceDirection == frontDir)
                {
                    founded = null;
                    return false;
                }
            }

            var newG = priorityNode.CostFromStart + oneCellCost;
            var newGBest = false;

            if (openedNodes.Contains(neighbord) == false)
            {
                newGBest = true;
                neighbord.HeuristicCost = grid.GetManhattan(neighbord.Cell.Position, target);
                openedNodes.Add(neighbord);
            }
            else if (newG < neighbord.CostFromStart)
                newGBest = true;

            if (newGBest)
            {
                neighbord.Parent = priorityNode;
                neighbord.CostFromStart = newG;
            }

            founded = null;
            return false;
        }
    }
}
