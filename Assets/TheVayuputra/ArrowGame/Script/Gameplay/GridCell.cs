using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
namespace ArrowGame
{

public class GridCell : MonoBehaviour
{
    [Header("Reference")]
    public Collider touchCollider;
    public LineRenderer lineRenderer;
    public SpriteRenderer innerPointSprite;

    public int cellId;
    public Vector2Int gridPos;
    public List<NearbyCell> nearbyCells = new List<NearbyCell>();
    public Arrow occupiedBy = null;
    private LevelManager levelManager=>LevelManager.Instance;

    public static event System.Action<GridCell> OnCellPointerClicked;
    private int innerPointColorIndex;
    void OnValidate()
    {
        if(touchCollider == null) touchCollider=GetComponent<Collider>();
        if(lineRenderer == null) lineRenderer=GetComponent<LineRenderer>();
        touchCollider.isTrigger = true;
    }
    void OnEnable()    {
        
        LevelManager.Instance.isShowGridLines.AddListener(UpdateGridLine);        
        GameData.IsDayTheme.AddListener(OnThemeChange);
    }
    void OnDisable()
    {
        LevelManager.Instance.isShowGridLines.RemoveListener(UpdateGridLine);        
        GameData.IsDayTheme.RemoveListener(OnThemeChange);
    }
    public void Init(int id,Vector2Int pos)
    {
        cellId = id;
        gridPos = pos;
        innerPointSprite.enabled=false;
        innerPointSprite.color=Color.white;
        innerPointColorIndex=0;
        SetEmpty();
    }
    void UpdateGridLine(bool isShowGridLines)
    {
        bool isShowLine=isShowGridLines && occupiedBy != null && occupiedBy.headCell == this;
        if(isShowLine)
        {
            Vector3 start=transform.position;
            Vector3 end=transform.position + (new Vector3(occupiedBy.arrowDirection.x,0,occupiedBy.arrowDirection.y) *2*levelManager.gridManager.gameBorder);
           
            lineRenderer.sortingOrder=LevelManager.gridLineSortingOrder;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(new Vector3[]{start,end});
            lineRenderer.enabled=true;  
        }
        else
        {
            lineRenderer.enabled=false;   
        }
    }
    public void OnThemeChange(bool isDayTheme)
    {
        var color = isDayTheme
            ? GameplayExtensionMethods.GetDayThemeArrowColor(innerPointColorIndex)
            : GameplayExtensionMethods.GetNightThemeArrowColor(innerPointColorIndex);
        color.a=innerPointSprite.color.a;
        innerPointSprite.color=color;
    }
    public void SetOccupied(Arrow a)
    {
        touchCollider.enabled=true;
        occupiedBy = a;
        innerPointColorIndex =a.colorIndex;
        OnThemeChange(GameData.IsDayTheme.Value);
        UpdateGridLine(levelManager.isShowGridLines.Value);
    }
    public void SetEmpty()
    {
        touchCollider.enabled=false;
        occupiedBy = null;
        UpdateGridLine(levelManager.isShowGridLines.Value);
    }
    public void EnableInnerPoint(float animTime=.2f)
    {
        Color color = innerPointSprite.color;
        color.a=0;
        float scale=innerPointSprite.transform.localScale.x;
        
        innerPointSprite.transform.DOKill();
        innerPointSprite.DOKill();

        innerPointSprite.enabled=true;
        innerPointSprite.transform.localScale = Vector3.zero;
        innerPointSprite.color=color;       

        float appearTime = animTime * 0.7f;
        float settleTime = animTime * 0.3f;

        Sequence seq = DOTween.Sequence();
        seq.Append(innerPointSprite.transform.DOScale(scale* 1.2f, appearTime).SetEase(Ease.OutBack));
        seq.Join(innerPointSprite.DOFade(1f, appearTime).SetEase(Ease.OutCubic));
        seq.Append(innerPointSprite.transform.DOScale(scale, settleTime).SetEase(Ease.OutCubic));
    }
    public void DisableInnerPoint(float animTime=.3f)
    { 
        if( !innerPointSprite.enabled) return;
        innerPointSprite.transform.DOKill();
        innerPointSprite.DOKill();

        float expandTime = animTime * 0.3f;
        float shrinkTime = animTime * 0.7f;

        Sequence seq = DOTween.Sequence();
        seq.Append( innerPointSprite.transform.DOScale(innerPointSprite.transform.localScale.x*3f, expandTime).SetEase(Ease.OutBack));
        seq.Append( innerPointSprite.transform.DOScale(0f, shrinkTime).SetEase(Ease.InCubic));
        seq.Join(innerPointSprite.DOFade(0f, shrinkTime).SetEase(Ease.InCubic));
    }


    public bool IsOccupied()
    {
        return occupiedBy != null;
    }

    public override string ToString()
    {
        return $"{cellId} - {gridPos}";
        // return $"Cell[id={cellId} pos={gridPos}]";
    }
    void OnMouseUpAsButton()
    {
        OnCellPointerClicked?.Invoke(this);
    }


}

[System.Serializable]
public struct NearbyCell
{
    public Vector2Int direction;
    public GridCell cell;
}
}