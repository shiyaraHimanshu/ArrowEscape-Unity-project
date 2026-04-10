using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace ArrowGameLevelEditor
{
    public class GridCell : MonoBehaviour, IPointerClickHandler,IPointerEnterHandler, IBeginDragHandler,IDragHandler, IEndDragHandler
    {
        public Image selectedImage;

        public int x;
        public int y;
        public int index;

        public readonly List<GridCell> neighbours = new();

        public bool isOccupied => ownerArrow != null;
        public GridArrow ownerArrow { get; private set; } = null;

        public bool isSelected { get; private set; }

        private GridManager grid;
        
        public static event System.Action<GridCell,bool> OnCellPointerClicked;
        public void Init(int x, int y, int id, GridManager manager)
        {
            this.x = x;
            this.y = y;
            index = id;
            grid = manager;

            gameObject.name = $"{index} ({x},{y})";

            neighbours.Clear();
            SetEmpty();
            SetSelected(false);
        }

        public void AddNeighbour(GridCell cell)
        {
            if (cell != null && !neighbours.Contains(cell))
                neighbours.Add(cell);
        }

        public bool SetOccupy(GridArrow arrow)
        {
            if (isOccupied)
                return false;

            ownerArrow = arrow;
            return true;
        }

        public void SetEmpty()
        {
            ownerArrow = null;
        }

        public void SetSelected(bool value)
        {
            isSelected = value;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (selectedImage != null)
                selectedImage.enabled = isSelected;
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                OnCellPointerClicked?.Invoke(this,true);
            else if (eventData.button == PointerEventData.InputButton.Right)
                OnCellPointerClicked?.Invoke(this,false);
        }

        public static event System.Action<GridCell> OnCellDragStart;
        public static event System.Action<GridCell> OnCellDragEnter;
        public static event System.Action<GridCell> OnCellDragEnd;
        private static bool isDragging;
        public void OnPointerEnter(PointerEventData eventData)
        {
            if(isDragging)
            {
                OnCellDragEnter?.Invoke(this);   
            }
        }      

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                isDragging=true;
                OnCellDragStart?.Invoke(this);
            }

        }
        public void OnDrag(PointerEventData eventData){}
        public void OnEndDrag(PointerEventData eventData)
        {
            if(isDragging)
            {
                OnCellDragEnd?.Invoke(this);      
            }
            isDragging=false;
        }
    }
}
