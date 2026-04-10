using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ArrowGame
{
    public static class GameplayExtensionMethods
    {
        /// <summary>
        /// Build nextPositions and run MovePositionsInSteps with fromHead option.
        /// </summary>
        public static IEnumerator MovePositionsInDirection(
            List<Vector3> positions,
            Vector3 direction,
            int totalStepToMove,
            float totalMoveTime,
            Action<List<Vector3>> onUpdateList = null,
            Action<Vector3,bool> onPositionRemove = null,
            Action<Vector3,bool> onPositionAdd = null,
            Action<List<Vector3>> onCompleted = null,
            float gridSize = 1f
            )
        {
        
            var nextPositions = new List<Vector3>(totalStepToMove);
            Vector3 lastHead = positions[0];
            for (int step = 0; step < totalStepToMove; step++)
            {
                lastHead += direction * gridSize;
                nextPositions.Add(lastHead);
            }
        

            yield return MovePositionsInSteps(
                positions,
                nextPositions,
                totalMoveTime,
                onUpdateList,
                onPositionRemove,
                onPositionAdd,
                onCompleted
            );
        }

        /// <summary>
        /// Animate steps either from head or from tail depending on fromHead flag.
        /// </summary>
        public static IEnumerator MovePositionsInSteps(
            List<Vector3> positions,
            List<Vector3> nextPositions,
            float totalMoveTime,
            Action<List<Vector3>> onUpdateList = null,
            Action<Vector3,bool> onPositionRemove = null,
            Action<Vector3,bool> onPositionAdd = null,
            Action<List<Vector3>> onCompleted = null,
            bool fromHead = true)
        {

            float stepMoveTime = (totalMoveTime > 0f) ? totalMoveTime / (float)nextPositions.Count : 0f;
            List<Vector3> buffer=new List<Vector3>();  

            Vector3 nextPoint = nextPositions[0];
            int bufferLastIndex = positions.Count;
            int bufferSecondLastIndex =  positions.Count-1;
            int positionLastIndex =  positions.Count -1 ;
            int positionSecondLastIndex =  positions.Count -2 ;

            float elapsed = 0f;
            if(fromHead)
            {   
                for (int step = 0; step < nextPositions.Count; step++)
                {
                    nextPoint = nextPositions[step];
                    buffer.Clear();                
                    buffer.Add(positions[0]);
                    buffer.AddRange(positions);
                    elapsed = 0f;           

                    while (elapsed < stepMoveTime)
                    {
                        elapsed += Time.deltaTime;
                        float t = Mathf.Clamp01(elapsed / stepMoveTime);
                        buffer[0] = Vector3.Lerp(positions[0], nextPoint, t);
                        buffer[bufferLastIndex] = Vector3.Lerp(positions[positionLastIndex], positions[positionSecondLastIndex], t);
                        onUpdateList?.Invoke(buffer);
                        yield return null;
                    }

                    Vector3 removed = positions[positionLastIndex];
                    positions.RemoveAt(positionLastIndex);
                    positions.Insert(0,nextPoint);
                    onPositionRemove?.Invoke(removed,fromHead);
                    onPositionAdd?.Invoke(nextPoint,fromHead);
                    onUpdateList?.Invoke(positions);
                }      
            }
            else  
            {   
                for (int step = 0; step < nextPositions.Count; step++)
                {
                    nextPoint = nextPositions[step];
                    buffer.Clear();                
                    buffer.AddRange(positions);
                    buffer.Add(positions[positionLastIndex]);

                    elapsed = 0f;

                    while (elapsed < stepMoveTime)
                    {
                        elapsed += Time.deltaTime;
                        float t = Mathf.Clamp01(elapsed / stepMoveTime);

                        buffer[0] = Vector3.Lerp(positions[0], positions[1], t);
                        buffer[bufferLastIndex] = Vector3.Lerp(positions[positionLastIndex], nextPoint, t);
                        onUpdateList?.Invoke(buffer);
                        yield return null;
                    }
                    Vector3 removed = positions[0];
                    positions.Add(nextPoint);     
                    positions.RemoveAt(0);     
                    onPositionRemove?.Invoke(removed, fromHead);
                    onPositionAdd?.Invoke(nextPoint, fromHead);
                    onUpdateList?.Invoke(positions);
                }
            }
            onCompleted?.Invoke(positions);
        }
    public static IEnumerator MovePositionsPingPong(
        List<Vector3> positions,
        List<Vector3> nextPositions,
        float totalMoveTime,
        Action<List<Vector3>> onUpdateList = null,
        Action<Vector3,bool> onPositionRemove = null,
        Action<Vector3,bool> onPositionAdd = null,
        Action<List<Vector3>> onCompleted = null)
        {
            List<Vector3> reversePath=new List<Vector3>();
            List<Vector3> lastPosition=new List<Vector3>();
            Action<Vector3,bool> removeAction = (a,fromHead) =>
            {
                reversePath.Insert(0,a);
                onPositionRemove?.Invoke(a,fromHead);
            
            };
                // Forward: head-driven

                yield return MovePositionsInSteps(
                    positions,
                    nextPositions,
                    totalMoveTime*.5f,
                    onUpdateList,
                    removeAction,
                    onPositionAdd,
                    (lastPosition)=>{positions=new List<Vector3>(lastPosition);},    
                    true
                );
                
                yield return MovePositionsInSteps(
                    positions,
                    reversePath,
                    totalMoveTime*.5f,
                    onUpdateList,
                    onPositionRemove,
                    onPositionAdd,
                    onCompleted,   
                    false
                );


        }

        public static List<Vector3> ToWorldPositionList(this List<GridCell> path)
        {
            List<Vector3> list=new List<Vector3>();
            path.ForEach(x=>list.Add(x.transform.position));
            return list;
        }
        public static Color GetDayThemeArrowColor(int colorIndex)
        {            
            return Color.black;           
        }
        public static Color GetNightThemeArrowColor(int colorIndex)
        {            
            return  LevelManager.Instance.GetArrowColor(colorIndex);           
        }
    }
}