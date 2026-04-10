using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArrowGameLevelEditor
{
    public class GridArrow : MonoBehaviour
    {
        [Header("Visual")]
        public Image headImage;                 // Head image
        public UILineRenderer lineRenderer;      // Body line


        public List<GridCell> cells = new();
        public GridCell head => length > 0 ? cells[0] : null;
        public GridCell tail => length > 0 ? cells[^1] : null;
        public int colorIndex{ get; private set; }=0;
        public Color arrowColor => gridManager.GetArrowColor(colorIndex);
        public Vector2Int direction { get; private set; }
        public Vector3 rotationAngle { get; private set; } = Vector3.zero;
        public int length => cells.Count;
        public bool isValidArrow { get; private set; }

        private GridManager gridManager;

        public bool AddCellAtEnd(GridCell c)
        {
            if (!CanAddCell(c, atStart: false))
                return false;

            if (!c.SetOccupy(this))
                return false;

            cells.Add(c);
            UpdateDirection();
            CheckArrowValidation();
            UpdateUI();
            return true;
        }

        public bool AddCellAtStart(GridCell c)
        {
            if (!CanAddCell(c, atStart: true))
                return false;

            if (!c.SetOccupy(this))
                return false;

            cells.Insert(0, c);
            UpdateDirection();
            CheckArrowValidation();
            UpdateUI();
            return true;
        }

        public bool RemoveCellAtEnd()
        {
            if (cells.Count == 0)
                return false;

            var cell = cells[^1];
            cells.RemoveAt(cells.Count - 1);
            cell.SetEmpty();

            UpdateDirection();
            CheckArrowValidation();
            UpdateUI();
            return true;
        }

        public bool RemoveCellAtStart()
        {
            if (cells.Count == 0)
                return false;

            var cell = cells[0];
            cells.RemoveAt(0);
            cell.SetEmpty();

            UpdateDirection();
            CheckArrowValidation();
            UpdateUI();
            return true;
        }

        public bool ClearAllCells()
        {
            if (cells.Count == 0)
                return false;

            foreach (var c in cells)
                c.SetEmpty();

            cells.Clear();
            direction = Vector2Int.up;
            rotationAngle = Vector3.zero;
            
            CheckArrowValidation();
            UpdateUI();
            return true;
        }


        private bool CanAddCell(GridCell c, bool atStart)
        {
            if (c == null || c.isOccupied)
                return false;

            if (cells.Count == 0)
                return true;

            GridCell refCell = atStart ? cells[0] : cells[^1];
            return refCell.neighbours.Contains(c);
        }


        void UpdateDirection()
        {
            direction = Vector2Int.up;
            rotationAngle = Vector3.zero;

            if (cells.Count < 2)
                return;

            var head = cells[0];
            var next = cells[1];

            direction = new Vector2Int(
                head.x - next.x,
                head.y - next.y
            );

            if (direction == Vector2Int.up)
                rotationAngle = Vector3.zero;
            else if (direction == Vector2Int.right)
                rotationAngle = new Vector3(0, 0, -90f);
            else if (direction == Vector2Int.down)
                rotationAngle = new Vector3(0, 0, 180f);
            else if (direction == Vector2Int.left)
                rotationAngle = new Vector3(0, 0, 90f);
        }
        public void SetArrowColorIndex(int ci)
        {
            colorIndex = ci;
            UpdateUI();
        }
        public void Init(GridManager grid)
        {
            gridManager=grid;
            headImage.GetComponent<RectTransform>().sizeDelta=Vector2.one*((int)gridManager.gridCellSize*.6f);
            lineRenderer.LineThickness=(int)(gridManager.gridCellSize*.2f);
        }
        public void SetData(ArrowData data)
        {
            ClearAllCells();
            colorIndex=data.ColorIndex;
            foreach (int index in data.Indices)
            {
                var cell = gridManager.GetCell(index);
                if (cell != null && cell.SetOccupy(this))
                    cells.Add(cell);
            }

            UpdateDirection();
            CheckArrowValidation();
            UpdateUI();
        }

        public void GetData(ArrowData data)
        {
            if (cells.Count == 0)
                return;

            var h = cells[0];

            data.Indices = new List<int>();
            foreach (var c in cells)
                data.Indices.Add(c.index);

            data.ColorIndex = colorIndex;
        }
        [ContextMenu("Update")]
        public void UpdateUI()
        {
            
            if (cells.Count >= 2)
            {
                headImage.transform.localPosition=head.transform.localPosition;
                headImage.rectTransform.localEulerAngles = rotationAngle;

                RectTransform lineRect = lineRenderer.rectTransform;
                Vector2[] points = new Vector2[cells.Count];

                for (int i = 0; i < cells.Count; i++)
                {
                    points[i] = cells[i].transform.localPosition;
                }

                lineRenderer.Points = points;
                lineRenderer.SetAllDirty();
                headImage.color = arrowColor;
                lineRenderer.color = arrowColor;
            }
            else if (cells.Count == 1)
            {
                headImage.transform.localPosition=head.transform.localPosition;
                RectTransform lineRect = lineRenderer.rectTransform;
                Vector2[] points = new Vector2[0];
                lineRenderer.Points = points;
                lineRenderer.SetAllDirty();
                headImage.color = arrowColor;
                lineRenderer.color = arrowColor;
            }
                
            if(isValidArrow)
            {
                lineRenderer.sprite=null;
                lineRenderer.ImproveResolution=ResolutionMode.None;
            }
            else
            {                
                lineRenderer.sprite=gridManager.dashedArrowSprite;
                lineRenderer.ImproveResolution=ResolutionMode.PerSegment;
                lineRenderer.Resolution=2;
            }
        }
        [ContextMenu("Test")]
        void CheckArrowValidation()
        {
            isValidArrow = false;

            if (cells == null || cells.Count < 2 || gridManager == null || head == null)
                return;

            // Calculate next head position
            int nextX = head.x + direction.x;
            int nextY = head.y + direction.y;

            if(nextX >=0 && nextX < gridManager.gridX &&nextY >=0 && nextY < gridManager.gridY)
            {                
                int nextIndex = nextY * gridManager.gridX + nextX;
                GridCell nextCell = gridManager.GetCell(nextIndex);

            // Invalid if head is blocked by its own body
            if (nextCell != null && cells.Contains(nextCell))
                return;
            }

            isValidArrow = true;
        }


    }
}
