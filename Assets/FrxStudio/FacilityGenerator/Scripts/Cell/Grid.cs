using UnityEditor;
using UnityEngine;

namespace FrxStudio.Generator
{
    /// <summary>
    /// The container of cells
    /// </summary>
    public class Grid : IGizmoDrawable
    {
        #region Var

        public readonly int CellsCountX, CellsCountY;

        private readonly Cell[,] cells;
        private readonly ScriptableGeneratorPreset preset;
        private readonly float worldYPosition;

        #region Gizmo
#if UNITY_EDITOR

        private readonly Color cellWireColor = new(1, 0, 0, .5f);
        private readonly Color cellFrameColor = new(1, 0, 0, .75f);

        private readonly Color cellEmptyColor = new(0, 1, 1, .1f);
        private readonly Color cellBusyColor = new(1, 1, 1, .5f);

        private readonly Color cellAdditionalColor = new(1, 0, 0, .75f);

        // text of cell position on editor
        private static readonly GUIStyle сellLabelStyle = new()
        {
            // white, 100% transparent
            normal = { textColor = new(1, 1, 1, 1) },
            fontSize = 10,
            alignment = TextAnchor.MiddleCenter,
        };

#endif
        #endregion

        #endregion

        #region Type

        public Grid(ScriptableGeneratorPreset preset, float worldYPosition)
        {
            this.preset = preset;
            this.worldYPosition = worldYPosition;

            CellsCountX = preset.CellsCountX;
            CellsCountY = preset.CellsCountY;

            cells = new Cell[preset.CellsCountX, preset.CellsCountY];

            for (var x = 0; x < preset.CellsCountX; x++)
            {
                for (var y = 0; y < preset.CellsCountY; y++)
                {
                    cells[x, y] = new(x, y, GetCellWorldPosition(x, y));
                }
            }
        }

        #endregion
        
        #region Logic

        public void SetCell(CellPosition position, Cell target)
        {
            var cell = GetCell(position);

            if (cell != Cell.Invalid)
                cells[position.X, position.Y] = target;
        }

        public Cell GetCell(CellPosition position) => GetCell(position.X, position.Y);

        public Cell GetCell(int x, int y)
        {
            if (cells.InBounds(x, y))
                return cells[x, y];

            return Cell.Invalid;
        }

        private Vector3 GetCellWorldPosition(int posX, int posY)
        {
            var oneCellSize = preset.CellSize;
            var step = oneCellSize + preset.CellSpacing;
            var half = oneCellSize * 0.5f;

            return new Vector3(
                posX * step + half,
                worldYPosition,
                posY * step + half
            );
        }

        #endregion

        #region Debug

#if UNITY_EDITOR

        public void DrawGizmo()
        {
            if (cells == null || cells.Length <= 0)
                return;

            // cell color
            Gizmos.color = Color.red;

            foreach (var cell in cells)
            {
                // wire
                Gizmos.color = cellWireColor;
                Gizmos.DrawWireCube(cell.Position.WorldPosition, Vector3.one * preset.CellSize);

                if (!cell.IsBusy)
                {
                    Gizmos.color = cellEmptyColor;
                    Gizmos.DrawCube(cell.Position.WorldPosition, Vector3.one * preset.CellSize);
                }
                else
                {
                    if (cell.Owner.IsRoomReserved)
                    {
                        Gizmos.color = cellAdditionalColor;
                        Gizmos.DrawCube(cell.Position.WorldPosition, Vector3.one * preset.CellSize);
                    }
                    else
                    {
                        Gizmos.color = cellBusyColor;
                        Gizmos.DrawCube(cell.Position.WorldPosition, Vector3.one * preset.CellSize);
                    }
                }

                // draw text
                Handles.Label(
                    cell.Position.WorldPosition,
                    $"X: ({cell.Position.X}, Y: {cell.Position.Y})",
                    сellLabelStyle);

                Gizmos.color = cellFrameColor;

                // draw frame
                var pos = cell.Position.WorldPosition;
                var half = preset.CellSize * 0.5f;

                var topLeft = pos + new Vector3(-half, 0, half);
                var topRight = pos + new Vector3(half, 0, half);
                var bottomLeft = pos + new Vector3(-half, 0, -half);
                var bottomRight = pos + new Vector3(half, 0, -half);

                Gizmos.DrawLine(topLeft, topRight);
                Gizmos.DrawLine(topRight, bottomRight);
                Gizmos.DrawLine(bottomRight, bottomLeft);
                Gizmos.DrawLine(bottomLeft, topLeft);
            }
        }
#endif

        #endregion
    }
}
