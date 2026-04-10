using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheVayuputra.Core;

namespace ArrowGame
{
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        public Vector2 cellSize = new Vector2(1f, 1f);
        public int sizeX = 10;
        public int sizeY = 10;

        [Header("References")]
        public GridCell cellPrefab;

        private GridCell[,] grid;
        public Dictionary<int, GridCell> idMap = new Dictionary<int, GridCell>();

        public bool includeDiagonals = true;
        #if UNITY_WEBGL
        const float emptySpaceX = 0.5f;
        const float emptySpaceY = 0.5f;
        #else
        const float emptySpaceX = 0.1f;
        const float emptySpaceY = 0.25f;
        #endif
        public int gameBorder = 15;
        private LevelManager levelManager => LevelManager.Instance;

        public void ResetAll()
        {
            idMap.Clear();
            transform.DestroyAllChildImmediate();
        }
        public void ResetCameraBorders()
        {

            float aspect = Camera.main.aspect;
            float halfX = (sizeX * 0.5f) + (sizeX * emptySpaceX);
            float halfY = (sizeY * 0.5f) + (sizeY * emptySpaceY);

            float requiredOrthoSize = Mathf.Max(halfY, halfX / aspect);


            int maxOrthographicSize = (int)Mathf.Max(2, requiredOrthoSize);
            gameBorder = (int)(Mathf.Max(maxOrthographicSize, (maxOrthographicSize * aspect)) + 1);
            levelManager.playerInputManager.SetLimits(gameBorder);
        }
        public void CreateGrid(int x, int y)
        {
            ResetAll();

            sizeX = Mathf.Max(1, x);
            sizeY = Mathf.Max(1, y);

            grid = new GridCell[sizeX, sizeY];

            int idCounter = 0;
            for (int iy = 0; iy < sizeY; iy++)
            {
                for (int ix = 0; ix < sizeX; ix++)
                {
                    GridCell cell = Instantiate(cellPrefab, transform);
                    Vector3 worldPos = GridToWorld(new Vector2Int(ix, iy));
                    cell.transform.position = worldPos;

                    cell.Init(idCounter++, new Vector2Int(ix, iy));

                    grid[ix, iy] = cell;
                    idMap[cell.cellId] = cell;
                    cell.name = cell.ToString();
                }
            }

            for (int ix = 0; ix < sizeX; ix++)
                for (int iy = 0; iy < sizeY; iy++)
                    PopulateNeighbours(grid[ix, iy]);

            ResetCameraBorders();
        }

        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            Vector3 origin = GetGridOriginWorld();
            Vector3 offset = new Vector3((gridPos.x + 0.5f) * cellSize.x, 0f, (gridPos.y + 0.5f) * cellSize.y);
            return origin + offset;
        }

        public bool WorldToGrid(Vector3 worldPos, out Vector2Int gridPos)
        {
            Vector3 origin = GetGridOriginWorld();
            Vector3 local = worldPos - origin;
            int x = Mathf.FloorToInt(local.x / cellSize.x);
            int y = Mathf.FloorToInt(local.z / cellSize.y);
            gridPos = new Vector2Int(x, y);
            if (x < 0 || x >= sizeX || y < 0 || y >= sizeY) return false;
            return true;
        }

        public GridCell GetCell(Vector2Int pos)
        {
            if (pos.x < 0 || pos.y < 0 || pos.x >= sizeX || pos.y >= sizeY) return null;
            return grid[pos.x, pos.y];
        }

        public GridCell GetCellFromWorld(Vector3 worldPos)
        {
            if (WorldToGrid(worldPos, out Vector2Int pos)) return GetCell(pos);
            return null;
        }

        private void PopulateNeighbours(GridCell cell)
        {
            cell.nearbyCells.Clear();

            Vector2Int[] directions = new Vector2Int[] {
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
        };

            if (includeDiagonals)
            {
                var diag = new Vector2Int[] {
                new Vector2Int(1,1),
                new Vector2Int(1,-1),
                new Vector2Int(-1,-1),
                new Vector2Int(-1,1)
            };
                var merged = new List<Vector2Int>(directions);
                merged.AddRange(diag);
                directions = merged.ToArray();
            }

            foreach (var d in directions)
            {
                Vector2Int neighborPos = cell.gridPos + d;
                GridCell neighbor = GetCell(neighborPos);
                if (neighbor != null)
                {
                    cell.nearbyCells.Add(new NearbyCell { direction = d, cell = neighbor });
                }
            }
        }



        private Vector3 GetGridOriginWorld()
        {
            float width = sizeX * cellSize.x;
            float height = sizeY * cellSize.y;
            Vector3 origin = transform.position - new Vector3(width * 0.5f, 0f, height * 0.5f);
            return origin;
        }

        public IEnumerator CloseGridFromCenter(float totalTime)
        {

            float centerX = (sizeX - 1) * 0.5f;
            float centerY = (sizeY - 1) * 0.5f;

            Dictionary<int, List<GridCell>> rings = new Dictionary<int, List<GridCell>>();

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    GridCell cell = grid[x, y];
                    if (cell == null) continue;

                    float dx = x - centerX;
                    float dy = y - centerY;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);

                    // Distance quantized into rings
                    int ringIndex = Mathf.RoundToInt(distance * 10f);

                    if (!rings.TryGetValue(ringIndex, out var list))
                    {
                        list = new List<GridCell>();
                        rings.Add(ringIndex, list);
                    }

                    list.Add(cell);
                }
            }

            List<int> ringKeys = new List<int>(rings.Keys);
            ringKeys.Sort();

            int ringCount = ringKeys.Count;
            if (ringCount == 0) yield break;

            float delayPerRing = totalTime / ringCount;

            foreach (int key in ringKeys)
            {
                foreach (GridCell cell in rings[key])
                {
                    cell.DisableInnerPoint();
                }

                yield return new WaitForSeconds(delayPerRing);
            }
        }


    }

}