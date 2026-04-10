using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheVayuputra.Core;

namespace ArrowGame
{
    public class ArrowManager : MonoBehaviour
    {
        [Tooltip("Prefab with Arrow component")]
        public Arrow arrowPrefab;

        [Tooltip("Parent transform for instantiated arrows")]
        public Transform arrowsParent;

        [Header("Arrow Speed Setting")]
        public float escapeSuccessSpeed = 50;
        public float escapeSuccessMinTime = .1f;
        public float escapeSuccessMaxTime = .5f;
        public float escapeFailedSpeed = 15;
        public float escapeFailedMinTime = .1f;
        public float escapeFailedMaxTime = .5f;

        public List<Arrow> spawnedArrows = new List<Arrow>();
        private LevelManager levelManager => LevelManager.Instance;
        public int totalArrows => spawnedArrows.Count;
        public ObservableValue<int> remandingArrow;
        public void ResetAll()
        {
            arrowsParent.DestroyAllChildImmediate();
            spawnedArrows = new List<Arrow>();
        }
        public void SetupArrowList(List<DataArrow> arrows)
        {

            ResetAll();
            int id = 0;
            foreach (var d in arrows)
            {
                Arrow a = Instantiate(arrowPrefab, arrowsParent);
                a.name = $"Arrow {id++}";
                a.SetupFromData(d);
                spawnedArrows.Add(a);
            }
            remandingArrow.Value = totalArrows;
        }
        public void PlayArrowSetupEffect(float stepTime, float maxTime)
        {
            for (int i = 0; i < spawnedArrows.Count; i++)
            {
                spawnedArrows[i].PlayArrowSetupEffect(stepTime, maxTime);
            }
        }
        public void OnArrowEscaped(Arrow arrow)
        {
            remandingArrow.Value--;
            LevelManager.Instance.CheckGameOver();
            LevelManager.Instance.EndTutorial();
        }
        public void OnFailedToEscaped(Arrow arrow)
        {
            LevelManager.Instance.DecreaseLife();
            LevelManager.Instance.CheckGameOver();
            SoundManager.Instance.PlayArrowEscapeFailedSFX();

        }
        public Arrow GetEscapedPossibleArrow()
        {
            for (int i = 0; i < spawnedArrows.Count; i++)
            {
                if (!spawnedArrows[i].isEscaped && spawnedArrows[i].CanEscape(out List<GridCell> gridPath))
                {
                    return spawnedArrows[i];
                }
            }
            return null;
        }

        public float CalculateEscapeSuccessTime(int totalStep)
        {
            float moveTime = totalStep / escapeSuccessSpeed;
            return Mathf.Clamp(moveTime, escapeSuccessMinTime, escapeSuccessMaxTime);
        }
        public float CalculateEscapeFailedTime(int totalStep)
        {
            float moveTime = totalStep / escapeFailedSpeed;
            return Mathf.Clamp(moveTime, escapeFailedMinTime, escapeFailedMaxTime);
        }

    }
}

