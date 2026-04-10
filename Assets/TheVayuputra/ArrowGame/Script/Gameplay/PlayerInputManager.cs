
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.EventSystems;
using TheVayuputra.Core;
using UnityEngine.UI;
namespace ArrowGame
{

    public class PlayerInputManager : MonoBehaviour
    {
        [Header("Input blocking")]
        [SerializeField] private bool inputBlocked = false;

        [Header("Target Camera")]
        public Camera targetCamera;
        public Color dayThemeBGColor;
        public Color nightThemeBGColor;
        [Header("Reset To Center")]
        [SerializeField, Range(0f, 1f)]
        private float showResetButtonAtPercent = 0.25f; // 25%

        [Header("Movement")]
        public float moveCameraSensitivity = 2f;
        public float dragThreshold = 10f; // Pixels before panning starts
        public float zoomCameraSensitivity = 2f;
        public float smoothTime = 0.05f;

        [Header("Camera Debug")]
        [SerializeField] private float minCameraZoom = 2f;
        [SerializeField] private float maxCameraZoom = 10;
        [SerializeField] private float minCameraX = -50f;
        [SerializeField] private float maxCameraX = 50f;
        [SerializeField] private float minCameraZ = -50f;
        [SerializeField] private float maxCameraZ = 50f;
        private Vector3 targetCameraPosition;
        private Vector3 velocityCamera;
        private float targetCameraZoom;
        private Vector3 dragStartPos;
        private Vector3 lastPointerPos;
        private bool isCameraDragging;
        private bool inputStartedOverUI;
        private int lastTouchCount;

        float dpi => Screen.dpi > 0 ? Screen.dpi : 160f; // 160 = Android mdpi baseline
        public ObservableValue<bool> enableResetCameraUI = new ObservableValue<bool>(false);
        private void Awake()
        {
            targetCameraPosition = targetCamera.transform.position;
            targetCameraZoom = targetCamera.orthographicSize;
        }

        private void OnEnable()
        {
            GridCell.OnCellPointerClicked += HandleCellClicked;
            GameData.IsDayTheme.AddListener(OnThemeChange);
        }

        private void OnDisable()
        {
            GridCell.OnCellPointerClicked -= HandleCellClicked;
            GameData.IsDayTheme.RemoveListener(OnThemeChange);
        }
        public void OnThemeChange(bool isDayTheme)
        {
            if (isDayTheme)
            {
                targetCamera.backgroundColor = dayThemeBGColor;
            }
            else
            {
                targetCamera.backgroundColor = nightThemeBGColor;
            }
        }
        private void HandleCellClicked(GridCell cell)
        {
            
            if (IsPointerOverUI())
                return;

            if (cell == null) return;
            if (inputBlocked) return;

            // Prefer direct occupiedBy reference (fast)
            Arrow arrow = cell.occupiedBy;



            if (arrow != null && !arrow.isMoving.Value)
            {
                if (arrow.CanEscape(out List<GridCell> gridPath))
                {
                    arrow.EscapeArrow();
                }
                else
                {
                    arrow.FailedEscapeArrow(gridPath);
                }
            }
        }

        public void SetInputBlocked(bool blocked)
        {
            inputBlocked = blocked;
            if (zoomSlider != null)
                zoomSlider.interactable = !blocked;
        }
        void Update()
        {
            if (inputBlocked) return;

            HandleCameraMovement();
            HandleCameraZoom();
            ClampCameraPosition();
            ApplyCameraSmoothing();
            UpdateResetCameraObservable();
            UpdateZoomSlider();
        }
        bool IsPointerOverUI()
        {
            if (isUIDragging)
                return true;
            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
                return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            #else
                if (EventSystem.current == null) return false;

                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                        return true;
                }
                return false;
            #endif
        }

        void UpdateResetCameraObservable()
        {
            bool shouldEnable = IsCameraFarFromCenter();

            if (enableResetCameraUI.Value == shouldEnable)
                return; // no change → no assignment

            enableResetCameraUI.Value = shouldEnable;
        }
        bool IsCameraFarFromCenter()
        {
            Vector2 camPos = new Vector2(
                targetCameraPosition.x,
                targetCameraPosition.z
            );

            float currentDist = camPos.magnitude;

            float maxDist = Mathf.Sqrt(
                maxCameraX * maxCameraX +
                maxCameraZ * maxCameraZ
            );

            if (maxDist <= 0f)
                return false;

            float normalized = currentDist / maxDist;
            return normalized >= showResetButtonAtPercent;
        }
        void HandleCameraMovement()
        {
            Vector2 currentPointerPos;
            bool pointerDown = false;
            bool pointerHeld = false;
            bool pointerUp = false;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            currentPointerPos = Input.mousePosition;
            pointerDown = Input.GetMouseButtonDown(0);
            pointerHeld = Input.GetMouseButton(0);
            pointerUp = Input.GetMouseButtonUp(0);
#else
            int touchCount = Input.touchCount;
            if (touchCount != 1) {
                if (touchCount == 0) inputStartedOverUI = false;
                lastTouchCount = touchCount;
                return;
            }

            Touch t = Input.GetTouch(0);
            currentPointerPos = t.position;

            // If we just transitioned to 1 touch (from 0 or 2+), reset drag state to avoid "jumping"
            if (lastTouchCount != 1)
            {
                dragStartPos = currentPointerPos;
                lastPointerPos = currentPointerPos;
                isCameraDragging = false;
                inputStartedOverUI = IsPointerOverUI();
            }
            lastTouchCount = 1;

            pointerDown = t.phase == TouchPhase.Began;
            pointerHeld = t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary;
            pointerUp = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
#endif

            if (pointerDown)
            {
                inputStartedOverUI = IsPointerOverUI();
            }

            if (inputStartedOverUI)
            {
                if (pointerUp) inputStartedOverUI = false;
                return;
            }

            if (pointerDown)
            {
                dragStartPos = currentPointerPos;
                lastPointerPos = currentPointerPos;
                isCameraDragging = false;
            }

            if (pointerHeld)
            {
                float dist = Vector2.Distance(currentPointerPos, dragStartPos);

                if (!isCameraDragging && dist >= dragThreshold)
                    isCameraDragging = true;

                if (isCameraDragging)
                {
                    Vector3 viewportDelta =
                        targetCamera.ScreenToViewportPoint(lastPointerPos) -
                        targetCamera.ScreenToViewportPoint(currentPointerPos);

                    float zoomScale = targetCamera.orthographicSize * 2f;

                    Vector3 move = new Vector3(
                        viewportDelta.x * zoomScale,
                        0f,
                        viewportDelta.y * zoomScale
                    );

                    targetCameraPosition += move * moveCameraSensitivity;

                    lastPointerPos = currentPointerPos;
                }

            }

            if (pointerUp)
            {
                isCameraDragging = false;
            }
        }

        void HandleCameraZoom()
        {
            if (IsPointerOverUI())
                return;

            float zoomDelta = 0f;

            // Mouse scroll (editor / desktop)
            zoomDelta = Input.mouseScrollDelta.y;

            // Pinch zoom (mobile)
            if (Input.touchCount == 2)
            {
                Touch a = Input.GetTouch(0);
                Touch b = Input.GetTouch(1);

                float prevDist = (a.position - a.deltaPosition -
                                (b.position - b.deltaPosition)).magnitude;

                float currDist = (a.position - b.position).magnitude;

                // ✅ DPI normalized (hardware independent)
                zoomDelta = (currDist - prevDist) / dpi;
            }

            if (Mathf.Abs(zoomDelta) > 0.0001f)
            {
                float previousZoom = targetCameraZoom;

                // 🔑 SCALE BY CURRENT ZOOM
                float zoomScale = Mathf.Clamp(targetCameraZoom, 1f, 8f);


                targetCameraZoom -= zoomDelta * zoomCameraSensitivity * zoomScale;
                targetCameraZoom = Mathf.Clamp(
                    targetCameraZoom,
                    minCameraZoom,
                    maxCameraZoom
                );

                ZoomTowardsPoint(previousZoom);
            }
        }

        void ZoomTowardsPoint(float previousZoom)
        {
            Vector3 focusPoint;

            // Mouse
            if (Input.touchCount < 2)
            {
                focusPoint = targetCamera.ScreenToWorldPoint(Input.mousePosition);
            }
            else
            {
                // Pinch midpoint
                Touch a = Input.GetTouch(0);
                Touch b = Input.GetTouch(1);
                Vector2 mid = (a.position + b.position) * 0.5f;
                focusPoint = targetCamera.ScreenToWorldPoint(mid);
            }

            float zoomFactor = targetCameraZoom / previousZoom;
            Vector3 dir = focusPoint - targetCameraPosition;
            dir.y = 0; // Ensure no vertical drift

            targetCameraPosition += dir * (1f - zoomFactor);
        }
        void ZoomFromCenter(float previousZoom)
        {
            // Center zoom for ortho doesn't require position changes if using targetCameraPosition as reference
            // This method is kept for logical consistency but won't shift position
            Vector3 focusPoint = targetCameraPosition;
            
            float zoomFactor = targetCameraZoom / previousZoom;

            Vector3 dir = focusPoint - targetCameraPosition;
            targetCameraPosition += dir * (1f - zoomFactor);
        }

        void RecalculateMaxZoom()
        {
            float halfWidth = (maxCameraX - minCameraX) * 0.5f;
            float halfHeight = (maxCameraZ - minCameraZ) * 0.5f;

            float maxZoomX = halfWidth / targetCamera.aspect;
            float maxZoomZ = halfHeight;

            maxCameraZoom = Mathf.Max(minCameraZoom, Mathf.Min(maxZoomX, maxZoomZ));
            targetCameraZoom = Mathf.Clamp(targetCameraZoom, minCameraZoom, maxCameraZoom);
        }

        void ClampCameraPosition()
        {
            float camHeight = targetCameraZoom;
            float camWidth = camHeight * targetCamera.aspect;

            targetCameraPosition.x = Mathf.Clamp(
                targetCameraPosition.x,
                minCameraX + camWidth,
                maxCameraX - camWidth
            );

            targetCameraPosition.z = Mathf.Clamp(
                targetCameraPosition.z,
                minCameraZ + camHeight,
                maxCameraZ - camHeight
            );
        }

        void ApplyCameraSmoothing()
        {
            targetCamera.transform.position = Vector3.SmoothDamp(
                targetCamera.transform.position,
                targetCameraPosition,
                ref velocityCamera,
                smoothTime
            );

            targetCamera.orthographicSize = Mathf.Lerp(
                targetCamera.orthographicSize,
                targetCameraZoom,
                Time.deltaTime * 10f
            );
        }

        public void SetLimits(int gameBorder)
        {
            minCameraX = -gameBorder;
            maxCameraX = gameBorder;

            minCameraZ = -gameBorder;
            maxCameraZ = gameBorder;
            RecalculateMaxZoom();
            targetCameraZoom = Mathf.Lerp(minCameraZoom, maxCameraZoom, .5f);
            targetCameraPosition.x = 0;
            targetCameraPosition.z = 0;
        }
        Vector3 GetNearestValidCameraPosition(Vector3 desiredPos)
        {
            float camHeight = targetCameraZoom;
            float camWidth = camHeight * targetCamera.aspect;

            desiredPos.x = Mathf.Clamp(
                desiredPos.x,
                minCameraX + camWidth,
                maxCameraX - camWidth
            );

            desiredPos.z = Mathf.Clamp(
                desiredPos.z,
                minCameraZ + camHeight,
                maxCameraZ - camHeight
            );

            return desiredPos;
        }
        public void GetCameraFocusAtPosition(Vector3 position, float moveTime = 0.3f)
        {
            if (targetCamera == null) return;

            SetInputBlocked(true);

            // Calculate offset to center target in viewport
            Vector3 viewportPos = targetCamera.WorldToViewportPoint(position);
            Vector3 viewportCenter = new Vector3(0.5f, 0.5f, viewportPos.z);

            Vector3 worldCenter = targetCamera.ViewportToWorldPoint(viewportCenter);
            Vector3 offset = position - worldCenter;

            Vector3 desiredTargetPos = targetCameraPosition + offset;
            desiredTargetPos.y = targetCameraPosition.y;

            // 🔑 Clamp BEFORE tweening
            Vector3 clampedTargetPos = GetNearestValidCameraPosition(desiredTargetPos);

            // Kill existing tween
            targetCamera.transform.DOKill();

            targetCamera.transform
                .DOMove(clampedTargetPos, moveTime)
                .SetEase(Ease.OutCubic)
                .OnUpdate(() =>
                {
                    targetCameraPosition = targetCamera.transform.position;
                })
                .OnComplete(() =>
                {
                    // Ensure internal state is perfectly synced
                    targetCameraPosition = clampedTargetPos;
                    SetInputBlocked(false);
                });
        }
        public void LevelStartAnimation(float animTime)
        {
            bool previousInputState = inputBlocked;
            SetInputBlocked(true);
            targetCamera.transform.DOKill();
            DOTween.Kill(targetCamera);

            // Center position (keep Y as-is)
            Vector3 centerPos = new Vector3(0f, targetCamera.transform.position.y, 0f);
            targetCamera.orthographicSize = maxCameraZoom * .7f;
            float zoomOutValue = maxCameraZoom;    // then zoom out to max       
            Sequence seq = DOTween.Sequence();
            // Move quickly to center
            seq.Join(targetCamera.transform.DOMove(centerPos, animTime).SetEase(Ease.OutCubic).OnUpdate(() =>
                    {
                        targetCameraPosition = targetCamera.transform.position;
                    })
            );
            // Zoom OUT to max
            seq.Join(
                DOTween.To(
                    () => targetCamera.orthographicSize,
                    x =>
                    {
                        targetCamera.orthographicSize = x;
                        targetCameraZoom = x;
                    },
                    zoomOutValue,
                    animTime
                ).SetEase(Ease.OutCubic)
            );

            seq.OnComplete(() =>
            {
                // Final sync
                targetCameraPosition = centerPos;
                targetCameraZoom = zoomOutValue;
                SetInputBlocked(previousInputState);

            });
        }
        public void LevelCompletedAnimation(float animTime)
        {
            bool previousInputState = inputBlocked;
            SetInputBlocked(true);
            targetCamera.transform.DOKill();
            DOTween.Kill(targetCamera);
            // Center position (keep Y as-is)
            Vector3 centerPos = new Vector3(0f, targetCamera.transform.position.y, 0f);
            // Zoom values
            float zoomInValue = maxCameraZoom * .7f;
            Sequence seq = DOTween.Sequence();
            // Move quickly to center

            seq.Join(targetCamera.transform.DOMove(centerPos, animTime).SetEase(Ease.OutCubic).OnUpdate(() =>
                    {
                        targetCameraPosition = targetCamera.transform.position;
                    })
            );
            // Zoom IN
            seq.Join(DOTween.To(() => targetCamera.orthographicSize, x =>
                    {
                        targetCamera.orthographicSize = x;
                        targetCameraZoom = x;
                    },
                    zoomInValue,
                    animTime
                ).SetEase(Ease.InCubic)
            );
            seq.OnComplete(() =>
            {
                // Final sync
                targetCameraPosition = centerPos;
                targetCameraZoom = zoomInValue;
                SetInputBlocked(previousInputState);

            });
        }
        
        private Slider zoomSlider;
        private bool ignoreSliderCallback;
        private bool isUIDragging;
        public void SetupZoomSlider(Slider slider)
        {
            zoomSlider = slider;

            zoomSlider.minValue = 0f;
            zoomSlider.maxValue = 1f;
            zoomSlider.wholeNumbers = false;

            ignoreSliderCallback = true;
            zoomSlider.value = GetNormalizedZoom(targetCameraZoom);
            ignoreSliderCallback = false;
            zoomSlider.onValueChanged.AddListener(OnZoomSliderChanged);
            var trigger = zoomSlider.gameObject.AddComponent<EventTrigger>();

        AddEvent(trigger, EventTriggerType.PointerDown, () => isUIDragging = true);
        AddEvent(trigger, EventTriggerType.PointerUp, () => isUIDragging = false);
        AddEvent(trigger, EventTriggerType.EndDrag, () => isUIDragging = false);
        }
        void AddEvent(EventTrigger trigger, EventTriggerType type, Action action)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = type;
            entry.callback.AddListener((_) => action());
            trigger.triggers.Add(entry);
        }
        void OnZoomSliderChanged(float sliderValue)
        {
            if (ignoreSliderCallback) return;
            if (inputBlocked) return;

            targetCameraZoom = GetZoomFromNormalized(sliderValue);
            targetCameraZoom = Mathf.Clamp(targetCameraZoom, minCameraZoom, maxCameraZoom);

            // Zooming from center slider shouldn't move the camera in an ortho setup
            // This avoids the drift and "jumping" issue reported.
        }
        void UpdateZoomSlider()
        {
            if (zoomSlider == null) return;

            ignoreSliderCallback = true;
            zoomSlider.value = GetNormalizedZoom(targetCameraZoom);
            ignoreSliderCallback = false;
        }
        float GetNormalizedZoom(float zoom)
        {
            if (maxCameraZoom <= minCameraZoom) return 0f;
            return 1-Mathf.InverseLerp(minCameraZoom, maxCameraZoom, zoom);
        }

        float GetZoomFromNormalized(float normalized)
        {
            return Mathf.Lerp(minCameraZoom, maxCameraZoom,1- normalized);
        }


    }
}