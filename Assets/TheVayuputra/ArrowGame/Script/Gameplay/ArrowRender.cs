using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
namespace ArrowGame
{
    [RequireComponent(typeof(Arrow))]
    [RequireComponent(typeof(LineRenderer))]
    public class ArrowRender : MonoBehaviour 
    {
        public Arrow arrow;
        public LineRenderer lineRenderer;        
        public Material normalLineMaterial;
        public Material dashedLineMaterial;
        public Transform headTransform;
        public Transform tailTransform;

        public SpriteRenderer headSpriteRenderer;
        public SpriteRenderer tailSpriteRenderer;


        private LevelManager levelManager=>LevelManager.Instance;
        private Tween highlightTween;
        private Color baseArrowColor;

        private void Awake()
        {
            if (arrow == null) arrow = GetComponent<Arrow>();
            lineRenderer = GetComponent<LineRenderer>();

            if (lineRenderer == null)
                lineRenderer = gameObject.AddComponent<LineRenderer>();

            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = true;
            lineRenderer.loop = false;
            lineRenderer.enabled=false;
        }

        private void OnEnable()
        {
            arrow.OnArrowUpdate += HandleArrowChanged;
            arrow.OnArrowSetupEffect += HandleArrowSetupEffect;

            arrow.isMoving.AddListener(UpdateSortingOrder);
            arrow.isMarkedAsFailedEscaped.AddListener(UpdateLineMaterial);
            arrow.isHighlighted.AddListener(HandleArrowHighLight);  
            GameData.IsDayTheme.AddListener(OnThemeChange);
            
            headTransform.localScale=Vector3.zero;
            tailTransform.localScale=Vector3.zero;

        }
        private void OnDisable()
        {
            StopHighlight();
            arrow.OnArrowUpdate -= HandleArrowChanged;
            arrow.OnArrowSetupEffect -= HandleArrowSetupEffect;
            
            arrow.isMoving.RemoveListener(UpdateSortingOrder);
            arrow.isMarkedAsFailedEscaped.RemoveListener(UpdateLineMaterial);
            arrow.isHighlighted.RemoveListener(HandleArrowHighLight);
            GameData.IsDayTheme.RemoveListener(OnThemeChange);
        }
        void UpdateSortingOrder(bool isMoving)
        {
            if(isMoving)
            {            
                lineRenderer.sortingOrder =LevelManager.arrowSortingOrderMoving;
                headSpriteRenderer.sortingOrder = LevelManager.arrowSortingOrderMoving+1;        
                tailSpriteRenderer.sortingOrder = LevelManager.arrowSortingOrderMoving+1;        
            }
            else
            {
                lineRenderer.sortingOrder =LevelManager.arrowSortingOrderNormal;
                headSpriteRenderer.sortingOrder = LevelManager.arrowSortingOrderNormal+1;  
                tailSpriteRenderer.sortingOrder = LevelManager.arrowSortingOrderNormal+1;        

            }
        }
        void UpdateLineMaterial(bool isFailedEscaped)
        {
            lineRenderer.material =isFailedEscaped?dashedLineMaterial:normalLineMaterial;
        }
        private void UpdateHeadAndTailTransform()
        {
            headTransform.position = arrow.parts[0].position;                
            headTransform.eulerAngles = new Vector3(headTransform.eulerAngles.x,arrow.parts[0].rotation.eulerAngles.y,headTransform.eulerAngles.z);
            
            tailTransform.position = arrow.parts[^1].position;                
            tailTransform.eulerAngles = new Vector3(tailTransform.eulerAngles.x,arrow.parts[^1].rotation.eulerAngles.y,tailTransform.eulerAngles.z);
                
        }
        private void HandleArrowChanged()
        {
            if(arrow.isRequiredToRender)
            {           
                UpdateHeadAndTailTransform();
                List<Vector3> points = new List<Vector3>();
                for (int i = 0; i < arrow.parts.Count; i++)
                {
                    points.Add(arrow.parts[i].position);
                }
                lineRenderer.positionCount = points.Count;
                lineRenderer.SetPositions(points.ToArray());
                lineRenderer.enabled=true;
            }
            else
            {
                lineRenderer.enabled=false;
            }
        }
        
        public void OnThemeChange(bool isDayTheme)
        {
            baseArrowColor = isDayTheme
                ? GameplayExtensionMethods.GetDayThemeArrowColor(arrow.colorIndex)
                : GameplayExtensionMethods.GetNightThemeArrowColor(arrow.colorIndex);

            ApplyArrowColor(baseArrowColor);
            HandleArrowHighLight(arrow.isHighlighted.Value);
        }
        private void ApplyArrowColor(Color c)
        {
            lineRenderer.startColor = c;
            lineRenderer.endColor = c;
            headSpriteRenderer.color = c;
            tailSpriteRenderer.color = c;
        }
        
        private void HandleArrowSetupEffect(float animTime=.5f)
        {
            List<Vector3> points = new List<Vector3>();   
            for (int i = arrow.parts.Count - 1; i >= 0 ; i--)
            {
                points.Add(arrow.parts[i].position);
            }           
            StartCoroutine(DrawLineForStartEffect(points,animTime));
        }
        private IEnumerator DrawLineForStartEffect(List<Vector3> points, float animTime)
        {
             int count = points.Count;
            lineRenderer.enabled = true;
            float elapsed = 0f;
            baseArrowColor = GameData.IsDayTheme.Value
                ? GameplayExtensionMethods.GetDayThemeArrowColor(arrow.colorIndex)
                : GameplayExtensionMethods.GetNightThemeArrowColor(arrow.colorIndex);

            ApplyArrowColor(baseArrowColor);
            UpdateHeadAndTailTransform();
            tailTransform.DOScale(Vector3.one, .1f).SetEase(Ease.InOutQuad);
            while (elapsed < animTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / animTime);

                float totalSegments = (count - 1) * t;
                int completedSegments = Mathf.FloorToInt(totalSegments);
                float segmentT = totalSegments - completedSegments;

                int positionCount = Mathf.Clamp(completedSegments + 2, 2, count);
                lineRenderer.positionCount = positionCount;

                for (int i = 0; i <= completedSegments; i++)
                {
                    lineRenderer.SetPosition(i, points[i]);
                }

                if (completedSegments < count - 1)
                {
                    lineRenderer.SetPosition(completedSegments + 1,Vector3.Lerp(points[completedSegments],points[completedSegments + 1],segmentT));
                }
                yield return null;
            }

            lineRenderer.positionCount = count;
            for (int i = 0; i < count; i++)
                lineRenderer.SetPosition(i, points[i]);

            headTransform.DOScale(Vector3.one, .1f).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                OnThemeChange(GameData.IsDayTheme.Value);                
            });
        }

        
        private void HandleArrowHighLight(bool isHighlighted)
            {
                if (isHighlighted)
                    RestartHighlight();
                else
                    StopHighlight();
            }
            private void StopHighlight()
            {
                if (highlightTween != null)
                {
                    highlightTween.Kill();
                    highlightTween = null;
                }
                headTransform.localScale = Vector3.one;
            }

            private void RestartHighlight()
            {
                StopHighlight();

                float scaleFactor = 1.5f;
                float duration = 0.25f;

                highlightTween= headTransform
                    .DOScale(Vector3.one * scaleFactor, duration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);

            }

        
    }

}