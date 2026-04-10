using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArrowGameLevelEditor
{
    public class GridManager : MonoBehaviour
    {
        [Header("Color Setting")]
        public List<Color> arrowColors = new()
        {
            new Color32(94,230,255,255),
            new Color32(108,255,181,255),
            new Color32(111,168,255,255),
            new Color32(180,140,255,255),
            new Color32(255,158,122,255),
            new Color32(255,217,102,255),
            new Color32(255,127,168,255),
            new Color32(77,255,210,255),
            new Color32(201,182,255,255),
            new Color32(182,255,106,255)
        };
        [Header("Control UI Reference")]
        [Space]
        public CanvasGroup gridSizeContent;   // Grid size input
        public TMP_InputField gridSizeX;
        public TMP_InputField gridSizeY;
        [Space]
        public TextMeshProUGUI totalArrowLbl;
        public CanvasGroup arrowDetailContent;
        public TextMeshProUGUI arrowLengthLbl;
        public TextMeshProUGUI arrowColorIndex;
        public Image arrowColorImage;


        [Header("References")]
        public RectTransform gridCellParent;
        public GridLayoutGroup gridLayout;
        public GridCell cellPrefab;
        public GridArrow arrowPrefab;
        public Transform gridArrowParent;
        public Sprite dashedArrowSprite;

        [Header("Grid Size")]
        public int gridX = 10;
        public int gridY = 10;

        [Header("Arrows")]
        public List<GridArrow> arrows = new();
        private Dictionary<int, GridCell> cells = new();
        [Header("Selection")]
        public List<GridCell> selectedCells = new();
        public GridArrow selectedArrow = null;
        public float gridCellSize = 100;

        public void ResetGrid()
        {
            foreach (Transform c in gridCellParent)
                Destroy(c.gameObject);
            foreach (Transform c in gridArrowParent)
                Destroy(c.gameObject);
            cells.Clear();
            arrows.Clear();
        }
        public void OnClick_CreateGridSize()
        {
            int x = int.Parse(gridSizeX.text);
            int y = int.Parse(gridSizeY.text);
            if (x == 0 || y == 0)
            {
                LevelEditorController.levelEditorController.ShowToast("Invalid Grid Size");
                return;
            }
            CreateGrid(x, y);
        }

        public void CreateGrid(int sizeX, int sizeY)
        {
            ResetGrid();

            gridX = sizeX;
            gridY = sizeY;

            gridSizeX.text = $"{gridX}";
            gridSizeY.text = $"{gridY}";

            ApplyGridLayoutSize();
            for (int y = 0; y < gridY; y++)
            {
                for (int x = 0; x < gridX; x++)
                {
                    int index = (y * gridX) + x;
                    var cell = Instantiate<GridCell>(cellPrefab, gridCellParent);
                    cell.Init(x, y, index, this);
                    cells[cell.index] = cell;
                }
            }
            SetupNeighbours();
            ClearSelection();
            UpdateControlUI();
        }


        public void SetupLevelArrows(List<ArrowData> arrowsData)
        {

            if (arrowsData == null)
                return;

            foreach (var data in arrowsData)
            {
                GridArrow arrow = Instantiate(arrowPrefab, gridArrowParent);
                arrow.Init(this);
                arrow.SetData(data);

                if (arrow.length == 0)
                {
                    Destroy(arrow.gameObject);
                    continue;
                }

                arrows.Add(arrow);
            }
        }
        public GridArrow CreateArrow(List<GridCell> arrowCells)
        {
            if (arrowCells == null || arrowCells.Count == 0)
                return null;

            for (int i = 0; i < arrowCells.Count; i++)
            {
                if (arrowCells[i] == null || arrowCells[i].isOccupied)
                    return null;

                if (i > 0 && !arrowCells[i - 1].neighbours.Contains(arrowCells[i]))
                    return null;
            }

            GridArrow arrow = Instantiate(arrowPrefab, gridArrowParent);
            arrow.Init(this);
            foreach (var cell in arrowCells)
            {
                if (!arrow.AddCellAtEnd(cell))
                {
                    arrow.ClearAllCells();
                    Destroy(arrow.gameObject);
                    return null;
                }
            }
            arrows.Add(arrow);
            return arrow;
        }
        public bool DeleteArrow(GridArrow arrow)
        {
            if (arrow == null || !arrows.Contains(arrow))
                return false;

            arrow.ClearAllCells();
            arrows.Remove(arrow);
            Destroy(arrow.gameObject);

            return true;
        }

        private void SetupNeighbours()
        {
            foreach (var cell in cells.Values)
            {
                TryAddNeighbour(cell, cell.x + 1, cell.y);
                TryAddNeighbour(cell, cell.x - 1, cell.y);
                TryAddNeighbour(cell, cell.x, cell.y + 1);
                TryAddNeighbour(cell, cell.x, cell.y - 1);
            }
        }

        private void TryAddNeighbour(GridCell cell, int x, int y)
        {
            if (x < 0 || x >= gridX || y < 0 || y >= gridY)
                return;

            int index = y * gridX + x;
            if (cells.TryGetValue(index, out var neighbour))
                cell.AddNeighbour(neighbour);
        }

        public GridCell GetCell(int index)
        {
            cells.TryGetValue(index, out var cell);
            return cell;
        }

        private void ApplyGridLayoutSize()
        {
            if (!gridLayout) return;

            float width = gridCellParent.rect.width;
            float height = gridCellParent.rect.height;

            float cellWidth = width / gridX;
            float cellHeight = height / gridY;

            gridCellSize = Mathf.Min(cellWidth, cellHeight);
            gridLayout.cellSize = new Vector2(gridCellSize, gridCellSize);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = gridX;
        }


        void SelectCell(GridCell cell)
        {
            if (cell == null || selectedCells.Contains(cell))
                return;

            selectedCells.Add(cell);
            cell.SetSelected(true);
        }
        void SelectCells(List<GridCell> cellsToSelect)
        {
            if (cellsToSelect == null || cellsToSelect.Count == 0)
                return;

            // Clear previous selection
            ClearSelection();

            foreach (var cell in cellsToSelect)
            {
                SelectCell(cell);
            }
        }


        void DeselectCell(GridCell cell)
        {
            if (cell == null || !selectedCells.Contains(cell))
                return;

            selectedCells.Remove(cell);
            cell.SetSelected(false);
        }
        void DeselectCells(List<GridCell> cellsToDeselect)
        {
            if (cellsToDeselect == null || cellsToDeselect.Count == 0)
                return;

            foreach (var cell in cellsToDeselect)
            {
                DeselectCell(cell);
            }
        }

        public void ClearSelection()
        {
            foreach (var cell in selectedCells)
            {
                if (cell != null)
                    cell.SetSelected(false);
            }

            selectedCells.Clear();
        }


        private void OnEnable()
        {
            GridCell.OnCellPointerClicked += HandleCellClicked;
            GridCell.OnCellDragStart += HandleCellDragStart;
            GridCell.OnCellDragEnter += HandleCellDragEnter;
            GridCell.OnCellDragEnd += HandleCellDragEnd;
        }

        private void OnDisable()
        {
            GridCell.OnCellPointerClicked -= HandleCellClicked;
            GridCell.OnCellDragStart -= HandleCellDragStart;
            GridCell.OnCellDragEnter -= HandleCellDragEnter;
            GridCell.OnCellDragEnd -= HandleCellDragEnd;
        }

        private void HandleCellClicked(GridCell cell, bool isLeftClick)
        {
            if (cell == null) return;
            if (isLeftClick)
            {
                if (selectedArrow != null)
                {
                    if (selectedArrow.cells.Contains(cell))
                    {
                        if (selectedArrow.head == cell)
                        {
                            selectedArrow.RemoveCellAtStart();
                            DeselectCell(cell);
                            if (selectedArrow.length < 2)
                            {
                                var arrow = selectedArrow;
                                SetSelectArrow(null);
                                DeleteArrow(arrow);
                            }
                        }
                        else if (selectedArrow.tail == cell)
                        {
                            selectedArrow.RemoveCellAtEnd();
                            DeselectCell(cell);
                            if (selectedArrow.length < 2)
                            {
                                var arrow = selectedArrow;
                                SetSelectArrow(null);
                                DeleteArrow(arrow);
                            }
                        }
                    }
                    else
                    {
                        if (!cell.isOccupied && selectedArrow.head.neighbours.Contains(cell) && !selectedArrow.tail.neighbours.Contains(cell))
                        {
                            selectedArrow.AddCellAtStart(cell);
                            SelectCell(cell);
                        }
                        else if (!cell.isOccupied && selectedArrow.tail.neighbours.Contains(cell))
                        {
                            selectedArrow.AddCellAtEnd(cell);
                            SelectCell(cell);
                        }
                        else
                        {
                            SetSelectArrow(null);
                            OnCellClick(cell);
                        }
                    }
                }
                else
                {
                    OnCellClick(cell);
                }
            }
            else
            {
                if (selectedArrow != null)
                {
                    SetSelectArrow(null);
                }
            }
            UpdateControlUI();
        }
        private void OnCellClick(GridCell cell)
        {
            if (cell.isOccupied)
            {
                SetSelectArrow(cell.ownerArrow);
            }
            else
            {
                if (selectedCells.Count == 0)
                {
                    SelectCell(cell);
                }
                else if (selectedCells.Count == 1)
                {
                    if (cell.isSelected)
                    {
                        DeselectCell(cell);
                    }
                    else if (selectedCells[0].neighbours.Contains(cell))
                    {
                        SelectCell(cell);
                        SetSelectArrow(CreateArrow(selectedCells));
                    }
                    else
                    {
                        DeselectCell(selectedCells[0]);
                        SelectCell(cell);
                    }
                }
            }
        }
        private void SetSelectArrow(GridArrow arrow)
        {
            if (selectedArrow != null)
            {
                DeselectCells(selectedArrow.cells);
                selectedArrow = null;
            }
            if (arrow != null)
            {
                selectedArrow = arrow;
                SelectCells(selectedArrow.cells);
            }
        }

        private bool isCreateArrowByDrag = false;
        private void HandleCellDragStart(GridCell cell)
        {
            if (cell == null) return;

            if (selectedArrow != null && selectedArrow.tail == cell)
            {
                isCreateArrowByDrag = true;
            }
            else if (!cell.isOccupied)
            {
                SetSelectArrow(CreateArrow(new List<GridCell>() { cell }));
                isCreateArrowByDrag = true;
            }
            else if (cell.isOccupied && cell.ownerArrow.tail == cell)
            {
                SetSelectArrow(cell.ownerArrow);
                isCreateArrowByDrag = true;
            }
            UpdateControlUI();

        }
        private void HandleCellDragEnter(GridCell cell)
        {
            if (cell == null || !isCreateArrowByDrag || selectedArrow == null) return;
            if (selectedArrow.tail.neighbours.Contains(cell))
            {
                if (!cell.isOccupied)
                {
                    selectedArrow.AddCellAtEnd(cell);
                    SelectCell(cell);
                }
                else if (cell.isOccupied && selectedArrow.cells.Contains(cell) && selectedArrow.length != 1)
                {
                    var tailCell = selectedArrow.tail;
                    selectedArrow.RemoveCellAtEnd();
                    DeselectCell(tailCell);
                }
            }
            else
            {
                EndCreateArrowByDrag();
            }
            UpdateControlUI();
        }
        private void HandleCellDragEnd(GridCell cell)
        {
            if (isCreateArrowByDrag)
                EndCreateArrowByDrag();

            UpdateControlUI();
        }
        private void EndCreateArrowByDrag()
        {

            isCreateArrowByDrag = false;
            if (selectedArrow != null && selectedArrow.length <= 1)
            {
                var arrow = selectedArrow;
                SetSelectArrow(null);
                DeleteArrow(arrow);
            }
        }
        // Control ---------
        public void UpdateControlUI()
        {
            gridSizeContent.SetCanvasInteractable(arrows.Count == 0);
            totalArrowLbl.text = $"{arrows.Count}";
            if (selectedArrow)
            {
                arrowDetailContent.alpha = 1; ;
                arrowDetailContent.SetCanvasInteractable(true);
                arrowLengthLbl.text = $"{selectedArrow.length}";
                arrowColorIndex.text = $"{selectedArrow.colorIndex}";
                arrowColorImage.color = selectedArrow.arrowColor;

            }
            else
            {
                arrowDetailContent.alpha = 0;
                arrowDetailContent.SetCanvasInteractable(false);
            }
        }
        public void OnClick_DeleteSelectedArrow()
        {
            var arrow = selectedArrow;
            SetSelectArrow(null);
            DeleteArrow(arrow);
            UpdateControlUI();
        }
        public void OnClick_DeselectSelectedArrow()
        {
            if (selectedArrow != null)
            {
                SetSelectArrow(null);
                UpdateControlUI();
            }
        }
        public void OnClick_NextColor()
        {
            selectedArrow.SetArrowColorIndex((selectedArrow.colorIndex + 1) % arrowColors.Count);
            UpdateControlUI();
        }
        public void OnClick_PreColor()
        {
            if (selectedArrow.colorIndex == 0)
            {
                selectedArrow.SetArrowColorIndex(arrowColors.Count - 1);

            }
            else
            {
                selectedArrow.SetArrowColorIndex(selectedArrow.colorIndex - 1);
            }
            UpdateControlUI();
        }
        public void OnClick_ApplyRandomColorToAll()
        {
            for (int i = 0; i < arrows.Count; i++)
            {
                arrows[i].SetArrowColorIndex(Random.Range(0, arrowColors.Count));
            }
            UpdateControlUI();
        }
        public Color GetArrowColor(int colorIndex)
        {
            if (arrowColors == null || arrowColors.Count == 0)
                return Color.white;

            // Handle negative & large indices safely
            int safeIndex = colorIndex % arrowColors.Count;

            return arrowColors[safeIndex];
        }
    }
}
