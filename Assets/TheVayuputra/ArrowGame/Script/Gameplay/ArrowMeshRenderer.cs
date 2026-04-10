using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace ArrowGame
{
    [RequireComponent(typeof(Arrow))]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ArrowMeshRenderer : MonoBehaviour
    {
        [Header("Line Shape")]
        public float lineWidth = 0.2f;
        public float uvTilingPerUnit = 1f; // how many repeats per world unit

        [Header("Materials")]
        public Material normalMaterial;
        public Material dashedMaterial;

        [Header("Head Renderer (separate object)")]
        public MeshRenderer headMeshRenderer;

        private Arrow arrow;
        private Mesh mesh;
        private MeshRenderer meshRenderer;

        private Color baseArrowColor;
        private Tween highlightTween;

        private Vector3 headBaseScale;

        private LevelManager levelManager => LevelManager.Instance;

        #region Unity

        private void Awake()
        {
            arrow = GetComponent<Arrow>();

            mesh = new Mesh { name = "Arrow Line Mesh" };
            GetComponent<MeshFilter>().mesh = mesh;

            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.enabled = false;

            if (headMeshRenderer != null)
                headBaseScale = headMeshRenderer.transform.localScale;
        }

        private void OnEnable()
        {
            arrow.OnArrowUpdate += HandleArrowChanged;
            arrow.OnArrowSetupEffect += HandleArrowSetupEffect;

            arrow.isMoving.AddListener(UpdateSortingOrder);
            arrow.isMarkedAsFailedEscaped.AddListener(UpdateMaterial);
            arrow.isHighlighted.AddListener(HandleHighlight);

            GameData.IsDayTheme.AddListener(OnThemeChange);

            if (headMeshRenderer != null)
                headMeshRenderer.transform.localScale = Vector3.zero;
        }

        private void OnDisable()
        {
            StopHighlight();

            arrow.OnArrowUpdate -= HandleArrowChanged;
            arrow.OnArrowSetupEffect -= HandleArrowSetupEffect;

            arrow.isMoving.RemoveListener(UpdateSortingOrder);
            arrow.isMarkedAsFailedEscaped.RemoveListener(UpdateMaterial);
            arrow.isHighlighted.RemoveListener(HandleHighlight);

            GameData.IsDayTheme.RemoveListener(OnThemeChange);
        }

        #endregion

        #region Arrow Events

        private void HandleArrowChanged()
        {
            if (!arrow.isRequiredToRender || arrow.parts == null || arrow.parts.Count < 2)
            {
                mesh.Clear();
                meshRenderer.enabled = false;

                if (headMeshRenderer != null)
                    headMeshRenderer.enabled = false;

                return;
            }

            BuildLineMesh(arrow.parts);
            meshRenderer.enabled = true;

            UpdateHeadTransform();
        }

        private void HandleArrowSetupEffect(float animTime)
        {
            StopAllCoroutines();
            StartCoroutine(DrawLineSetupEffect(animTime));
        }

        #endregion

        #region Mesh Build (Line Only)

        private void BuildLineMesh(List<Arrow.ArrowPart> parts)
        {
            mesh.Clear();

            int count = parts.Count;

            var vertices = new List<Vector3>(count * 2);
            var triangles = new List<int>((count - 1) * 6);
            var uvs = new List<Vector2>(count * 2);

            float totalLength = CalculateTotalLength(parts);
            float uvScale = totalLength * uvTilingPerUnit;

            float accumulatedLength = 0f;

            for (int i = 0; i < count; i++)
            {
                Vector3 worldPos = parts[i].position;
                Vector3 localPos = transform.InverseTransformPoint(worldPos);

                Vector3 worldForward = GetForward(parts, i);
                Vector3 localForward = transform.InverseTransformDirection(worldForward);

                Vector3 right = Vector3.Cross(Vector3.up, localForward).normalized;

                vertices.Add(localPos - right * lineWidth * 0.5f);
                vertices.Add(localPos + right * lineWidth * 0.5f);

                float v = (accumulatedLength / totalLength) * uvScale;

                uvs.Add(new Vector2(0, v));
                uvs.Add(new Vector2(1, v));

                if (i < count - 1)
                    accumulatedLength += Vector3.Distance(parts[i].position, parts[i + 1].position);
            }

            for (int i = 0; i < count - 1; i++)
            {
                int idx = i * 2;

                triangles.Add(idx);
                triangles.Add(idx + 2);
                triangles.Add(idx + 1);

                triangles.Add(idx + 1);
                triangles.Add(idx + 2);
                triangles.Add(idx + 3);
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            ApplyArrowColor(baseArrowColor);
        }

        private float CalculateTotalLength(List<Arrow.ArrowPart> parts)
        {
            float length = 0f;
            for (int i = 0; i < parts.Count - 1; i++)
                length += Vector3.Distance(parts[i].position, parts[i + 1].position);
            return Mathf.Max(length, 0.0001f);
        }

        #endregion

        #region Setup Draw Effect

        private IEnumerator DrawLineSetupEffect(float animTime)
        {
            float elapsed = 0f;
            int fullCount = arrow.parts.Count;

            meshRenderer.enabled = true;

            while (elapsed < animTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / animTime);

                int visibleCount = Mathf.Max(2, Mathf.RoundToInt(fullCount * t));
                BuildLineMesh(arrow.parts.GetRange(0, visibleCount));

                yield return null;
            }

            BuildLineMesh(arrow.parts);

            if (headMeshRenderer != null)
            {
                headMeshRenderer.transform
                    .DOScale(headBaseScale, 0.1f)
                    .SetEase(Ease.InOutQuad);
            }
        }

        #endregion

        #region Head Handling (Separate Mesh)

        private void UpdateHeadTransform()
        {
            if (headMeshRenderer == null) return;

            var parts = arrow.parts;
            int last = parts.Count - 1;

            Vector3 headWorldPos = parts[last].position;
            Vector3 headDir = (parts[last].position - parts[last - 1].position).normalized;

            headMeshRenderer.transform.position = headWorldPos;
            headMeshRenderer.transform.rotation = Quaternion.LookRotation(headDir, Vector3.up);
            headMeshRenderer.enabled = true;
        }

        #endregion

        #region Visual State

        private void UpdateSortingOrder(bool isMoving)
        {
            int order = isMoving
                ? LevelManager.arrowSortingOrderMoving
                : LevelManager.arrowSortingOrderNormal;

            meshRenderer.sortingOrder = order;

            if (headMeshRenderer != null)
                headMeshRenderer.sortingOrder = order + 1;
        }

        private void UpdateMaterial(bool isFailed)
        {
            meshRenderer.material = isFailed ? dashedMaterial : normalMaterial;
        }

        public void OnThemeChange(bool isDayTheme)
        {
            baseArrowColor = isDayTheme
                ? GameplayExtensionMethods.GetDayThemeArrowColor(arrow.colorIndex)
                : GameplayExtensionMethods.GetNightThemeArrowColor(arrow.colorIndex);

            ApplyArrowColor(baseArrowColor);
            HandleHighlight(arrow.isHighlighted.Value);
        }

        private void ApplyArrowColor(Color c)
        {
            if (meshRenderer.material != null)
                meshRenderer.material.color = c;

            if (headMeshRenderer != null && headMeshRenderer.material != null)
                headMeshRenderer.material.color = c;
        }

        #endregion

        #region Highlight

        private void HandleHighlight(bool isHighlighted)
        {
            if (isHighlighted)
                RestartHighlight();
            else
                StopHighlight();
        }

        private void RestartHighlight()
        {
            StopHighlight();

            float alpha = 0.5f;
            float duration = 0.25f;

            highlightTween = DOTween.To(
                () => meshRenderer.material.color.a,
                a =>
                {
                    Color col = baseArrowColor;
                    col.a = a;
                    meshRenderer.material.color = col;
                },
                alpha,
                duration
            )
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
        }

        private void StopHighlight()
        {
            highlightTween?.Kill();
            highlightTween = null;
            ApplyArrowColor(baseArrowColor);
        }

        #endregion

        #region Utils

        private Vector3 GetForward(List<Arrow.ArrowPart> parts, int index)
        {
            if (index == parts.Count - 1)
                return (parts[index].position - parts[index - 1].position).normalized;

            return (parts[index + 1].position - parts[index].position).normalized;
        }

        #endregion
    }
}
