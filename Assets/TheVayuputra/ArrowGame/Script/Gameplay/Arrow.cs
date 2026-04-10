using System;
using System.Collections.Generic;
using UnityEngine;
using TheVayuputra.Core;

namespace ArrowGame
{
    public class Arrow : MonoBehaviour
    {
        [System.Serializable]
        public class ArrowPart
        {
            public Vector3 position;
            public Quaternion rotation;
        }

        [Header("Arrow Part Setting")]
        public float maxDistance = 0.25f;
        public float cornerRadius = 0.25f;

        public List<ArrowPart> parts = new List<ArrowPart>();

        [Tooltip("Direction in grid space. e.g. (1,0) to the right, (0,1) up")]
        public Vector2Int arrowDirection
        {
            get
            {
                return _direction;
            }
            set
            {
                _direction = value;
                arrowAngle = Mathf.Atan2(_direction.x, _direction.y) * Mathf.Rad2Deg;
            }
        }
        public float arrowAngle { get; private set; }
        private Vector2Int _direction = Vector2Int.up;
        [Tooltip("Ordered list of cells the arrow occupies. Index 0 is the start cell.")]
        public List<GridCell> placedCells = new List<GridCell>();
        public GridCell headCell => arrowLength > 0 ? placedCells[0] : null;
        public GridCell tailCell => arrowLength > 0 ? placedCells[^1] : null;
        public int arrowLength => placedCells.Count;

        public bool isEscaped { get; private set; } = false;
        public bool isRequiredToRender { get; private set; } = true;
        public ObservableValue<bool> isMoving = new ObservableValue<bool>(false);
        public ObservableValue<bool> isMarkedAsFailedEscaped = new ObservableValue<bool>(false);
        public ObservableValue<bool> isHighlighted = new ObservableValue<bool>(false);
        public int colorIndex;
        public event Action OnArrowUpdate;
        public event Action<float> OnArrowSetupEffect;
        private LevelManager levelManager => LevelManager.Instance;

        public void SetupFromData(DataArrow data)
        {

            isHighlighted.Value = false;
            if (data.Indices != null && data.Indices.Count >= 2)
            {
                placedCells.Clear();

                for (int i = 0; i < data.Indices.Count; i++)
                {
                    int idx = data.Indices[i];
                    var cell = levelManager.gridManager.idMap[idx];
                    placedCells.Add(cell);
                }

                arrowDirection = placedCells[0].gridPos - placedCells[1].gridPos;
                transform.position = placedCells[0].transform.position;
                transform.rotation = Quaternion.Euler(0f, arrowAngle, 0f);

                colorIndex = data.ColorIndex;

                //set on grid
                for (int i = 0; i < arrowLength; i++)
                {
                    placedCells[i].SetOccupied(this);
                    // placedCells[i].SetInnerPointEnable(arrowColor.Value);
                }
            }

            GenerateSmoothPathOfPlacedCells();

        }


        public void GenerateSmoothPathOfPlacedCells()
        {
            if (placedCells == null || arrowLength < 2)
            {
                parts.Clear();
                return;
            }

            List<Vector3> cellPos = new List<Vector3>(arrowLength);
            foreach (var c in placedCells)
            {
                if (c != null && c.transform != null)
                    cellPos.Add(c.transform.position);
                else
                    cellPos.Add(Vector3.zero);
            }

            GenerateSmoothPath(cellPos);
        }

        public void GenerateSmoothPath(List<Vector3> cellPos)
        {
            parts.Clear();
            if (cellPos == null || cellPos.Count < 2) return;

            // cellPos.ForEach(x =>
            // {
            //    parts.Add(new ArrowPart(){position=x,rotation=Quaternion.identity}) ;
            // });
            // return;

            int segCount = cellPos.Count - 1;

            // per-segment start/end
            var segStart = new Vector3[segCount];
            var segEnd = new Vector3[segCount];
            for (int i = 0; i < segCount; i++)
            {
                segStart[i] = cellPos[i];
                segEnd[i] = cellPos[i + 1];
            }

            // corner trimming
            var hasCorner = new bool[cellPos.Count];
            var cornerP = new Vector3[cellPos.Count];
            var cornerQ = new Vector3[cellPos.Count];

            const float EPS = 1e-6f;
            if (cornerRadius > 0f && cellPos.Count >= 3)
            {
                for (int j = 1; j < cellPos.Count - 1; j++)
                {
                    Vector3 A = cellPos[j - 1];
                    Vector3 B = cellPos[j];
                    Vector3 C = cellPos[j + 1];

                    Vector3 dirAB = B - A;
                    Vector3 dirBC = C - B;
                    float lenAB = dirAB.magnitude;
                    float lenBC = dirBC.magnitude;
                    if (lenAB < EPS || lenBC < EPS) continue;

                    float angle = Vector3.Angle(dirAB, dirBC);
                    // skip nearly straight lines
                    if (Mathf.Abs(angle) < 0.5f || Mathf.Abs(angle - 180f) < 0.5f) continue;

                    float r = Mathf.Min(cornerRadius, lenAB * 0.5f, lenBC * 0.5f);
                    if (r <= EPS) continue;

                    Vector3 p = B - dirAB.normalized * r;
                    Vector3 q = B + dirBC.normalized * r;

                    // if p and q collapse (too close) skip corner to avoid degenerate arc
                    if ((p - q).sqrMagnitude < (EPS * EPS)) continue;

                    // apply trimmed endpoints
                    segEnd[j - 1] = p;
                    segStart[j] = q;

                    hasCorner[j] = true;
                    cornerP[j] = p;
                    cornerQ[j] = q;
                }
            }

            // Build polyline (linear segments + corner arcs)
            List<Vector3> poly = new List<Vector3>();
            // start from first segment's start (head if head-first)
            poly.Add(segStart[0]);

            // helper: quadratic bezier
            Vector3 QuadBezier(Vector3 P, Vector3 B, Vector3 Q, float t)
            {
                float u = 1f - t;
                return u * u * P + 2f * u * t * B + t * t * Q;
            }

            // subdiv parameters
            float subdivLen = Mathf.Max(0.0001f, maxDistance * 0.25f);

            for (int i = 0; i < segCount; i++)
            {
                Vector3 a = segStart[i];
                Vector3 b = segEnd[i];
                float lineLen = Vector3.Distance(a, b);

                if (lineLen > EPS)
                {
                    int lineSubs = Mathf.Max(1, Mathf.CeilToInt(lineLen / subdivLen));
                    for (int s = 1; s <= lineSubs; s++)
                    {
                        float t = (float)s / (float)lineSubs;
                        Vector3 candidate = Vector3.Lerp(a, b, t);
                        if ((candidate - poly[poly.Count - 1]).sqrMagnitude > 1e-8f)
                            poly.Add(candidate);
                    }
                }
                // else: degenerate linear segment; skip

                int junctionIndex = i + 1;
                if (junctionIndex >= 1 && junctionIndex < cellPos.Count - 1 && hasCorner[junctionIndex])
                {
                    Vector3 P = cornerP[junctionIndex];
                    Vector3 Q = cornerQ[junctionIndex];
                    Vector3 B = cellPos[junctionIndex];

                    // estimate arc length (sample)
                    int estimateSamples = 12;
                    float arcLen = 0f;
                    Vector3 prev = P;
                    for (int k = 1; k <= estimateSamples; k++)
                    {
                        float tt = (float)k / (float)estimateSamples;
                        Vector3 pt = QuadBezier(P, B, Q, tt);
                        arcLen += Vector3.Distance(prev, pt);
                        prev = pt;
                    }

                    if (arcLen > EPS)
                    {
                        int arcSubs = Mathf.Max(1, Mathf.CeilToInt(arcLen / subdivLen));
                        for (int k = 1; k <= arcSubs; k++)
                        {
                            float tt = (float)k / (float)arcSubs;
                            Vector3 candidate = QuadBezier(P, B, Q, tt);
                            if ((candidate - poly[poly.Count - 1]).sqrMagnitude > 1e-8f)
                                poly.Add(candidate);
                        }
                    }
                }
            }

            if (poly.Count < 2) return;

            // Remove near-duplicate consecutive points (defensive)
            for (int i = poly.Count - 1; i > 0; i--)
            {
                if ((poly[i] - poly[i - 1]).sqrMagnitude < 1e-10f)
                    poly.RemoveAt(i);
            }
            if (poly.Count < 2) return;

            // cumulative lengths
            List<float> cum = new List<float>(poly.Count);
            cum.Add(0f);
            for (int i = 1; i < poly.Count; i++)
                cum.Add(cum[i - 1] + Vector3.Distance(poly[i], poly[i - 1]));

            float totalLength = cum[cum.Count - 1];
            if (totalLength <= 1e-6f) return;

            // number of parts so spacing <= maxDistance and both ends included
            int numParts = Mathf.Max(2, Mathf.CeilToInt(totalLength / Mathf.Max(1e-6f, maxDistance)) + 1);
            float spacing = totalLength / (numParts - 1);

            // helper: binary search sample
            Vector3 SamplePointAtDistance(float target)
            {
                if (target <= 0f) return poly[0];
                if (target >= totalLength) return poly[poly.Count - 1];

                // binary search
                int low = 0, high = cum.Count - 1;
                while (low < high)
                {
                    int mid = (low + high) / 2;
                    if (cum[mid] <= target) low = mid + 1;
                    else high = mid;
                }
                int idx = Mathf.Max(0, low - 1);
                float segStartLen = cum[idx];
                float segLen = cum[idx + 1] - segStartLen;
                if (segLen <= 1e-8f) return poly[idx];
                float localT = (target - segStartLen) / segLen;
                return Vector3.Lerp(poly[idx], poly[idx + 1], localT);
            }

            // sample
            for (int pidx = 0; pidx < numParts; pidx++)
            {
                float targetDist = pidx * spacing;
                Vector3 pos = SamplePointAtDistance(targetDist);
                parts.Add(new ArrowPart { position = pos, rotation = Quaternion.identity });
            }

            // rotations
            Quaternion firstRot = Quaternion.identity;
            Vector3 dirWorld = new Vector3(arrowDirection.x, 0f, arrowDirection.y);
            if (dirWorld.sqrMagnitude > 1e-6f) firstRot = Quaternion.LookRotation(dirWorld.normalized, Vector3.up);

            if (parts.Count > 0)
            {
                parts[0].rotation = firstRot;
                for (int i = 1; i < parts.Count; i++)
                {
                    Vector3 toPrev = parts[i - 1].position - parts[i].position;
                    if (toPrev.sqrMagnitude <= 1e-6f)
                        parts[i].rotation = parts[i - 1].rotation;
                    else
                        parts[i].rotation = Quaternion.LookRotation(toPrev.normalized, Vector3.up);
                }
            }
        }



        public void UpdateView()
        {
            if (parts.Count > 0)
            {
                transform.position = parts[0].position;
                transform.rotation = parts[0].rotation;
            }
            OnArrowUpdate?.Invoke();
        }
        public void PlayArrowSetupEffect(float stepTime, float maxTime)
        {
            float moveTime = Mathf.Clamp(arrowLength * stepTime, .05f, maxTime);
            OnArrowSetupEffect?.Invoke(moveTime);
        }
        public bool CanEscape(out List<GridCell> gridPath)
        {
            gridPath = new List<GridCell>();

            GridCell head = placedCells[0];

            Vector2Int pos = head.gridPos;
            int safety = 0;
            while (safety++ < 10000)
            {
                pos += arrowDirection;
                GridCell next = levelManager.gridManager.GetCell(pos);
                if (next == null) return true;
                gridPath.Add(next);
                if (next.IsOccupied()) return false;
            }

            return false;
        }
        public void FailedEscapeArrow(List<GridCell> gridPath)
        {
            int totalStep = gridPath.Count * 2;
            float moveTime = levelManager.arrowManager.CalculateEscapeFailedTime(totalStep);
            isMoving.Value = true;
            StartCoroutine(GameplayExtensionMethods.MovePositionsPingPong(
               placedCells.ToWorldPositionList(),
               gridPath.ToWorldPositionList(),
               moveTime,
               (path) =>
               {
                   GenerateSmoothPath(path);
                   UpdateView();
               }, null, null,
               (path) =>
               {
                   isMoving.Value = false;
                   if (!isMarkedAsFailedEscaped.Value)
                   {
                       levelManager.arrowManager.OnFailedToEscaped(this);
                       isMarkedAsFailedEscaped.Value = true;
                   }
                   GenerateSmoothPath(path);
                   UpdateView();
               }
           ));
        }
        public void EscapeArrow()
        {
            int totalStep = arrowLength + levelManager.gridManager.gameBorder * 2;
            float moveTime = levelManager.arrowManager.CalculateEscapeSuccessTime(totalStep);
            isMoving.Value = true;
            for (int i = 0; i < arrowLength; i++)
            {
                placedCells[i].SetEmpty();
            }
            isHighlighted.Value = false;
            SoundManager.Instance.PlayArrowEscapeSuccessSFX();
            StartCoroutine(GameplayExtensionMethods.MovePositionsInDirection(
                placedCells.ToWorldPositionList(),
                new Vector3(arrowDirection.x, 0f, arrowDirection.y),
                totalStep,
                moveTime,
                (path) =>
                {
                    GenerateSmoothPath(path);
                    UpdateView();
                },

                (pos, fromHead) =>
                {
                    var rc = levelManager.gridManager.GetCellFromWorld(pos);
                    if (rc)
                    {
                        if (placedCells.Remove(rc))
                        {
                            rc.EnableInnerPoint();
                        }
                    }
                },
                null
                , (lastPost) =>
                {
                    isMoving.Value = false;
                    isRequiredToRender = false;
                    UpdateView();
                }
            ));
            levelManager.arrowManager.OnArrowEscaped(this);
            isEscaped = true;
        }

        /// <summary>
        private void OnDrawGizmosSelected()
        {
            if (parts == null || parts.Count == 0) return;

            Color partColor = new Color(0.0f, 0.75f, 1.0f, 0.9f);
            Color dirColor = Color.yellow;
            float sphereSize = 0.08f;
            float dirLength = 0.25f;

            for (int i = 0; i < parts.Count; i++)
            {
                var ap = parts[i];
                if (ap == null) continue;

                Vector3 pos = ap.position + Vector3.up * 0.05f; // lift slightly so spheres sit above ground

                Gizmos.color = partColor;
                Gizmos.DrawSphere(pos, sphereSize);

                Gizmos.color = dirColor;
                Vector3 forward = ap.rotation * Vector3.forward;
                Gizmos.DrawRay(pos, forward * dirLength);
            }
        }
    }



    [System.Serializable]
    public class DataArrow
    {
        public List<int> Indices = new List<int>();
        public int ColorIndex;
    }
}