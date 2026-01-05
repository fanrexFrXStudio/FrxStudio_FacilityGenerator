using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrxStudio.Generator
{
    public class Pathinder
    {
        private readonly Grid grid;

        private PathfindNode[,] nodes;

        private readonly List<PathfindNode> openedNodes = new();
        private readonly HashSet<PathfindNode> closedNodes = new();

        public Pathinder(Grid grid)
        {
            this.grid = grid;
        }

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
        public PathfindNode GetPath(CellPosition from, CellPosition to)
        {
            RefreshNodes();

            // Каждый проход по клетке стоит 1 G
            const int oneCellCost = 1;

            // алгоритм звезды использует колекции открытых и закрытых нод, для дальнейшей работы с ними
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

                foreach (var neighbord in grid.GetNeighbords(priorityNode.Cell.Position))
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
            {
                Debug.LogWarning("Path not founded");
                return null;
            }

            return founded;
        }
        
        private bool ValidateNeighbord(
            out PathfindNode founded,
            PathfindNode neighbord, PathfindNode priorityNode,
            CellPosition target,
            int oneCellCost = 1)
        {
            // если бы не было этой проверки - алгоритм всегда бы багался, так как не может
            // прийти к таргету, так как он считается занятой клеткой
            if (neighbord.Cell.Position == target)
            {
                // задаем родителя, иначе конструированние путя не сможет начаться, так как нету родителя 
                // (он задаеться ниже)
                neighbord.Parent = priorityNode;
                founded = neighbord;

                return true;
            }

            // есть ли клетка в закрытых? занята ли она?
            // то что это, может быть целью - уже учли выше. буквально на 1 пробел выше
            if (closedNodes.Contains(neighbord) || neighbord.Cell.IsBusy)
            {
                // не валидная нода для обработки, пропускаем до следущего соседа
                founded = null;
                return false;
            }

            var newG = priorityNode.CostFromStart + oneCellCost;
            var newGBest = false;

            if (openedNodes.Contains(neighbord) == false)
            {
                newGBest = true;

                // пушим этого соседа в открытые ноды, для дальнейшей обработки
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
